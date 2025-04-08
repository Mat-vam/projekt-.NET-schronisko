using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projekt_.NET_schronisko.Models;
using projekt_.NET_schronisko.Data;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;

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

        // Akcja do wyświetlania zawartości koszyka
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
                    UserId = user.Id
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            // Aktualizujemy ViewData o liczbę produktów w koszyku
            ViewData["CartItemCount"] = await GetCartItemCount(user.Id);

            return View(cart);
        }

        // Akcja do dodawania produktów do koszyka
        [HttpPost]
        public async Task<IActionResult> AddToCart(string productId, int quantity)
        {
            var user = await _userManager.GetUserAsync(User);
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = user.Id
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var product = await _context.Product.FindAsync(productId);

            if (product != null && quantity > 0)
            {
                var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

                if (existingItem != null)
                {
                    // Jeśli produkt już istnieje w koszyku, zwiększamy ilość
                    existingItem.Quantity += quantity;
                    existingItem.Price = product.Price * existingItem.Quantity;
                }
                else
                {
                    // Jeśli produkt nie istnieje, dodajemy go do koszyka
                    cart.Items.Add(new CartItem
                    {
                        ProductId = productId,
                        Product = product,
                        Quantity = quantity,
                        Price = product.Price * quantity
                    });
                }

                await _context.SaveChangesAsync();
            }

            // Aktualizujemy ViewData o liczbę produktów w koszyku
            ViewData["CartItemCount"] = await GetCartItemCount(user.Id);

            // Zwracamy liczbę produktów w koszyku w odpowiedzi JSON
            return Json(new { cartItemCount = ViewData["CartItemCount"] });
        }

        // Akcja do usuwania produktu z koszyka
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);

            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }

            // Aktualizujemy ViewData o liczbę produktów w koszyku
            var user = await _userManager.GetUserAsync(User);
            ViewData["CartItemCount"] = await GetCartItemCount(user.Id);

            return RedirectToAction("Index");
        }

        // Akcja do zmiany ilości produktu w koszyku
        [HttpPost]
        public async Task<IActionResult> UpdateCartItemQuantity(int cartItemId, int quantity)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);

            if (cartItem != null && quantity > 0)
            {
                cartItem.Quantity = quantity;
                cartItem.Price = cartItem.Product.Price * quantity;
                await _context.SaveChangesAsync();
            }

            // Aktualizujemy ViewData o liczbę produktów w koszyku
            var user = await _userManager.GetUserAsync(User);
            ViewData["CartItemCount"] = await GetCartItemCount(user.Id);

            return RedirectToAction("Index");
        }

        // Pomocnicza metoda do uzyskania liczby przedmiotów w koszyku
        public async Task<int> GetCartItemCount(string userId)
        {
            var cart = await _context.Carts
                                     .Include(c => c.Items)
                                     .FirstOrDefaultAsync(c => c.UserId == userId);

            return cart?.Items.Sum(item => item.Quantity) ?? 0;
        }
    }
}
