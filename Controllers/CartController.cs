using dotnetredis.Models;
using Microsoft.AspNetCore.Mvc;
using NReJSON;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace dotnetredis.Controllers
{
    [ApiController]
    [Route("/api/carts")]
    public class CartController : ControllerBase
    {
        [HttpGet]
        [Route("get")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Cart))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Get(string id)
        {
            var db = Program.GetDatabase();
            var key = $"Cart:{id}";

            var result = db.JsonGet(key);

            if (result.IsNull) return NotFound();

            var cart = JsonConvert.DeserializeObject<Cart>(result.ToString());
            return Ok(cart);
        }

        [HttpPost]
        [Route("addToCart")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult AddToCart(string id, CartItem item)
        {
            var db = Program.GetDatabase();
            var key = $"Cart:{id}";

            string json = JsonConvert.SerializeObject(item);
            db.JsonArrayAppend(key, "Items", json);
            return Ok();
        }

        [HttpDelete]
        [Route("removeFromCart")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult RemoveFromCart(string id, string isbn)
        {
            var db = Program.GetDatabase();
            var key = $"Cart:{id}";

            var result = db.JsonGet(key, "Items");

            if (result.IsNull) return NotFound();

            var cartItems = JsonConvert.DeserializeObject<CartItem[]>(result.ToString());
            cartItems = cartItems.Where(item => item.Isbn != isbn).ToArray();

            string json = JsonConvert.SerializeObject(cartItems);
            db.JsonSet(key, json, "Items");

            return Ok();
        }

        [HttpPost]
        [Route("create")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(long))]
        public IActionResult Create(long userId)
        {
            var db = Program.GetDatabase();

            var cart = new Cart();

            cart.Id = db.StringIncrement("Cart:id");
            cart.UserId = userId;
            cart.Items = new CartItem[] {};

            string key = $"Cart:{cart.Id}";
            string json = JsonConvert.SerializeObject(cart);

            OperationResult result = db.JsonSet(key, json);

            return Ok(cart.Id);
        }
    }
}