using NRedi2Read.Helpers;
using NRedi2Read.Models;
using NRedi2Read.Providers;
using NRediSearch;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NRedi2Read.Services
{
    public class BookService
    {
        private readonly RedisProvider _redisProvider;
        private readonly Client _searchClient;

        public BookService(RedisProvider redisProvider)
        {
            _redisProvider = redisProvider;
            _searchClient = new Client("books-idx", _redisProvider.Database);
        }

        /// <summary>
        /// Get a single book
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Book> Get(string id)
        {
            var query = new Query($"@id:{{{id}}}");
            var result = await _searchClient.SearchAsync(query);
            if(result.TotalResults == 0)
            {
                return null;
            }
            return result.AsList<Book>().FirstOrDefault();
        }

        /// <summary>
        /// Creates a whole set of books in the database, great for bulk-loading when
        /// the app is starting up
        /// </summary>
        /// <param name="books"></param>
        /// <returns></returns>
        public async Task<bool> CreateBulk(IEnumerable<Book> books)
        {
            var db = _redisProvider.Database;
            var tasks = new List<Task>();
            foreach(var book in books)
            {
                var hashEntries = book.AsHashEntries().ToArray();
                tasks.Add(db.HashSetAsync(BookKey(book.Id), hashEntries));
            }
            await Task.WhenAll(tasks.ToArray());

            return true;
        }

        /// <summary>
        /// creates a single book
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        public async Task<Book> Create(Book book)
        {
            var db = _redisProvider.Database;
            db.HashSet(BookKey(book.Id), book.AsHashEntries().ToArray());
            return await Get(book.Id);
        }

        /// <summary>
        /// Get all the books associated with the given Ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetBulk(IEnumerable<string> ids)
        {
            var db = _redisProvider.Database;
            var tasks = new List<Task<RedisValue>>();
            foreach(var id in ids)
            {
                tasks.Add(db.HashGetAsync(BookKey(id), "id"));
            }
            await Task.WhenAll(tasks);
            return tasks.Select(t => t.Result.ToString());
        }

        /// <summary>
        /// Searches for books matching the given query see the
        /// <see href="https://oss.redislabs.com/redisearch/Commands/#ftsearch">RediSearch</see>
        /// docs for details on how to structure queries.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Book>> Search(string query)
        {
            var q = new Query(query);
            var result = await _searchClient.SearchAsync(q);
            return result.AsList<Book>();
        }

        /// <summary>
        /// Paginates all the books in the database for the given query.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<IList<Book>> PaginateBooks(string query, int page, int pageSize = 10)
        {
            var q = new Query(query);
            q.Limit(page * pageSize, pageSize);
            var results = await _searchClient.SearchAsync(q);
            return results.AsList<Book>();
        }

        /// <summary>
        /// Creates the book index
        /// </summary>
        /// <returns></returns>
        public async Task CreateBookIndex()
        {
            // drop the index, if it doesn't exists, that's fine
            try
            {
                await _redisProvider.Database.ExecuteAsync("FT.DROPINDEX", "books-idx");
            }
            catch(Exception)
            {
                // books-idx didn't exist - don't do anything
            }

            var schema = new Schema();

            schema.AddSortableTextField("title");
            schema.AddTextField("subtitle");
            schema.AddTextField("description");
            schema.AddTagField("id");
            schema.AddTextField("authors.[0]");
            schema.AddTextField("authors.[1]");
            schema.AddTextField("authors.[2]");
            schema.AddTextField("authors.[3]");
            schema.AddTextField("authors.[4]");
            schema.AddTextField("authors.[5]");
            schema.AddTextField("authors.[7]");
            var options = new Client.ConfiguredIndexOptions(
                new Client.IndexDefinition( prefixes: new [] { "Book:" } )
            );
            await _searchClient.CreateIndexAsync(schema, options);
        }
        
        /// <summary>
        /// generates a redis key for a book of a given id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static RedisKey BookKey(string id)
        {
            return new RedisKey(new Book().GetType().Name + ":" + id);
        }
    }
}