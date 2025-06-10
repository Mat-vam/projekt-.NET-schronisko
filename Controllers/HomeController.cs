using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using projekt_.NET_schronisko.Models;
using projekt_.NET_schronisko.Services;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.Json;
using projekt_.NET_schronisko.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace projekt_.NET_schronisko.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICartService _cartService;
        private readonly UserManager<Users> _userManager;
        private readonly ShelterContext _context;

        public HomeController(ILogger<HomeController> logger, ICartService cartService, UserManager<Users> userManager, ShelterContext context)
        {
            _logger = logger;
            _cartService = cartService;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (User.Identity.IsAuthenticated)
            {
                ViewBag.FullName = user?.FullName;

                if (user != null)
                {
                    var cartItemCount = await _cartService.GetCartItemCount(user.Id);
                    ViewData["CartItemCount"] = cartItemCount;
                }
            }
            else
            {
                var sessionCart = HttpContext.Session.GetString("Cart");
                if (!string.IsNullOrEmpty(sessionCart))
                {
                    var cartData = JsonSerializer.Deserialize<Dictionary<int, int>>(sessionCart);
                    ViewData["CartItemCount"] = cartData?.Values.Sum() ?? 0;
                }
                else
                {
                    ViewData["CartItemCount"] = 0;
                }
            }

            return View();
        }

        [Authorize(Roles = "Klient")]
        public async Task<IActionResult> History(string sortBy, string statusFilter)
        {
            int cartCount = 0;

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.UserId == user.Id);
                cartCount = cart?.Items.Sum(i => i.Quantity) ?? 0;
            }
            else
            {
                var sessionCart = HttpContext.Session.GetString("Cart");
                if (!string.IsNullOrEmpty(sessionCart))
                {
                    var cartData = JsonSerializer.Deserialize<Dictionary<int, int>>(sessionCart);
                    cartCount = cartData?.Values.Sum() ?? 0;
                }
            }

            ViewData["CartItemCount"] = cartCount;

            var email = User.Identity?.Name;
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            var orders = await _cartService.GetOrdersByEmailAsync(email);

            if (!string.IsNullOrEmpty(statusFilter))
            {
                orders = orders.Where(o => o.Status == statusFilter).ToList();
            }

            switch (sortBy)
            {
                case "date_asc":
                    orders = orders.OrderBy(o => o.DataZamowienia).ToList();
                    break;
                case "date_desc":
                    orders = orders.OrderByDescending(o => o.DataZamowienia).ToList();
                    break;
                case "total_asc":
                    orders = orders.OrderBy(o => o.TotalAmount).ToList();
                    break;
                case "total_desc":
                    orders = orders.OrderByDescending(o => o.TotalAmount).ToList();
                    break;
                default:
                    orders = orders.OrderByDescending(o => o.DataZamowienia).ToList();
                    break;
            }

            return View(orders);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
