using gamers_heaven.Models;

namespace gamers_heaven.ViewModel
{
    public class ProductDetailsViewModel
    {
        public required Products Product { get; set; }
        public required List<Products> OtherProducts { get; set; }
    }
}
