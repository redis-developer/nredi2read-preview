using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NRedi2Read.Helpers
{
    public static class RedisHelper
    {
        /// <summary>
        /// converts an object to a set of hash entries for consumption by the StackeExchange libraries SetHash methods
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerable<HashEntry> AsHashEntries(this object obj, string propertyNamePrefix= "")
        {
            var properties = obj
                .GetType()
                .GetProperties()
                .Where(x => x.GetValue(obj) != null);
            foreach (var property in properties)
            {
                var redisFieldInfo = property.CustomAttributes
                    .FirstOrDefault(x => x.AttributeType.Name == nameof(RedisHashFieldAttribute));
                var propertyName = property.Name;

                if (redisFieldInfo != null)
                {
                    propertyName = redisFieldInfo.ConstructorArguments[0].Value.ToString();
                }

                var propertyValue = property.GetValue(obj);
                if (property.PropertyType.GetInterfaces().Contains(typeof(IEnumerable<string>)))
                {
                    var enumerable = (propertyValue as IEnumerable<object>).ToArray();
                    for (var i = 0; i < enumerable.Count(); i++)
                    {
                        var name = $"{propertyNamePrefix}{propertyName}.[{i}]";
                        yield return new HashEntry(name, enumerable[i].ToString());
                    }
                }
                else
                {
                    yield return new HashEntry($"{propertyNamePrefix}{propertyName}", propertyValue.ToString());
                }
            }
        }
        
        /// <summary>
        /// Converts a set of hash entries to a POCO
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashEntries"></param>
        /// <returns></returns>
        public static T ConvertFromRedis<T>(HashEntry[] hashEntries)
        {
            var properties = typeof(T).GetProperties();
            var obj = Activator.CreateInstance(typeof(T));
            foreach (var property in properties)
            {
                var redisFieldInfo = property.CustomAttributes
                    .FirstOrDefault(x => x.AttributeType.Name == nameof(RedisHashFieldAttribute));
                var redisPropertyName = property.Name;

                if (redisFieldInfo != null)
                {
                    redisPropertyName = redisFieldInfo.ConstructorArguments[0].Value.ToString();
                }

                HashEntry entry = hashEntries.FirstOrDefault(g => g.Name.ToString().Equals(redisPropertyName));

                if (entry.Equals(new HashEntry())) continue;
                if (property.PropertyType == typeof(String[]))
                {
                    String[] blah = new []{entry.Value.ToString()};
                    property.SetValue(obj, Convert.ChangeType(blah, property.PropertyType));
                }
                else 
                { 
                    property.SetValue(obj, Convert.ChangeType(entry.Value.ToString(), property.PropertyType));
                }
            }
            return (T)obj;
        }
    }
}

