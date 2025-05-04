using projekt_.NET_schronisko.ViewModels;

namespace projekt_.NET_schronisko.Services
{
    public interface ICartService
    {
        Task<int> GetCartItemCount(string userId); // Metoda do pobierania liczby przedmiotów w koszyku
        Task AddToCart(string userId, int productId, int quantity); // Metoda do dodania przedmiotu do koszyka

        Task<bool> PlaceOrderAsync(string userId, OrderFormViewModel model);
    }
}
