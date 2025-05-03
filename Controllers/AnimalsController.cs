using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using projekt_.NET_schronisko.Data;
using projekt_.NET_schronisko.Interfaces;
using projekt_.NET_schronisko.Models;
using projekt_.NET_schronisko.Repository;
using projekt_.NET_schronisko.Services;

namespace projekt_.NET_schronisko.Controllers
{
    public class AnimalsController : Controller
    {
        private readonly ShelterContext _context;
        private readonly IAnimalService _animalService;
        private readonly IWebHostEnvironment _env;

        public AnimalsController(ShelterContext context, IAnimalService animalService, IWebHostEnvironment env)
        {
            _context = context;
            _animalService = animalService;
            _env = env;
        }

        // GET: Animals
        public async Task<IActionResult> Index(string species, bool? adopted)
        {
            var animals = _context.Animal.Include(p=>p.Images).AsQueryable();

            if (!string.IsNullOrEmpty(species))
            {
                animals = animals.Where(a => a.Species.ToLower() == species.ToLower());
            }

            if (adopted.HasValue)
            {
                animals = animals.Where(a => a.Status == (adopted.Value ? "adoptowany" : "nieadoptowany"));
            }

            return View(await animals.ToListAsync());
        }

        // GET: Animals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
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
                Images = new List<IFormFile>()
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
    }
}
