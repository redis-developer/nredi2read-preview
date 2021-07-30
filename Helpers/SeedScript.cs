using Newtonsoft.Json;
using NRedi2Read.Models;
using NRedi2Read.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace NRedi2Read
{
    public static class SeedScript
    {
        /// <summary>
        /// seeds the database with lots of fun stuff
        /// </summary>
        /// <param name="bookService">the service for books</param>
        /// <param name="userService">the service for users</param>
        /// <param name="cartService"></param>
        /// <returns></returns>
        public static async Task<bool> SeedDatabase(BookService bookService, UserService userService, CartService cartService)
        {
            var books = await CreateBooks(bookService);

            var users = await CreateUsers(userService);

            await CreateCarts(cartService, userService, bookService, books, users);

            await bookService.CreateBookIndex();

            
            return true;
        }

        /// <summary>
        /// Loads books out of redi2read-data/books, deseralizes them from JSON into POCOs
        /// Checks the Database to see if they are present, and loads every book not currently present
        /// in the database
        /// </summary>
        /// <param name="bookService"></param>
        /// <returns></returns>
        private static async Task<IEnumerable<Book>> CreateBooks(BookService bookService)
        {
            List<Book> books = new List<Book>();            

            //Create Books
            foreach (var file in Directory.GetFiles("redi2read-data/books"))
            {
                books.AddRange(JsonConvert.DeserializeObject<Book[]>(await File.ReadAllTextAsync(file)));
            }

            var currentBooks = await bookService.GetBulk(books.Select(b => b.Id));
            var booksToCreate = books.Where(b => !currentBooks.Contains(b.Id)).ToList();

            Console.WriteLine($"Creating {booksToCreate.Count} books");
            await bookService.CreateBulk(booksToCreate);
            Console.WriteLine("Finished creating books");
            return books;
        }

        /// <summary>
        /// Reads all the user info out of the redi2read-data/users directory and loads them into
        /// the database if they aren't already present - NOTE - because of the way the BCRYPT
        /// algorithm works, if the BCryptWorkFactor configuration variable is not set to something relatively
        /// low, bulk loading users can take time
        /// </summary>
        /// <param name="userService"></param>
        /// <returns></returns>
        private static async Task<IEnumerable<User>> CreateUsers(UserService userService)
        {
            List<User> users = new List<User>();

            foreach (var file in Directory.GetFiles("redi2read-data/users"))
            {
                users.AddRange(JsonConvert.DeserializeObject<User[]>(await File.ReadAllTextAsync(file)));
            }

            var currentUsers = await userService.CheckBulk(users.Select(u => u.Id));
            var usersToCreate = users.Where(u => !currentUsers.Contains(u.Id)).ToList();

            Console.WriteLine($"Creating {usersToCreate.Count} users");
            await userService.CreateBulk(usersToCreate);
            Console.WriteLine("Finished Creating Users");
            return users;
        }

        /// <summary>
        /// seeds the database with 100 carts each of which has 1-7 random books in it.
        /// </summary>
        /// <param name="cartService"></param>
        /// <param name="userService"></param>
        /// <param name="bookService"></param>
        /// <param name="books"></param>
        /// <param name="users"></param>
        /// <returns></returns>
        private static async Task CreateCarts(
            CartService cartService, 
            UserService userService, 
            BookService bookService,
            IEnumerable<Book> books,
            IEnumerable<User> users)
        {            
            Random rand = new Random();
            const uint NUM_CARTS = 100;
            var cartCreateTasks = new List<Task<string>>();
            //create 100 carts
            for (var i = 0; i<NUM_CARTS; i++)
            {
                var index = rand.Next(0, users.Count());
                var user = users.ElementAt(index);
                cartCreateTasks.Add(cartService.Create(user.Id));                
            }

            //await all the cart creation tasks and pull the cartId's out from the tasks
            await Task.WhenAll(cartCreateTasks);
            var cartIds = cartCreateTasks.Select(t => t.Result);

            foreach(var cart in cartIds)
            {
                var numBooks = rand.Next(1, 7);
                var cartItems = new List<CartItem>();
                for (var j = 0; j < numBooks; j++)
                {
                    var book = books.ElementAt(rand.Next(0, books.Count()));
                    var item = new CartItem { Isbn = book.Id, Price = book.Price, Quantity = 1 };
                    await cartService.AddToCart(cart, item);
                }
            }
        }
    }
}
