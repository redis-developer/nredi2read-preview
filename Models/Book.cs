namespace dotnetredis.Models
{
    public class Book
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Description { get; set; }
        public string Language { get; set; }
        public long PageCount { get; set; }
        public string Thumbnail { get; set; }
        public double Price { get; set; }
        public string Currency { get; set; }
        public string InfoLink { get; set; }
        public string[] Authors { get; set; }
    }
}