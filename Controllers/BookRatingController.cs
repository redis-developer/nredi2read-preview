using NRedi2Read.Models;
using Microsoft.AspNetCore.Mvc;
using NRedi2Read.Services;
using System.Threading.Tasks;

namespace NRedi2Read.Controllers
{
    [ApiController]
    [Route("/api/ratings")]
    public class BookRatingController : ControllerBase
    {
        private readonly BookRatingService _bookRatingService;

        public BookRatingController(BookRatingService service)
        {
            _bookRatingService = service;
        }

        /// <summary>
        /// Creates a Book Rating
        /// </summary>
        /// <param name="rating"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create(BookRating rating)
        {
            await _bookRatingService.Create(rating);
            return StatusCode(201);
        }

        /// <summary>
        /// Gets a Book Rating
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var bookRating = await _bookRatingService.Get(id);
            if (bookRating != null)
            {
                return Ok(bookRating);
            }
            else
            {
                return NotFound();
            }
        }
    }
}