using NRedi2Read.Helpers;
using NRedi2Read.Models;
using NRedi2Read.Providers;
using StackExchange.Redis;
using System.Linq;
using System.Threading.Tasks;

namespace NRedi2Read.Services
{
    public class BookRatingService
    {
        const string BOOK_RATING_KEY_FORMAT = "BookRating:{0}";
        private readonly IDatabase _db;
        public BookRatingService(RedisProvider provider)
        {
            _db = provider.Database;
        }

        /// <summary>
        /// Creates a single BookRating as a Hash
        /// </summary>
        /// <param name="rating"></param>
        /// <returns></returns>
        public async Task Create(BookRating rating)
        {
            await _db.HashSetAsync(BookRatingKey(rating.Id), rating.AsHashEntries().ToArray());
        }

        /// <summary>
        /// Gets a Book Rating
        /// </summary>
        /// <param name="id"></param>        
        /// <returns></returns>
        public async Task<BookRating> Get(string id)
        {
            var rating = await _db.HashGetAllAsync(BookRatingKey(id));
            if (rating.Length == 0)
            {
                return null;
            }
            return RedisHelper.ConvertFromRedis<BookRating>(rating);
        }

        /// <summary>
        /// Creates a RedisKey for book ratings
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string BookRatingKey(string id)
        {
            return string.Format(BOOK_RATING_KEY_FORMAT, id);
        }
    }
}
