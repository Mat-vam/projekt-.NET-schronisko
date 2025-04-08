namespace projekt_.NET_schronisko.Services
{
    public interface ICartService
    {
        Task<int> GetCartItemCount(string userId); // Metoda do pobierania liczby przedmiotów w koszyku
        Task AddToCart(string userId, string productId, int quantity); // Metoda do dodania przedmiotu do koszyka
    }
}
