using NRedi2Read.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using NRedi2Read.Services;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace NRedi2Read.Controllers
{
    [ApiController]
    [Route("/api/books")]
    public class BookController : ControllerBase
    {
        private readonly BookService _bookService;
        public BookController(BookService service)
        {
            _bookService = service;
        }


        /// <summary>
        /// Creates a book object in the Redis Database
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create(Book book)
        {
            var newBook = await _bookService.Create(book);
            return Created(newBook.Id, newBook);
        }


        /// <summary>
        /// Paginates books based on the current page (0 indexed) and the Page Size
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> GetBooks([FromQuery] int page = 0, [FromQuery] int pageSize = 10, [FromQuery] string q = "*")
        {
            try
            {
                return Ok(await _bookService.PaginateBooks(q, page, pageSize));
            }
            catch (Exception)
            {

                return NoContent();
            }
        }


        /// <summary>
        /// Gets a single book
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Read(string id)
        {
            try
            {
                var result = await _bookService.Get(id);
                if (result != null)
                {
                    return Ok(result);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        /// <summary>
        /// Searches for books matching the given query see the
        /// <see href="https://oss.redislabs.com/redisearch/Commands/#ftsearch">RediSearch</see>
        /// docs for details on how to structure queries.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Search(string query)
        {
            try
            {
                return Ok(await _bookService.Search(query));
            }
            catch
            {
                return NoContent();
            }
        }

        /// <summary>
        /// Bulk loads a set of books into Redis
        /// </summary>
        /// <param name="books"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("load")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Load(Book[] books)
        {
            await _bookService.CreateBulk(books);
            return Accepted();
        }

        /// <summary>
        /// Creates the Search index
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("createIndex")]
        public async Task<IActionResult> CreateIndex()
        {
            await _bookService.CreateBookIndex();
            return Accepted();
        }
    }
}
