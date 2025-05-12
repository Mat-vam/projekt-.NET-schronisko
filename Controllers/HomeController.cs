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

        // G³ówna strona - Index
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
                // Niezalogowany – licz koszyk z sesji
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

        // Strona Prywatnoœci
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
