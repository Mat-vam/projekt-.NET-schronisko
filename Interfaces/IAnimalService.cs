using projekt_.NET_schronisko.Models;
using projekt_.NET_schronisko.Repository;

namespace projekt_.NET_schronisko.Interfaces
{
    public interface IAnimalService
    {
        Task<Animal> CreateProductAsync(AnimalRepo animalRepo);
        Task<Animal> UpdateProductAsync(AnimalRepo animalRepo);
        Task<bool> DeleteProductAsync(int id);
    }
}
