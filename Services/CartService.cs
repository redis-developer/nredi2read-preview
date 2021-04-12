using System;
using System.Linq;
using dotnetredis.Models;
using dotnetredis.Providers;
using Newtonsoft.Json;
using NReJSON;
using StackExchange.Redis;

namespace dotnetredis.Services
{
    public class CartService
    {
        private readonly RedisProvider _redisProvider;

        public CartService(RedisProvider redisProvider)
        {
            _redisProvider = redisProvider;
        }

        public Cart Get(long id)
        {
            var db = _redisProvider.Database();
            var result = db.JsonGet(CartKey(id));
            if (result.IsNull) return null;
            var cart = JsonConvert.DeserializeObject<Cart>(result.ToString() ?? throw new InvalidOperationException());
            return cart;
        }

        public void AddToCart(long id, CartItem item)
        {
            var db = _redisProvider.Database();
            string json = JsonConvert.SerializeObject(item);
            db.JsonArrayAppend(CartKey(id), "Items", json);
        }

        public void RemoveFromCart(long id, string isbn)
        {
            var db = _redisProvider.Database();
            var result = db.JsonGet(CartKey(id), "Items");
            if (result.IsNull) return;
            var cartItems = JsonConvert.DeserializeObject<CartItem[]>(result.ToString() ?? throw new InvalidOperationException());
            cartItems = (cartItems ?? throw new InvalidOperationException()).Where(item => item.Isbn != isbn).ToArray();
            string json = JsonConvert.SerializeObject(cartItems);
            db.JsonSet(CartKey(id), json, "Items");
        }

        public long Create(long userId)
        {
            var db = _redisProvider.Database();
            var cart = new Cart
            {
                Id = db.StringIncrement("Cart:id"), 
                UserId = userId, 
                Items = Array.Empty<CartItem>()
            };
            var json = JsonConvert.SerializeObject(cart);
            db.JsonSet(CartKey(cart.Id), json);
            return cart.Id;
        }

        private RedisKey CartKey(long id)
        {
            return new(new Cart().GetType().Name + ":" + id);
        }
    }
}