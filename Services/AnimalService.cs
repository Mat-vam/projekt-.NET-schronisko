using Microsoft.EntityFrameworkCore;
using projekt_.NET_schronisko.Data;
using projekt_.NET_schronisko.Interfaces;
using projekt_.NET_schronisko.Models;
using projekt_.NET_schronisko.Repository;

namespace projekt_.NET_schronisko.Services
{
    public class AnimalService : IAnimalService
    {
        private readonly ShelterContext _context;
        private readonly IWebHostEnvironment _env;

        public AnimalService(ShelterContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<Animal> CreateProductAsync(AnimalRepo animalRepo)
        {
            // Mapowanie DTO na encję. Możesz użyć AutoMapper lub zwykłego mapowania.
            var animal = new Animal
            {
                ID = animalRepo.ID,
                Name = animalRepo.Name,
                Species = animalRepo.Species,
                Bio = animalRepo.Bio,
                Status = animalRepo.Status,
                Images = new List<AnimalImage>()
            };

            _context.Animal.Add(animal);
            await _context.SaveChangesAsync(); // Zapisujemy, aby uzyskać wygenerowane ID produktu.

            // Obsługa przesłanych zdjęć
            if (animalRepo.Images != null && animalRepo.Images.Any())
            {
                foreach (var image in animalRepo.Images)
                {
                    var fileName = Path.GetFileNameWithoutExtension(image.FileName) +
                                   "_" + Guid.NewGuid() +
                                   Path.GetExtension(image.FileName);
                    // Katalog docelowy np. wwwroot/images/products
                    var filePath = Path.Combine(_env.WebRootPath, "images", "animals", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    var animalImage = new AnimalImage
                    {
                        AnimalId = animal.ID,
                        ImageUrl = "/images/animals/" + fileName
                    };

                    _context.AnimalImages.Add(animalImage);
                }

                await _context.SaveChangesAsync();
            }

            return animal;
        }

        public async Task<Animal> UpdateProductAsync(AnimalRepo animalRepo)
        {
            // Wczytanie istniejącego produktu
            var animal = await _context.Animal.Include(p => p.Images)
                                                 .FirstOrDefaultAsync(p => p.ID == animalRepo.ID);
            if (animal == null)
            {
                throw new Exception("Produkt nie został znaleziony");
            }

            // Aktualizacja podstawowych danych produktu (mapowanie właściwości)


            animal.ID = animalRepo.ID;
            animal.Name = animalRepo.Name;
            animal.Species = animalRepo.Species;
            animal.Bio = animalRepo.Bio;
            animal.Status = animalRepo.Status;


            _context.Animal.Update(animal);
            await _context.SaveChangesAsync();

            // Obsługa nowych przesłanych zdjęć (dodanie do galerii)
            if (animalRepo.Images != null && animalRepo.Images.Any())
            {
                foreach (var image in animalRepo.Images)
                {
                    var fileName = Path.GetFileNameWithoutExtension(image.FileName) +
                                   "_" + Guid.NewGuid() +
                                   Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(_env.WebRootPath, "images", "animals", fileName);
                    var directory = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    var animalImage = new AnimalImage
                    {
                        AnimalId = animal.ID,
                        ImageUrl = "/images/animals/" + fileName
                    };

                    _context.AnimalImages.Add(animalImage);
                }
                await _context.SaveChangesAsync();
            }

            return animal;
        }
        public async Task<bool> DeleteProductAsync(int id)
        {
            var animal = await _context.Animal
                                        .Include(p => p.Images)
                                        .FirstOrDefaultAsync(p => p.ID == id);

            if (animal == null)
                return false;

            foreach (var image in animal.Images)
            {
                var fullPath = Path.Combine(_env.WebRootPath, image.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }

            _context.Animal.Remove(animal);
            await _context.SaveChangesAsync();

            return true;
        }

    }
}
