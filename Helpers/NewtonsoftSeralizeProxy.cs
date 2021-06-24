using Newtonsoft.Json;
using NReJSON;
using StackExchange.Redis;
using System;

namespace NRedi2Read
{
    /// <summary>
    /// Proxy to handle the serialization/deserialization for NReJSON
    /// A reference to this will be handed to NReJSON at startup
    /// </summary>
    public class NewtonsoftSeralizeProxy : ISerializerProxy
    {
        /// <summary>
        /// Marshall's a JSON result from Redis into a POCO
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="serializedValue"></param>
        /// <returns></returns>
        public TResult Deserialize<TResult>(RedisResult serializedValue)
        {
            try
            {
                return JsonConvert.DeserializeObject<TResult>(serializedValue.ToString());
            }
            catch (ArgumentNullException)
            {
                throw new RedisKeyNotFoundException("Key not present in database");
            }
        }

        /// <summary>
        /// Marshall's a POCO into a JSON String for storage in Redis
        /// </summary>
        /// <typeparam name="TObjectType"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public string Serialize<TObjectType>(TObjectType obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}
