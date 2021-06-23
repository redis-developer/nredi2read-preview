using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NRedi2Read
{
    public class RedisKeyNotFoundException : Exception
    {
        public RedisKeyNotFoundException(string message) : base(message) { }
    }
}
