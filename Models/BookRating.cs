namespace dotnetredis.Models
{
    public class BookRating
    {
        public long Id { get; set; }
        public long userId { get; set; }
        public long bookId { get; set; }
        public int rating { get; set; }
    }
}