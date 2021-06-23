using Microsoft.Extensions.Configuration;
using NRedi2Read.Models;
using NRedi2Read.Providers;
using NReJSON;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NRedi2Read.Services
{
    public class UserService
    {
        private readonly RedisProvider _redisProvider;

        private readonly int _bcryptWorkFactory;

        public UserService(RedisProvider redisProvider, IConfiguration config)
        {
            _redisProvider = redisProvider;
            if(config["BCryptWorkFactor"] != null)
            {
                _bcryptWorkFactory = int.Parse(config["BCryptWorkFactor"]); // use a differnet work factor
            }
            else
            {
                _bcryptWorkFactory = 11; // use the default
            }

        }
        
        /// <summary>
        /// Creates a user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task Create(User user)
        {
            var db = _redisProvider.Database;
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            await db.JsonSetAsync(UserKey(user.Id), user);
        }

        /// <summary>
        /// Creates a set of user NOTE this can take a very long time if you don't set the
        /// BCryptWorkFactor configuration item to a lower number (e.g. 4) which is fine for demonstration
        /// but obviously you'd want to avoid using a lower work factor for any production item as the password
        /// hashes are intrinsically more vulnerable.
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        public async Task CreateBulk(IEnumerable<User> users)
        {
            var db = _redisProvider.Database;
            var tasks = new List<Task>();
            foreach(var user in users)
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password, _bcryptWorkFactory);
                tasks.Add(db.JsonSetAsync(UserKey(user.Id), user));
            }
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// gets users for the given Ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetBulk(IEnumerable<string> ids)
        {
            var db = _redisProvider.Database;
            var tasks = new List<Task<string>>();
            foreach (var id in ids)
            {
                tasks.Add(db.JsonGetAsync<string>(UserKey(id), "Id"));
            }
            try
            {
                await Task.WhenAll(tasks);
            }
            catch (RedisKeyNotFoundException)
            {
            }
            return tasks.Where(t=>!t.IsFaulted).Select(t => t.Result);
        }

        /// <summary>
        /// gets a single user from the database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<User> Read(string id)
        {
            var db = _redisProvider.Database;
            return await db.JsonGetAsync<User>(UserKey(id));
        }

        /// <summary>
        /// generates a redis key for a user id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static RedisKey UserKey(string id)
        {
            return new(new User().GetType().Name + ":" + id);
        }
    }
}