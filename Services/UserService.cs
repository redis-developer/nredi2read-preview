using Microsoft.Extensions.Configuration;
using NRedi2Read.Models;
using NRedi2Read.Providers;
using NRediSearch;
using NReJSON;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NRediSearch.Client;

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
            var tasks = new List<Task<bool>>();
            foreach (var id in ids)
            {
                tasks.Add(db.KeyExistsAsync(UserKey(id)));
            }
            await Task.WhenAll(tasks);
            return ids.Where((id, index) => tasks[index].Result);
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

        public void CreateUserIndex()
        {
            var db = _redisProvider.Database;
            try
            {
                db.Execute("FT.DROPINDEX", "user-idx");
            }
            catch
            {
                // do nothing
            }
            db.Execute("FT.CREATE", "user-idx", "ON", "JSON", "PREFIX", "1", "User:", "SCHEMA", "$.Email", "AS", "email", "TAG", "$.Password", "AS", "password", "TAG");
        }

        public async Task<User> ValidateUserCredentials(string email, string password){
            var db = _redisProvider.Database;
            var client = new Client("user-idx", db);
            var user = await GetUserWithEmail(email);
            if(user == null || !BCrypt.Net.BCrypt.Verify(password,user.Password))
            {
                return null;
            }
            return new User { Email = user.Email, Name = user.Name, Id = user.Id };
        }

        public async Task<User> GetUserWithEmail(string email)
        {
            var db = _redisProvider.Database;
            var searchClient = new Client("user-idx", db);
            var escapedEmail = RediSearchEscape(email);

            var query = new Query($"@email:{{{escapedEmail}}}");

            var result = (await searchClient.SearchAsync(query)).Documents.FirstOrDefault();
            if (result == null)
            {
                return null;
            }
            var user = await db.JsonGetAsync<User>(result.Id);
            return user;
        }

        public string RediSearchEscape(string inputString)
        {
            var chars = new char[] { ',', '.', '<', '>', '{', '}', '[', ']', '"', '\'', ':', ';', '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '-', '+', '=', '~' };
            var sb = new StringBuilder();
            foreach(char c in inputString)
            {
                if (chars.Contains(c))
                {
                    sb.Append("\\");
                }
                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}