using projekt_.NET_schronisko.Repository;
using projekt_.NET_schronisko.Models;

namespace projekt_.NET_schronisko.Interfaces
{
    public interface IProductService
    {
        Task<Product> CreateProductAsync(ProductRepo productRepo);
        Task<Product> UpdateProductAsync(ProductRepo productRepo);
        Task<bool> DeleteProductAsync(int id);
    }

}
