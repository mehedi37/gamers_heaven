using gamers_heaven.Areas.Identity.Data;
using gamers_heaven.Data;
using gamers_heaven.Models;
using gamers_heaven.Services;
using gamers_heaven.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace gamers_heaven.Controllers
{
    public class SellController : Controller
    {
        private readonly ILogger<SellController> _logger;
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly ProductService _productService;
        private readonly CustomerService _customerService;

        public SellController(ILogger<SellController> logger, AppDbContext context, UserManager<AppUser> userManager, ProductService productService, CustomerService customerService)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _productService = productService;
            _customerService = customerService;
        }

        public async Task<IActionResult> SellGames()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized(); // or handle the null case appropriately
            }

            var itemsForSale = await _productService.GetItemsForSaleByUserIdAsync(userId);
            var customers = await _customerService.GetCustomersBySellerIdAsync(userId);

            var model = new SellerViewModel
            {
                ItemsForSale = itemsForSale,
                Customers = customers
            };

            return View(model);
        }

        public IActionResult AddGames()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return View(new Products
            {
                ProductId = new int(),
                ProductName = string.Empty,
                ProductDescription = string.Empty,
                ProductImage = string.Empty,
                ProductPrice = 0.0M,
                UserId = userId,
                Stock = 0
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddGames(Products product)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                product.UserId = userId ?? throw new InvalidOperationException("User ID cannot be null");
                await _productService.AddItemAsync(product);
                return RedirectToAction("SellGames");
            }
            return View(product);
        }

        public async Task<IActionResult> EditGames(int id)
        {
            var product = await _productService.GetItemByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> EditGames(Products product)
        {
            if (ModelState.IsValid)
            {
                await _productService.UpdateItemAsync(product);
                return RedirectToAction("SellGames");
            }
            return View(product);
        }

        public async Task<IActionResult> DeleteItem(int id)
        {
            var product = await _productService.GetItemByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (product.UserId != userId)
            {
                return Unauthorized();
            }

            await _productService.DeleteItemAsync(id);
            return RedirectToAction("SellGames");
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
