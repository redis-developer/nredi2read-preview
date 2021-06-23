using System;

namespace NRedi2Read.Helpers
{
    /// <summary>
    /// Decoration for POCO Properties, meant to make it easier to identify what the proper name of 
    /// a field is meant to be from Redis' perspective - can make interoption between different languages
    /// with different naming convetions easier (e.g. camelCase in java <-> PascalCase in .NET)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class RedisHashFieldAttribute : Attribute
    {        
        public RedisHashFieldAttribute(string redisFieldName, bool isArray = false){}
    }
}
