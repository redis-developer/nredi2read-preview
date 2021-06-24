using NRedi2Read.Helpers;

namespace NRedi2Read.Models
{
    public class BookRating
    {
        [RedisHashField("id")]
        public string Id { get; set; }

        [RedisHashField("userId")]
        public string UserId { get; set; }

        [RedisHashField("bookId")]
        public string BookId { get; set; }

        [RedisHashField("rating")]
        public int Rating { get; set; }
    }
}