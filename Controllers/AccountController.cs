using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using projekt_.NET_schronisko.Models;
using projekt_.NET_schronisko.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Google;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using projekt_.NET_schronisko.Data;

namespace projekt_.NET_schronisko.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Users> _signInManager;
        private readonly UserManager<Users> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ShelterContext _context;

        public AccountController(SignInManager<Users> signInManager, UserManager<Users> userManager, RoleManager<IdentityRole> roleManager, ShelterContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user != null)
                        await TransferSessionCartToUser(user.Id);

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Email lub hasło jest niepoprawne");
            }
            return View(model);
        }

        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new Users
                {
                    FullName = model.Name,
                    Email = model.Email,
                    UserName = model.Email
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("Klient"))
                        await _roleManager.CreateAsync(new IdentityRole("Klient"));

                    await _userManager.AddToRoleAsync(user, "Klient");

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    await TransferSessionCartToUser(user.Id);

                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }

        public IActionResult VerifyEmail() => View();

        [HttpPost]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", "Podano niepoprawny Email");
                    return View(model);
                }
                return RedirectToAction("ChangePassword", "Account", new { username = user.UserName });
            }
            return View(model);
        }

        public IActionResult ChangePassword(string username)
        {
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("VerifyEmail", "Account");

            return View(new ChangePasswordViewModel { Email = username });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Email);
                if (user != null)
                {
                    var result = await _userManager.RemovePasswordAsync(user);
                    if (result.Succeeded)
                    {
                        result = await _userManager.AddPasswordAsync(user, model.NewPassword);
                        return RedirectToAction("Login", "Account");
                    }

                    foreach (var error in result.Errors)
                        ModelState.AddModelError("", error.Description);
                }
                else
                {
                    ModelState.AddModelError("", "Nie znaleziono Emailu");
                }
            }
            else
            {
                ModelState.AddModelError("", "Coś poszło nie tak. Spróbuj ponownie");
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Remove("Cart");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult GoogleLogin(string returnUrl = "/")
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse", new { returnUrl }) };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> GoogleResponse(string returnUrl = "/")
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!result.Succeeded)
                return RedirectToAction("Login");

            var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (email == null || !email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Login");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new Users
                {
                    UserName = email,
                    Email = email,
                    FullName = name
                };
                await _userManager.CreateAsync(user);
                await _userManager.AddToRoleAsync(user, "Klient");
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            await TransferSessionCartToUser(user.Id);

            return LocalRedirect(returnUrl);
        }

        private async Task TransferSessionCartToUser(string userId)
        {
            var sessionCartJson = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(sessionCartJson)) return;

            var sessionCart = JsonSerializer.Deserialize<Dictionary<int, int>>(sessionCartJson);
            if (sessionCart == null || !sessionCart.Any()) return;

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId, Items = new List<CartItem>() };
                _context.Carts.Add(cart);
            }

            foreach (var entry in sessionCart)
            {
                var product = await _context.Product.FindAsync(entry.Key);
                if (product == null) continue;

                var existing = cart.Items.FirstOrDefault(i => i.ProductId == entry.Key);
                if (existing != null)
                {
                    existing.Quantity += entry.Value;
                    existing.Price = existing.Quantity * product.Price;
                }
                else
                {
                    cart.Items.Add(new CartItem
                    {
                        ProductId = entry.Key,
                        Product = product,
                        Quantity = entry.Value,
                        Price = product.Price * entry.Value
                    });
                }
            }

            await _context.SaveChangesAsync();
            HttpContext.Session.Remove("Cart");
        }
    }
}
