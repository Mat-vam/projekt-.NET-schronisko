using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using projekt_.NET_schronisko.Data;
using projekt_.NET_schronisko.Interfaces;
using projekt_.NET_schronisko.Models;
using projekt_.NET_schronisko.Repository;
using projekt_.NET_schronisko.Services;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using projekt_.NET_schronisko.ViewModels;


namespace projekt_.NET_schronisko.Controllers
{
    public class AnimalsController : Controller
    {
        private readonly ShelterContext _context;
        private readonly IAnimalService _animalService;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<Users> _userManager;
        private readonly ICartService _cartService;

        public AnimalsController(ShelterContext context, IAnimalService animalService, IWebHostEnvironment env, UserManager<Users> userManager, ICartService cartService)
        {
            _context = context;
            _animalService = animalService;
            _env = env;
            _userManager = userManager;
            _cartService = cartService;
        }

        // GET: Animals
        public async Task<IActionResult> Index(string species, string adopted)
        {
            if (Request.Query["adoptionSuccess"] == "true")
            {
                ViewBag.adoptSuccess = true;
            }
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

            var animalsQuery = _context.Animal.Include(a => a.Images).AsQueryable();

            if (!string.IsNullOrEmpty(species))
            {
                animalsQuery = animalsQuery.Where(a => a.Species.ToLower() == species.ToLower());
            }
            if (!string.IsNullOrEmpty(adopted))
            {

                if (adopted.ToLower() == "true")
                {
                    animalsQuery = animalsQuery.Where(a => a.Status == "Adoptowany");
                }
                else if (adopted.ToLower() == "false")
                {
                    animalsQuery = animalsQuery.Where(a => a.Status != "Adoptowany");
                }
            }
            var speciesList = await _context.Animal
                .Select(a => a.Species)
                .Distinct()
                .ToListAsync();

            ViewBag.SpeciesList = speciesList;

            // reszta logiki kontrolera
            var animals = await animalsQuery.ToListAsync();
            return View(animals);

        }


        // GET: Animals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            int cartCount = 0;

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.UserId == user.Id);
                cartCount = cart?.Items.Sum(i => i.Quantity) ?? 0;
                ViewBag.FullName = user?.FullName ?? "";
                ViewBag.UserEmail = user?.Email ?? "";
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
            
            if (id == null)
            {
                return NotFound();
            }

            var animal = await _context.Animal.Include(p => p.Images)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (animal == null)
            {
                return NotFound();
            }

            return View(animal);
        }

        // GET: Animals/Create
        [Authorize(Roles = "Pracownik")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Animals/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Pracownik")]
        public async Task<IActionResult> Create(AnimalRepo animalRepo)
        {
            if (ModelState.IsValid)
            {
                await _animalService.CreateProductAsync(animalRepo); 
                return RedirectToAction(nameof(Index));
            }
            return View(animalRepo);
        }

        [Authorize(Roles = "Pracownik")]
        public async Task<IActionResult> Edit(int? id)

        {
            var animal = await _context.Animal.Include(p => p.Images)
                                     .FirstOrDefaultAsync(p => p.ID == id);
            if (animal == null)
            {
                return NotFound();
            }

            var animalRepo = new AnimalRepo
            {
                ID = animal.ID,
                Name = animal.Name,
                Species = animal.Species,
                Bio = animal.Bio,
                Status = animal.Status,
                Images = new List<IFormFile>(),
                ExistingImages = animal.Images?.Select(img => img.ImageUrl).ToList() ?? new List<string>()
            };

            return View(animalRepo);
        }

        // POST: Animals/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Pracownik")]
        public async Task<IActionResult> Edit(int id, AnimalRepo animalRepo)
        {
            if (id != animalRepo.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var animal = await _context.Animal.Include(p => p.Images)
                                                        .FirstOrDefaultAsync(p => p.ID == animalRepo.ID);
                    if (animal == null)
                    {
                        return NotFound();
                    }

                    if (animalRepo.ImagesToDelete != null && animalRepo.ImagesToDelete.Any())
                    {
                        var imagesToRemove = animal.Images
                            .Where(img => animalRepo.ImagesToDelete.Contains(img.ImageUrl))
                            .ToList();

                        foreach (var image in imagesToRemove)
                        {
                            var fullPath = Path.Combine(_env.WebRootPath, image.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(fullPath))
                            {
                                System.IO.File.Delete(fullPath);
                            }

                            _context.AnimalImages.Remove(image);
                        }

                        await _context.SaveChangesAsync();
                    }

                    await _animalService.UpdateProductAsync(animalRepo);

                }
                catch (Exception)
                {
                    if (!AnimalExists(animalRepo.ID))
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
            return View(animalRepo);
        }

        // GET: Animals/Delete/5
        [Authorize(Roles = "Pracownik")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animal = await _context.Animal
                .FirstOrDefaultAsync(m => m.ID == id);
            if (animal == null)
            {
                return NotFound();
            }

            return View(animal);
        }

        // POST: Animals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _animalService.DeleteProductAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AnimalExists(int id)
        {
            return _context.Animal.Any(e => e.ID == id);
        }
        [HttpPost]
        public async Task<IActionResult> SubmitAdoption([FromBody] AdoptionRequestViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Nieprawidłowe dane." });

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var request = new AdoptionRequest
            {
                AnimalId = model.AnimalId,
                FullName = user.FullName,
                Email = user.Email,
                PostalCode = model.PostalCode,
                City = model.City,
                Address = model.Address,
                PhoneNumber = model.PhoneNumber,
                AboutMe = model.AboutMe,
                SubmittedAt = DateTime.Now
            };

            _context.AdoptionRequests.Add(request);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Zgłoszenie adopcji zostało zapisane." });
        }
    }
}
