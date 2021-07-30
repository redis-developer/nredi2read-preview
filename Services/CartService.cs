using NRedi2Read.Helpers;
using NRedi2Read.Models;
using NRedi2Read.Providers;
using NRediSearch;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NRedi2Read.Services
{
    public class CartService
    {
        private readonly RedisProvider _redisProvider;
        private readonly UserService _userService;

        public CartService(RedisProvider redisProvider, UserService userService)
        {
            _redisProvider = redisProvider;
            _userService = userService;
        }

        /// <summary>
        /// Get's a cart
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Cart> Get(string id)
        {
            var db = _redisProvider.Database;
            var result = await db.HashGetAllAsync(CartKey(id));
            var cart = RedisHelper.ConvertFromRedis<Cart>(result);
            cart.Items = ParseCartItems(result).ToArray();
            return cart;
        }

        private IEnumerable<CartItem> ParseCartItems(IEnumerable<HashEntry> entries)
        {
            var uniqueIds = new HashSet<string>();
            entries
                .Where(s => s.Name.ToString().Split(':').Length > 2)
                .Select(s => s.Name.ToString().Split(":")[1]).ToList()
                .ForEach(x => uniqueIds.Add(x));
            foreach(var id in uniqueIds)
            {
                var isbn = entries.FirstOrDefault(e => e.Name == $"items:{id}:Isbn").Value;
                var price = entries.FirstOrDefault(e => e.Name == $"items:{id}:Price").Value;
                var quantity = (long)entries.FirstOrDefault(e => e.Name == $"items:{id}:Quantity").Value;
                yield return new CartItem { Isbn = isbn, Quantity = quantity, Price=price};
            }
            
            
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
            var closed = bool.Parse(await db.HashGetAsync(CartKey(id), "Closed"));
            if (closed)
            {
                throw new InvalidOperationException("Cart has already been closed out");
            }
            var key = CartKey(id);
            await db.HashSetAsync(key, item.AsHashEntries(CartItemKey(id, item.Isbn)).ToArray());            
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
            await db.HashDeleteAsync(CartKey(id), CartItemHashFields(id,isbn));
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
            var currentCart = await GetCartForUser(userId);
            if (currentCart != null)
            {
                return currentCart.Id;
            }
            var user = await _userService.Read(userId);
            var newCartId = (await db.StringIncrementAsync("Cart:id")).ToString(); // get the cart id by incrmenting the current highestcart id
            var cart = new Cart
            {
                Id = newCartId,
                UserId = userId, 
                Items = null
            };
            await db.HashSetAsync(CartKey(cart.Id), cart.AsHashEntries().ToArray());            
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
            
            var cart = await Get(id);
            
            await _userService.AddBooks(cart.UserId, new List<string>(cart.Items.Select(x => x.Isbn)).ToArray());            ;
            await db.HashSetAsync(CartKey(id), "Closed", true);
            return true;
        }

        /// <summary>
        /// Returns cart that has not been closed for user, if one exists, otherwise returns null
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<Cart> GetCartForUser(string userId)
        {
            var db = _redisProvider.Database;
            var client = new Client("cart-idx", db);
            var query = new Query($"@UserId: {userId}");
            query.ReturnFields("Id","Closed");
            query.Limit(0,1);
            var result = await client.SearchAsync(query);
            if (result.Documents.Count < 1)
            {
                return null;
            }
            var cart = result.Documents.Where(x => x["Closed"] != "true").FirstOrDefault();
            if(cart == null)
            {
                return null;
            }
            var idStr = result.Documents[0]["Id"].ToString();
            return await Get(idStr);
        }

        public void CreateCartIndex()
        {
            var db = _redisProvider.Database;
            var client = new Client("cart-idx", db);
            try
            {
                db.Execute("FT.DROPINDEX", "cart-idx");
            }
            catch (Exception)
            {
                //do nothing, the index didn't exist
            }
            var schema = new Schema();
            schema.AddSortableTextField("UserId");
            var options = new Client.ConfiguredIndexOptions(new Client.IndexDefinition(prefixes: new[] { "Cart:" }));
            client.CreateIndex(schema, options);
        }

        private RedisValue[] CartItemHashFields(string cartId, string isbn)
        {
            return new RedisValue[] { $"{CartItemKey(cartId, isbn)}:Isbn", $"{CartItemKey(cartId, isbn)}:Price", $"{CartItemKey(cartId, isbn)}:Quantity" };
        }        

        private RedisKey CartKey(string id)
        {
            return new(new Cart().GetType().Name + ":" + id);
        }

        private RedisKey CartItemKey(string cartId, string isbn)
        {
            return $"items:{isbn}:";
        }
    }
}