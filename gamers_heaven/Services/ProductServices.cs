using gamers_heaven.Data;
using gamers_heaven.Models;
using Microsoft.EntityFrameworkCore;

namespace gamers_heaven.Services
{
    public class ProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Products>> GetItemsForSaleByUserIdAsync(string userId)
        {
            return await _context.Products.Where(p => p.UserId == userId).ToListAsync();
        }

        public async Task<Products?> GetItemByIdAsync(int id)
        {
            return await _context.Products.FindAsync(id);
        }

        public async Task AddItemAsync(Products product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateItemAsync(Products product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteItemAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }
    }
}
