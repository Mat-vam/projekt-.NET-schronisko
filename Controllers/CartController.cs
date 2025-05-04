using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projekt_.NET_schronisko.Models;
using projekt_.NET_schronisko.Data;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using projekt_.NET_schronisko.ViewModels;

namespace projekt_.NET_schronisko.Controllers
{
    [Authorize(Roles = "Klient")]
    public class CartController : Controller
    {
        private readonly ShelterContext _context;
        private readonly UserManager<Users> _userManager;

        public CartController(ShelterContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Pomocnicza klasa do JSON
        public class CartAddRequest
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }

        // Wyświetlanie zawartości koszyka
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = user.Id,
                    Items = new List<CartItem>()
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            // Ustawienie łącznej ceny w ViewBag
            ViewBag.TotalPrice = cart.Items.Sum(i => i.Quantity * i.Product.Price);
            // Liczba produktów w koszyku w ViewData
            ViewData["CartItemCount"] = await GetCartItemCount(user.Id);

            return View(cart); // Przekazanie całego koszyka do widoku
        }

        // Dodawanie produktu do koszyka przez fetch
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart([FromBody] CartAddRequest request)
        {
            if (request == null || request.ProductId <= 0 || request.Quantity <= 0)
                return BadRequest("Nieprawidłowe dane.");

            var user = await _userManager.GetUserAsync(User);

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = user.Id,
                    Items = new List<CartItem>()
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var product = await _context.Product.FindAsync(request.ProductId);
            if (product == null)
                return NotFound("Nie znaleziono produktu.");

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += request.Quantity;
                existingItem.Price = existingItem.Quantity * product.Price;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = request.ProductId,
                    Product = product,
                    Quantity = request.Quantity,
                    Price = product.Price * request.Quantity
                });
            }

            await _context.SaveChangesAsync();

            var cartItemCount = await GetCartItemCount(user.Id);
            return Json(new { cartItemCount });
        }


        // Usuwanie produktu z koszyka
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var cartItem = await _context.CartItems
                .Include(ci => ci.Product)
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId);

            if (cartItem == null)
            {
                return Json(new { success = false, message = "Nie znaleziono elementu koszyka." });
            }

            var userId = cartItem.Cart.UserId;

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            var totalPrice = cart?.Items.Sum(i => i.Quantity * i.Product.Price) ?? 0;
            var cartItemCount = cart?.Items.Sum(i => i.Quantity) ?? 0;

            return Json(new
            {
                success = true,
                totalPrice = totalPrice.ToString("0.00"),
                cartItemCount
            });
        }

        // Zmiana ilości produktu w koszyku
        [HttpPost]
        public async Task<IActionResult> UpdateCartItemQuantity(int cartItemId, int quantity)
        {
            var cartItem = await _context.CartItems
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId);

            if (cartItem != null && quantity > 0)
            {
                cartItem.Quantity = quantity;
                cartItem.Price = cartItem.Product.Price * quantity;
                await _context.SaveChangesAsync();
            }

            var user = await _userManager.GetUserAsync(User);
            ViewData["CartItemCount"] = await GetCartItemCount(user.Id);

            return RedirectToAction("Index");
        }

        // Liczba produktów w koszyku
        public async Task<int> GetCartItemCount(string userId)
        {
            var cart = await _context.Carts
                                     .Include(c => c.Items)
                                     .FirstOrDefaultAsync(c => c.UserId == userId);

            return cart?.Items.Sum(item => item.Quantity) ?? 0;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder([FromBody] OrderFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Błędne dane." });

            var user = await _userManager.GetUserAsync(User);
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null || !cart.Items.Any())
                return BadRequest(new { success = false, message = "Koszyk jest pusty." });

            var order = new Orders
            {
                CustomerName = $"{model.FirstName} {model.LastName}",
                DeliveryAddress = $"{model.PostalCode}, {model.City}, {model.Address}",
                ProduktyZamowienia = cart.Items.Select(i => new OrderProduct
                {
                    ProductId = i.ProductId,
                    Ilosc = i.Quantity
                }).ToList()
            };

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cart.Items);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetCartItemCountAPI()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { count = 0 });

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            int count = cart?.Items.Sum(item => item.Quantity) ?? 0;

            return Json(new { count });
        }
    }
}
