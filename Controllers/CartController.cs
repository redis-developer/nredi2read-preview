using NRedi2Read.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using NRedi2Read.Services;
using System.Threading.Tasks;
using System;

namespace NRedi2Read.Controllers
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
        
        /// <summary>
        /// Gets a Cart, returns a 404 if the cart isn't found
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Cart))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(string id)
        {
            var result = await _cartService.Get(id);
            if (result != null)
            {
                return Ok(result);
            }
            return NotFound();
        }

        /// <summary>
        /// Add a book to the cart
        /// </summary>
        /// <param name="id"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{id}/addToCart")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AddToCart(string id, CartItem item)
        {
            try
            {
                await _cartService.AddToCart(id, item);
                var cart = await _cartService.Get(id);
                return Ok(cart);
            }
            catch (InvalidOperationException)
            {
                return StatusCode(403);
            }
        }

        /// <summary>
        /// removes a book from the cart
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isbn"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{id}/removeFromCart")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RemoveFromCart(string id, string isbn)
        {
            await _cartService.RemoveFromCart(id, isbn);
            return Ok();
        }

        /// <summary>
        /// Create a new cart for a given User.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("create")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(long))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create(string userId)
        {
            try
            {
                var cartId = await _cartService.Create(userId);
                return Ok(await _cartService.Get(cartId));
            }
            catch (RedisKeyNotFoundException)
            {
                return NotFound();
            }
            
        }

        /// <summary>
        /// Checks out, this will close the cart, and add all the books in it to the user
        /// </summary>
        /// <param name="cartId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{cartId}/checkout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status410Gone)]
        public async Task<IActionResult> Checkout(string cartId)
        {
            var cart = await _cartService.Get(cartId);

            if(cart == null)
            {
                return NotFound();
            }
            if(cart.Closed)
            {
                return StatusCode(410);
            }
            await _cartService.Checkout(cartId);

            return Ok("Cart Checked out");
        }

        [HttpGet]
        [Route("getByUserId")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetForUserId([FromQuery]string userId)
        {
            var cart = await _cartService.GetCartForUser(userId);
            if(cart == null)
            {
                var cartId = await _cartService.Create(userId);
                cart = await _cartService.Get(cartId);
            }
            return Ok(cart);
        }
    }
}