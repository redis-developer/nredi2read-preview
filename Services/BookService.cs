using System.Linq;
using dotnetredis.Helpers;
using dotnetredis.Models;
using dotnetredis.Providers;
using NRediSearch;
using StackExchange.Redis;

namespace dotnetredis.Services
{
    public class BookService
    {
        private readonly RedisProvider _redisProvider;

        public BookService(RedisProvider redisProvider)
        {
            _redisProvider = redisProvider;
        }

        public Book Get(string id)
        {
            var db = _redisProvider.Database();
            var bookHash = db.HashGetAll(BookKey(id));
            var book = RedisHelper.ConvertFromRedis<Book>(bookHash);
            var authors = db.SetMembers(BookAuthorKey(id));
            book.Authors = authors.Select(author => author.ToString()).ToArray();
            return book;
        }

        public Book Create(Book book)
        {
            var db = _redisProvider.Database();
            db.HashSetAsync(BookKey(book.Id), RedisHelper.ToHashEntries(book));
            foreach (var author in book.Authors)
            {
                db.SetAdd(BookAuthorKey(book.Id), author);
            }
            return Get(book.Id);
        }

        public Book[] Search(string query)
        {
            var db = _redisProvider.Database();
            var client = new Client("books-idx", db);
            var q = new Query(query);
            var result = client.Search(q);

            return result.Documents.Select(d => 
            {
                var id = d.Id.Split(":")[1];
                var bookHash = db.HashGetAll(BookKey(id));
                var book = RedisHelper.ConvertFromRedis<Book>(bookHash);
                var authors = db.SetMembers(BookAuthorKey(id));
                book.Authors = authors.Select(author => author.ToString()).ToArray();
                return book;
            }).ToArray();
        }

        public void CreateBookIndex()
        {
            Client client = new Client("books-idx", _redisProvider.Database());

            // drop the index, if it doesn't exists, that's fine
            try
            {
                client.DropIndex();
            }
            catch
            {
                // ignored
            }

            var schema = new Schema();
            schema.AddSortableTextField("Title");
            schema.AddTextField("Subtitle");
            schema.AddTextField("Description");
            var options = new Client.ConfiguredIndexOptions(
                new Client.IndexDefinition( prefixes: new [] { "Book:" } )
            );
            client.CreateIndex(schema, options);
        }
        
        private static RedisKey BookKey(string id)
        {
            return new RedisKey(new Book().GetType().Name + ":" + id);
        }

        private static RedisKey BookAuthorKey(string id)
        {
            return new RedisKey(new Book().GetType().Name + ":" + id + ":authors");
        }
        
    }
}