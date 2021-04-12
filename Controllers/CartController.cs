using dotnetredis.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using dotnetredis.Services;

namespace dotnetredis.Controllers
{
    [ApiController]
    [Route("/api/carts")]
    public class CartController : ControllerBase
    {
        
        private readonly CartService _cartService;
        
        public CartController(CartService service)
        {
            _cartService = service;
        }
        
        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Cart))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult Get(long id)
        {
            var result = _cartService.Get(id);
            if (result != null)
            {
                return Ok(result);
            }
            return NoContent();
        }

        [HttpPost]
        [Route("addToCart")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult AddToCart(long id, CartItem item)
        {
            _cartService.AddToCart(id, item);
            return Ok();
        }

        [HttpDelete]
        [Route("removeFromCart")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult RemoveFromCart(long id, string isbn)
        {
            _cartService.RemoveFromCart(id, isbn);
            return Ok();
        }

        [HttpPost]
        [Route("create")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(long))]
        public IActionResult Create(long userId)
        {
            return Ok(_cartService.Create(userId));
        }
    }
}