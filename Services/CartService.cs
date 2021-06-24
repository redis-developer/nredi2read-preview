using System;
using System.Linq;
using NRedi2Read.Models;
using NRedi2Read.Providers;
using Newtonsoft.Json;
using NReJSON;
using StackExchange.Redis;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NRedi2Read.Services
{
    public class CartService
    {
        private readonly RedisProvider _redisProvider;

        public CartService(RedisProvider redisProvider)
        {
            _redisProvider = redisProvider;
        }

        /// <summary>
        /// Get's a cart
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Cart> Get(string id)
        {
            var db = _redisProvider.Database;
            var result = await db.JsonGetAsync(CartKey(id));
            if (result.IsNull) return null;
            var cart = JsonConvert.DeserializeObject<Cart>(result.ToString() ?? throw new InvalidOperationException());
            return cart;
        }

        /// <summary>
        /// adds an item to a cart
        /// </summary>
        /// <param name="id"></param>
        /// <param name="item"></param>
        /// <exception cref="InvalidOperationException">Thrown if cart is already closed</exception>
        /// <returns></returns>
        public async Task AddToCart(string id, CartItem item)
        {
            var db = _redisProvider.Database;
            var closed = await db.JsonGetAsync<bool>(CartKey(id), "Closed");
            if (closed)
            {
                throw new InvalidOperationException("Cart has already been closed out");
            }
            string json = JsonConvert.SerializeObject(item);
            await db.JsonArrayAppendAsync(CartKey(id), "Items", json);
        }

        /// <summary>
        /// Removes an Item from a cart
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isbn"></param>
        /// <returns></returns>
        public async Task RemoveFromCart(string id, string isbn)
        {
            var db = _redisProvider.Database;
            var cartItems = await db.JsonGetAsync<CartItem[]>(CartKey(id), "Items");
            cartItems = cartItems.Where(item => item.Isbn != isbn).ToArray();
            await db.JsonSetAsync(CartKey(id), cartItems, "Items");
        }

        /// <summary>
        /// Creates a new Cart
        /// </summary>
        /// <param name="userId"></param>
        /// <exception cref="RedisKeyNotFoundException">Thrown if user id isn't found</exception>
        /// <returns></returns>
        public async Task<string> Create(string userId)
        {
            var db = _redisProvider.Database;
            var user = await db.JsonGetAsync<User>(UserService.UserKey(userId));
            var newCartId = (await db.StringIncrementAsync("Cart:id")).ToString(); // get the cart id by incrmenting the current highestcart id
            var cart = new Cart
            {
                Id = newCartId,
                UserId = userId, 
                Items = Array.Empty<CartItem>()
            };
            await db.JsonSetAsync(CartKey(cart.Id), cart);
            return cart.Id;
        }

        /// <summary>
        /// Checks out a user with a given cart, adds all their items to the user
        /// and closes out the cart
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> Checkout(string id)
        {
            var db = _redisProvider.Database;
            var cart = await db.JsonGetAsync<Cart>(CartKey(id));
            var user = await db.JsonGetAsync<User>(UserService.UserKey(cart.UserId));

            if (user.Books == null)
            {
                user.Books = new List<string>(cart.Items.Select(x => x.Isbn));
            }
            else
            {
                user.Books.AddRange(cart.Items.Select(x => x.Isbn));
            }

            await db.JsonSetAsync(UserService.UserKey(user.Id), user.Books, "Books");
            await db.JsonSetAsync(CartKey(id), true, "Closed");
            return true;
        }

        private RedisKey CartKey(string id)
        {
            return new(new Cart().GetType().Name + ":" + id);
        }
    }
}