using NRedi2Read.Helpers;
using NRedi2Read.Models;
using NRedi2Read.Providers;
using StackExchange.Redis;

namespace NRedi2Read.Services
{
    public class UserService
    {
        private readonly RedisProvider _redisProvider;

        public UserService(RedisProvider redisProvider)
        {
            _redisProvider = redisProvider;
        }
        
        public void Create(User user)
        {
            var db = _redisProvider.Database();
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            var userHash = RedisHelper.ToHashEntries(user);
            db.HashSet(UserKey(user.Id), userHash);
        }
        
        public User Read(long id)
        {
            var db = _redisProvider.Database();
            var userHash = db.HashGetAll(UserKey(id));
            if (userHash?.Length > 0)
            {
                return RedisHelper.ConvertFromRedis<User>(userHash);
            }
            return null;
        }

        private static RedisKey UserKey(long id)
        {
            return new(new User().GetType().Name + ":" + id);
        }
    }
}