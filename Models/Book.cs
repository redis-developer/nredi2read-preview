using NRedi2Read.Helpers;
namespace NRedi2Read.Models
{
    public class Book
    {
        [RedisHashField("id")]
        public string Id { get; set; }

        [RedisHashField("title")]
        public string Title { get; set; }

        [RedisHashField("subtitle")]
        public string Subtitle { get; set; }

        [RedisHashField("Description")]
        public string Description { get; set; }

        [RedisHashField("language")]
        public string Language { get; set; }

        [RedisHashField("pageCount")]
        public long? PageCount { get; set; }

        [RedisHashField("thumbnail")]
        public string Thumbnail { get; set; }

        [RedisHashField("price")]
        public string Price { get; set; }

        [RedisHashField("currency")]
        public string Currency { get; set; }

        [RedisHashField("infoLink")]
        public string InfoLink { get; set; }

        [RedisHashField("authors", isArray: true)]
        public string[] Authors { get; set; }
    }
}