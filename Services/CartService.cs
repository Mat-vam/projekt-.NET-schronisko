using projekt_.NET_schronisko.Models;
using Microsoft.EntityFrameworkCore;
using projekt_.NET_schronisko.Data;
using projekt_.NET_schronisko.ViewModels;

namespace projekt_.NET_schronisko.Services
{
    public class CartService : ICartService
    {
        private readonly ShelterContext _context;

        public CartService(ShelterContext context)
        {
            _context = context;
        }

        // Zwraca liczbę produktów w koszyku użytkownika
        public async Task<int> GetCartItemCount(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return cart?.Items.Sum(i => i.Quantity) ?? 0;
        }

        // Dodaje przedmiot do koszyka
        public async Task AddToCart(string userId, int productId, int quantity)
        {
            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                var product = await _context.Product.FindAsync(productId);
                if (product != null)
                {
                    cart.Items.Add(new CartItem
                    {
                        ProductId = productId,
                        Product = product,
                        Quantity = quantity,
                        Price = product.Price
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> PlaceOrderAsync(string userId, OrderFormViewModel model)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || cart.Items.Count == 0)
                return false;

            var order = new Orders
            {
                CustomerName = $"{model.FirstName} {model.LastName}",
                DeliveryAddress = $"{model.PostalCode}, {model.City}, {model.Address}",
                ProduktyZamowienia = cart.Items
                    .GroupBy(i => i.ProductId)
                    .Select(g => new OrderProduct
                    {
                        ProductId = g.Key,
                        Ilosc = g.Sum(i => i.Quantity)
                    }).ToList()
            };

            _context.Orders.Add(order);

            _context.Carts.Remove(cart); // usunięcie całego koszyka

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
