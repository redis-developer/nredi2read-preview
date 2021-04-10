namespace dotnetredis.Models
{
    public class Cart
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public CartItem[] Items { get; set; }
    }
}