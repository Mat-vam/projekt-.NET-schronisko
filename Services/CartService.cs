using projekt_.NET_schronisko.Models;
using Microsoft.EntityFrameworkCore;
using projekt_.NET_schronisko.Data;

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
        public async Task AddToCart(string userId, string productId, int quantity)
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
    }
}
