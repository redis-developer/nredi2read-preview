using NRedi2Read.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using NRedi2Read.Services;

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

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public IActionResult Create(Book book)
        {
            var newBook = _bookService.Create(book);
            return Created(newBook.Id,newBook);
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult Read(string id)
        {
            try
            {
                return Ok(_bookService.Get(id));
            }
            catch
            {
                return NoContent();
            }
        }

        [HttpGet]
        [Route("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult Search(string query)
        {
            try
            {
                return Ok(_bookService.Search(query));
            }
            catch
            {
                return NoContent();
            }
        }

        [HttpPost]
        [Route("load")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Load(Book[] books)
        {
            foreach (var book in books)
            {
                Create(book);
            }
            return Ok();
        }
        [HttpGet]
        [Route("createIndex")]
        public void CreateIndex()
        {
            _bookService.CreateBookIndex();
        }
    }
}