using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projekt_.NET_schronisko.Models;
using Microsoft.AspNetCore.Authorization;
using projekt_.NET_schronisko.Data;
using projekt_.NET_schronisko.Services;

namespace projekt_.NET_schronisko.Controllers
{
    [Authorize(Roles = "Pracownik")]
    public class OrdersController : Controller
    {
        private readonly ShelterContext _context;
        private readonly ICartService _cartService;

        public OrdersController(ShelterContext context, ICartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        // GET: Orders/Index
        public async Task<IActionResult> Index(string sortBy, string statusFilter)
        {
            var ordersQuery = _context.Orders
                .Include(o => o.ProduktyZamowienia)
                .ThenInclude(op => op.Product)
                .AsQueryable();

            // Filtrowanie po statusie
            if (!string.IsNullOrEmpty(statusFilter))
            {
                ordersQuery = ordersQuery.Where(o => o.Status == statusFilter);
            }

            // Pobierz do pamięci (potrzebne dla TotalAmount, jeśli nie jest w bazie)
            var orders = await ordersQuery.ToListAsync();

            // Sortowanie w pamięci
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
                    orders = orders.OrderByDescending(o => o.DataZamowienia).ToList(); // domyślnie najnowsze
                    break;
            }

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
