using NRediSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NRedi2Read.Helpers
{
    public static class NRedisSearchExtensions
    {        
        static Dictionary<PropertyInfo, CustomAttributeData> _attributeCache = new Dictionary<PropertyInfo, CustomAttributeData>();

        /// <summary>
        /// Extension method to allow a set of search results from a hashes to be converted nicely into POCOs
        /// Use of this method assumes that <see cref="RedisHashFieldAttribute"/> has been used to 
        /// decorate all the pertinent POCO fields
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        public static IList<T> AsList<T>(this SearchResult result) where T : class
        {
            var t = typeof(T);
            var list = new List<T>();

            var properties = typeof(T) 
                .GetProperties();

            //itterate over all the documents in the result, each one of them shoud map to the supplied type
            foreach (var document in result.Documents)
            {
                // initalize an instance of our provided class
                var obj = (T)Activator.CreateInstance(t);

                //itterate over each property in the type
                foreach (var property in properties)
                {
                    CustomAttributeData redisFieldInfo = null;

                    //check our propety cache to see if we can skip searching for the redis field info for this class
                    if (_attributeCache.ContainsKey(property))
                    {
                        redisFieldInfo = _attributeCache[property];
                    }
                    else
                    {
                        // pull the redisFieldInfo Attribute from the property
                        redisFieldInfo = property.CustomAttributes.FirstOrDefault(x => 
                            x.AttributeType.Name == nameof(RedisHashFieldAttribute));
                        _attributeCache.Add(property, redisFieldInfo);
                    }

                    if (redisFieldInfo != null)
                    {
                        // check if the field is an array or collection of some type
                        if (!(bool)redisFieldInfo.ConstructorArguments[1].Value)
                        {
                            // Get the property name from the field info
                            var propertyName = redisFieldInfo.ConstructorArguments[0].Value.ToString();
                            var value = document.GetProperties()
                                .FirstOrDefault(x => x.Key == propertyName).Value.ToString();

                            if (value != null)
                            {
                                // we need to get the underlying type for a nullable so that the reflection
                                // library doesn't complain
                                var underlyingType = Nullable.GetUnderlyingType(property.PropertyType);

                                if (underlyingType != null)
                                {
                                    var castValue = Convert.ChangeType(value, underlyingType);
                                    property.SetValue(obj, castValue); // set the value of the instance
                                }
                                else
                                {
                                    var castValue = Convert.ChangeType(value, property.PropertyType);
                                    property.SetValue(obj, castValue); // set the value of the instance
                                }
                            }
                        }
                        else
                        {
                            var value = document.GetProperties()
                                .Where(x => x.Key.Contains(redisFieldInfo.ConstructorArguments[0].Value.ToString()))
                                .Select(x => x.Value.ToString()).ToArray();
                            property.SetValue(obj, value);
                        }

                    }
                }
                list.Add(obj);
            }

            return list;
        }        
    }
}
