using gamers_heaven.Areas.Identity.Data;
using gamers_heaven.Data;
using gamers_heaven.Models;
using gamers_heaven.Services;
using gamers_heaven.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace gamers_heaven.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly ProductService _productService;
        private readonly CustomerService _customerService;

        public HomeController(ILogger<HomeController> logger, AppDbContext context, UserManager<AppUser> userManager, ProductService productService, CustomerService customerService)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _productService = productService;
            _customerService = customerService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var products = await _context.Products
                .Where(p => p.UserId != userId)
                .ToListAsync();
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var products = string.IsNullOrEmpty(query)
                ? await _context.Products
                    .Where(p => p.UserId != userId)
                    .ToListAsync()
                : await _context.Products
                    .Where(p => (p.ProductName.ToLower().Contains(query) || p.ProductDescription.ToLower().Contains(query)) && p.UserId != userId)
                    .ToListAsync();

            return PartialView("Partials/_GamesList", products);
        }

        public async Task<IActionResult> GameDetails(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var otherProducts = await _context.Products
                .Where(p => p.UserId != userId && p.ProductId != id)
                .Take(4)
                .ToListAsync();

            var viewModel = new ProductDetailsViewModel
            {
                Product = product,
                OtherProducts = otherProducts
            };

            return View(viewModel);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
