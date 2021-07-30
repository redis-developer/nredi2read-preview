using Microsoft.Extensions.Configuration;
using NRedi2Read.Helpers;
using NRedi2Read.Models;
using NRedi2Read.Providers;
using NRediSearch;
using NRediSearch.QueryBuilder;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            if (user.Books!=null)
            {
                await db.SetAddAsync(UserBookKey(user.Id), user.Books.Select(r => new RedisValue(r.ToString())).ToArray());
            }            
            user.Books = null;
            await db.HashSetAsync(UserKey(user.Id), user.AsHashEntries().ToArray());
        }

        /// <summary>
        /// adds a range of books to the users book id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="books"></param>
        /// <returns></returns>
        public async Task AddBooks(string id, params string[] books)
        {
            var db = _redisProvider.Database;
            await db.SetAddAsync(UserBookKey(id), books.Select(b=>new RedisValue(b)).ToArray());
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
                if (user.Books != null)
                {
                    tasks.Add(db.SetAddAsync(UserBookKey(user.Id), user.Books.Select(r => new RedisValue(r.ToString())).ToArray()));
                }
                tasks.Add(db.HashSetAsync(UserKey(user.Id), user.AsHashEntries().ToArray()));
            }
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Checks the existence of users for given IDs
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> CheckBulk(IEnumerable<string> ids)
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
            var result = await db.HashGetAllAsync(UserKey(id));
            var user = RedisHelper.ConvertFromRedis<User>(result);
            var books = await db.SetMembersAsync(UserBookKey(id));
            user.Books = books.Select(r=>r.ToString()).ToList();
            return user;
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

        public static RedisKey UserBookKey(string id)
        {
            return new RedisKey($"{UserKey(id)}:Books");
        }

        public void CreateUserIndex()
        {
            var db = _redisProvider.Database;
            var client = new Client("user-idx",db);
            try
            {
                db.Execute("FT.DROPINDEX", "user-idx");
            }
            catch
            {
                // do nothing
            }

            var schema = new Schema();
            schema.AddTagField("Email");            
            var options = new Client.ConfiguredIndexOptions(new Client.IndexDefinition(prefixes: new[] { "User:" }));
            client.CreateIndex(schema, options);
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

            var query = new Query($"@Email:{{{escapedEmail}}}");
            
            var result = (await searchClient.SearchAsync(query)).Documents.FirstOrDefault();
            if (result == null)
            {
                return null;
            }
            var user = await Read(result.Id.Split(':')[1]);
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