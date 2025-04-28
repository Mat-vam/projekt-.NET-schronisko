using projekt_.NET_schronisko.Dto;
using projekt_.NET_schronisko.Models;

namespace projekt_.NET_schronisko.Interfaces
{
    public interface IProductService
    {
        Task<Product> CreateProductAsync(ProductDto productDto);
        Task<Product> UpdateProductAsync(ProductDto productDto);
    }

}
