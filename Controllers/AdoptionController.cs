using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projekt_.NET_schronisko.Data;

namespace projekt_.NET_schronisko.Controllers
{
    [Authorize(Roles = "Pracownik")]
    public class AdoptionController : Controller
    {
        private readonly ShelterContext _context;

        public AdoptionController(ShelterContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string statusFilter)
        {
            var query = _context.AdoptionRequests.Include(r => r.Animal).AsQueryable();

            if (!string.IsNullOrEmpty(statusFilter))
            {
                query = query.Where(r => r.Animal.Status == statusFilter);
            }

            var requests = await query.ToListAsync();

            ViewBag.StatusFilter = statusFilter; 

            return View(requests);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateAnimalStatus(int id, string newStatus)
        {
            var animal = await _context.Animal.FindAsync(id);
            if (animal == null)
            {
                return NotFound();
            }

            animal.Status = newStatus;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }

}
