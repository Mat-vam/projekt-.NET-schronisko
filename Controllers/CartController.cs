using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using projekt_.NET_schronisko.Models;
using projekt_.NET_schronisko.Data;
using projekt_.NET_schronisko.ViewModels;
using projekt_.NET_schronisko.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace projekt_.NET_schronisko.Controllers
{
    public class CartController : Controller
    {
        private readonly ShelterContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly ICartService _cartService;

        public CartController(ShelterContext context, UserManager<Users> userManager, ICartService cartService)
        {
            _context = context;
            _userManager = userManager;
            _cartService = cartService;
        }

        public class CartAddRequest
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                ViewBag.FullName = user.FullName;
                ViewBag.UserEmail = user.Email;
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

                ViewBag.TotalPrice = cart.Items.Sum(i => i.Quantity * i.Product.Price);
                ViewData["CartItemCount"] = await GetCartItemCount(user.Id);
                return View(cart);
            }
            else
            {
                var sessionCartJson = HttpContext.Session.GetString("Cart");
                var cartData = sessionCartJson != null
                    ? JsonSerializer.Deserialize<Dictionary<int, int>>(sessionCartJson)
                    : new Dictionary<int, int>();

                var items = new List<CartItem>();
                decimal total = 0;

                foreach (var kvp in cartData)
                {
                    var product = await _context.Product.FindAsync(kvp.Key);
                    if (product != null)
                    {
                        items.Add(new CartItem
                        {
                            Product = product,
                            ProductId = product.ID,
                            Quantity = kvp.Value,
                            Price = product.Price * kvp.Value
                        });
                        total += product.Price * kvp.Value;
                    }
                }

                ViewBag.TotalPrice = total;
                ViewData["CartItemCount"] = cartData.Values.Sum();

                var tempCart = new Cart
                {
                    Items = items
                };

                return View(tempCart);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart([FromBody] CartAddRequest request)
        {
            if (request == null || request.ProductId <= 0 || request.Quantity <= 0)
                return BadRequest("Nieprawidłowe dane.");

            if (User.Identity.IsAuthenticated)
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
            else
            {
                var sessionCartJson = HttpContext.Session.GetString("Cart");
                var cart = sessionCartJson != null
                    ? JsonSerializer.Deserialize<Dictionary<int, int>>(sessionCartJson)
                    : new Dictionary<int, int>();

                if (cart.ContainsKey(request.ProductId))
                    cart[request.ProductId] += request.Quantity;
                else
                    cart[request.ProductId] = request.Quantity;

                HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));

                int cartItemCount = cart.Values.Sum();
                return Json(new { cartItemCount });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            if (User.Identity.IsAuthenticated)
            {
                var cartItem = await _context.CartItems
                    .Include(ci => ci.Product)
                    .Include(ci => ci.Cart)
                    .FirstOrDefaultAsync(ci => ci.Id == cartItemId);

                if (cartItem == null)
                    return Json(new { success = false, message = "Nie znaleziono elementu koszyka." });

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
            else
            {
                // niezalogowany – sesja
                var sessionCart = HttpContext.Session.GetString("Cart");
                var cart = !string.IsNullOrEmpty(sessionCart)
                    ? JsonSerializer.Deserialize<Dictionary<int, int>>(sessionCart)
                    : new Dictionary<int, int>();

                if (cart.ContainsKey(cartItemId))
                {
                    cart.Remove(cartItemId);
                    HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));

                    // przelicz sumę
                    decimal totalPrice = 0;
                    foreach (var kvp in cart)
                    {
                        var product = await _context.Product.FindAsync(kvp.Key);
                        if (product != null)
                        {
                            totalPrice += product.Price * kvp.Value;
                        }
                    }

                    int cartItemCount = cart.Values.Sum();

                    return Json(new
                    {
                        success = true,
                        totalPrice = totalPrice.ToString("0.00"),
                        cartItemCount
                    });
                }

                return Json(new { success = false, message = "Produkt nie istnieje w koszyku." });
            }
        }


        [HttpPost]
        public async Task<IActionResult> UpdateCartItemQuantity(int cartItemId, int quantity)
        {
            if (User.Identity.IsAuthenticated)
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
            }
            else
            {
                // niezalogowany – operuj na sesji
                var sessionCart = HttpContext.Session.GetString("Cart");
                if (!string.IsNullOrEmpty(sessionCart))
                {
                    var cart = JsonSerializer.Deserialize<Dictionary<int, int>>(sessionCart);
                    if (cart != null && cart.ContainsKey(cartItemId) && quantity > 0)
                    {
                        cart[cartItemId] = quantity;
                        HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
                        ViewData["CartItemCount"] = cart.Values.Sum();
                    }
                }
            }

            return RedirectToAction("Index");
        }


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
            model.FullName = user.FullName;
            model.Email = user.Email;

            var success = await _cartService.PlaceOrderAsync(user.Id, model);

            return Json(new
            {
                success = success,
                message = success ? "Zamówienie zostało złożone." : "Koszyk jest pusty lub wystąpił błąd."
            });
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
