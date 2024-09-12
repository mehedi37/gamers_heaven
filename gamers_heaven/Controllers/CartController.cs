using gamers_heaven.Data;
using gamers_heaven.Services;
using gamers_heaven.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace gamers_heaven.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly AppDbContext _context;
        private readonly CartService _cartService;
        private readonly ProductService _productService;

        public CartController(AppDbContext context, CartService cartService, ProductService productService)
        {
            _context = context;
            _cartService = cartService;
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cart = await _cartService.GetCartByUserIdAsync(userId);

            if (cart == null || !cart.CartDetails.Any())
            {
                ViewBag.Message = "Your cart is empty.";
                return View(new CartViewModel());
            }

            var cartViewModel = new CartViewModel
            {
                CartId = cart.CartId,
                CartDetails = cart.CartDetails.ToList(),
                TotalPrice = cart.CartDetails.Sum(cd => cd.Price * cd.Quantity)
            };

            return View(cartViewModel);
        }


        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var product = await _context.Products.FindAsync(productId);

            if (product == null)
            {
                return BadRequest("Product not found.");
            }

            var cart = await _cartService.GetCartByUserIdAsync(userId);
            var cartDetail = cart?.CartDetails.FirstOrDefault(cd => cd.ProductId == productId);

            if (cartDetail != null)
            {
                int newQuantity = cartDetail.Quantity + quantity;
                if (newQuantity > product.Stock)
                {
                    newQuantity = product.Stock;
                    TempData["Message"] = $"Only {product.Stock} items available for {product.ProductName}.";
                }
                await _cartService.UpdateCartAsync(cartDetail.CartDetailsId, newQuantity);
            }
            else
            {
                if (quantity > product.Stock)
                {
                    quantity = product.Stock;
                    TempData["Message"] = $"Only {product.Stock} items available for {product.ProductName}.";
                }
                await _cartService.AddToCartAsync(userId, productId, quantity);
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> UpdateCart(int cartDetailsId, int quantity)
        {
            if (quantity < 1)
            {
                quantity = 1;
            }

            // Retrieve the product from the database
            var cartDetail = await _context.CartDetails.FindAsync(cartDetailsId);
            var product = await _context.Products.FindAsync(cartDetail.ProductId);

            // Check if the quantity is greater than the product's stock
            if (quantity > product.Stock)
            {
                quantity = product.Stock;
                TempData["StockMessage"] = "The quantity has been adjusted to the available stock.";
            }

            await _cartService.UpdateCartAsync(cartDetailsId, quantity);
            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartDetailsId)
        {
            await _cartService.RemoveFromCartAsync(cartDetailsId);
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _cartService.ClearCartAsync(userId);
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> Purchase()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cart = await _cartService.GetCartByUserIdAsync(userId);

            if (cart == null || !cart.CartDetails.Any())
            {
                return BadRequest("Your cart is empty.");
            }

            foreach (var cartDetail in cart.CartDetails)
            {
                var product = await _context.Products.FindAsync(cartDetail.ProductId);
                if (product == null || product.Stock < cartDetail.Quantity)
                {
                    return BadRequest("Insufficient stock for product: " + product?.ProductName);
                }

                product.Stock -= cartDetail.Quantity;
                await _productService.UpdateItemAsync(product);
            }

            await _cartService.PurchaseCartAsync(userId);
            return RedirectToAction("Index");
        }

    }
}
