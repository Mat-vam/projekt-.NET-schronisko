using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using projekt_.NET_schronisko.Data;
using projekt_.NET_schronisko.Repository;
using projekt_.NET_schronisko.Interfaces;
using projekt_.NET_schronisko.Models;
using projekt_.NET_schronisko.Services;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;


namespace projekt_.NET_schronisko.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ShelterContext _context;
        private readonly IProductService _productService;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<Users> _userManager;
        private readonly ICartService _cartService;

        public ProductsController(ShelterContext context, IProductService productService, IWebHostEnvironment env, UserManager<Users> userManager, ICartService cartService)
        {
            _context = context;
            _productService = productService;
            _env = env;
            _userManager = userManager;
            _cartService = cartService;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            // ustawianie cartItemCount
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

            // reszta logiki kontrolera
            var products = await _context.Product.Include(p => p.Images).ToListAsync();
            return View(products);
        }


        // GET: Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Product
                .Include(p => p.Images)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }


        // GET: Products/Create
        [Authorize(Roles = "Pracownik")]
        public IActionResult Create()
        {
            var categories = new List<string> { "Zabawka", "Karma", "Akcesoria" };

            ViewBag.Categories = new SelectList(categories);

            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Pracownik")]
        public async Task<IActionResult> Create(ProductRepo productRepo)
        {
            if (ModelState.IsValid)
            {
                await _productService.CreateProductAsync(productRepo);
                return RedirectToAction(nameof(Index));
            }

            var categories = new List<string> { "Zabawka", "Karma", "Akcesoria" };
            ViewBag.Categories = new SelectList(categories);

            return View(productRepo);
        }






        [Authorize(Roles = "Pracownik")]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Product.Include(p => p.Images)
                                                 .FirstOrDefaultAsync(p => p.ID == id);
            if (product == null)
            {
                return NotFound();
            }

            var productRepo = new ProductRepo
            {
                ID = product.ID,
                Name = product.Name,
                Price = product.Price,
                Category = product.Category,
                Images = new List<IFormFile>() 
            };

            var categories = new List<string> { "Zabawka", "Karma", "Akcesoria" };
            ViewBag.Categories = new SelectList(categories);

            return View(productRepo);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Pracownik")]
        public async Task<IActionResult> Edit(int id, ProductRepo productRepo)
        {
            if (id != productRepo.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _productService.UpdateProductAsync(productRepo);
                }
                catch (Exception)
                {
                    if (!ProductExists(productRepo.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            var categories = new List<string> { "Zabawka", "Karma", "Akcesoria" };
            ViewBag.Categories = new SelectList(categories);

            return View(productRepo);
        }


        // GET: Products/Delete/5
        [Authorize(Roles = "Pracownik")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .FirstOrDefaultAsync(m => m.ID == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Pracownik")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _productService.DeleteProductAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }


        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.ID == id);
        }
    }
}
