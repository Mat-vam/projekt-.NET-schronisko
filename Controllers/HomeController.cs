using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using projekt_.NET_schronisko.Models;
using projekt_.NET_schronisko.Services;
using System.Threading.Tasks;
using System.Diagnostics;

namespace projekt_.NET_schronisko.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICartService _cartService;
        private readonly UserManager<Users> _userManager;

        public HomeController(ILogger<HomeController> logger, ICartService cartService, UserManager<Users> userManager)
        {
            _logger = logger;
            _cartService = cartService;
            _userManager = userManager;
        }

        // G³ówna strona - Index
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var cartItemCount = await _cartService.GetCartItemCount(user.Id);
                ViewData["CartItemCount"] = cartItemCount;
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
