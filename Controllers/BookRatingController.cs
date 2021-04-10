using dotnetredis.Models;
using Microsoft.AspNetCore.Mvc;

namespace dotnetredis.Controllers
{
    [ApiController]
    [Route("/api/ratings")]
    public class BookRatingController
    {
        [HttpPost]
        [Route("create")]
        public void Create(BookRating book)
        {
        }

        [HttpPost]
        [Route("read")]
        public BookRating Get(string id)
        {
            return new BookRating();
        }
    }
}