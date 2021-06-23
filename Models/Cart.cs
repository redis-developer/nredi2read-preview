namespace NRedi2Read.Models
{
    public class Cart
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public CartItem[] Items { get; set; }
        public bool Closed { get; set; } = false;
    }
}