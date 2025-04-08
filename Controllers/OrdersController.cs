using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projekt_.NET_schronisko.Models;
using Microsoft.AspNetCore.Authorization;
using projekt_.NET_schronisko.Data;

namespace projekt_.NET_schronisko.Controllers
{
    [Authorize(Roles = "Pracownik")]
    public class OrdersController : Controller
    {
        private readonly ShelterContext _context;

        public OrdersController(ShelterContext context)
        {
            _context = context;
        }

        // GET: Orders/Index
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.ProduktyZamowienia)
                .ThenInclude(op => op.Product)
                .ToListAsync();

            return View(orders);
        }

        // Akcja do zmiany statusu zamówienia
        public async Task<IActionResult> UpdateStatus(int id, string newStatus)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                order.Status = newStatus;
                _context.Update(order);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
