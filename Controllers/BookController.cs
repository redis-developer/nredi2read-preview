using dotnetredis.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using NRediSearch;
using System.Linq;
using System;

namespace dotnetredis.Controllers
{
    [ApiController]
    [Route("/api/books")]
    public class BookController : ControllerBase
    {

        [HttpPost]
        [Route("create")]
        public void Create(Book book)
        {
            var db = Program.GetDatabase();
            var bookKey = $"Book:{book.Id}";
            var bookAuthorsKey = $"{bookKey}:authors";

            db.HashSet(bookKey, Program.ToHashEntries(book));
            foreach (var author in book.Authors)
            {
                db.SetAdd(bookAuthorsKey, author);
            }
        }

        [HttpGet]
        [Route("read")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Get(string id)
        {
            var db = Program.GetDatabase();
            var bookKey = $"Book:{id}";
            var bookAuthorsKey = $"{bookKey}:authors";

            var bookHash = db.HashGetAll(bookKey);
            if (bookHash.Length == 0) return NotFound();

            var book = Program.ConvertFromRedis<Book>(bookHash);

            var authors = db.SetMembers(bookAuthorsKey);
            book.Authors = authors.Select(author => author.ToString()).ToArray();

            return Ok(book);
        }

        [HttpGet]
        [Route("search")]
        public Book[] Search(string text)
        {
            var db = Program.GetDatabase();

            Client client = new Client("books-idx", db);

            var q = new Query(text);
            var result = client.Search(q);

            var books = result.Documents.Select(d => 
            {
                var id = d.Id.Split(":")[1];

                var bookKey = $"Book:{id}";
                var bookAuthorsKey = $"{bookKey}:authors";

                var bookHash = db.HashGetAll(bookKey);
                var book = Program.ConvertFromRedis<Book>(bookHash);

                var authors = db.SetMembers(bookAuthorsKey);
                book.Authors = authors.Select(author => author.ToString()).ToArray();

                return book;
            }).ToArray();

            return books;
        }

        [HttpPost]
        [Route("load")]
        public void Load(Book[] books)
        {
            foreach (var book in books)
            {
                Create(book);
            }
        }

        [HttpGet]
        [Route("createIndex")]
        public void CreateIndex()
        {
            Client client = new Client("books-idx", Program.GetDatabase());

            // drop the index, if it doesn't exists, that's fine
            try
            {
                client.DropIndex();
            }
            catch {}

            var schema = new Schema();
            schema.AddSortableTextField("Title");
            schema.AddTextField("Subtitle");
            schema.AddTextField("Description");

            Client.ConfiguredIndexOptions options = new Client.ConfiguredIndexOptions(
                new Client.IndexDefinition( prefixes: new [] { "Book:" } )
            );

            client.CreateIndex(schema, options);
        }
    }
}