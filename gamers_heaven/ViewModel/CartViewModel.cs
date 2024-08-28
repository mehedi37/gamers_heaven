using gamers_heaven.Models;

namespace gamers_heaven.ViewModel
{
    public class CartViewModel
    {
        public int CartId { get; set; }
        public List<CartDetails> CartDetails { get; set; } = new List<CartDetails>();
        public decimal TotalPrice { get; set; }
    }
}
