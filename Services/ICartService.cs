using projekt_.NET_schronisko.Models;
using projekt_.NET_schronisko.ViewModels;

namespace projekt_.NET_schronisko.Services
{
    public interface ICartService
    {
        Task<int> GetCartItemCount(string userId); 
        Task AddToCart(string userId, int productId, int quantity); 

        Task<bool> PlaceOrderAsync(string userId, OrderFormViewModel model);
        Task<List<Orders>> GetOrdersByEmailAsync(string email);
    }
}
