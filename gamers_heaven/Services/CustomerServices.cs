using gamers_heaven.Data;
using gamers_heaven.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace gamers_heaven.Services
{
    public class CustomerService
    {
        private readonly AppDbContext _context;

        public CustomerService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CustomerViewModel>> GetCustomersBySellerIdAsync(string sellerId)
        {
            // Fetch the necessary data without performing the aggregation
            var cartData = await _context.Cart
                .Where(c => c.IsPurchased && c.CartDetails.Any(cd => cd.Products.UserId == sellerId))
                .Include(c => c.CartDetails)
                .ThenInclude(cd => cd.Products)
                .Include(c => c.AppUser)
                .ToListAsync();

            // Perform the aggregation in memory
            var customers = cartData
                .GroupBy(c => c.UserId)
                .Select(g => new CustomerViewModel
                {
                    CustomerName = g.First().AppUser.Name,
                    TotalSpent = g.Sum(c => c.CartDetails.Sum(cd => cd.Price * cd.Quantity))
                })
                .ToList();

            return customers;
        }
    }
}
