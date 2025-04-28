using projekt_.NET_schronisko.Data;
using projekt_.NET_schronisko.Dto;
using projekt_.NET_schronisko.Interfaces;
using projekt_.NET_schronisko.Models;
using Microsoft.EntityFrameworkCore;
namespace projekt_.NET_schronisko.Services
{
    public class ProductService : IProductService
    {
        private readonly ShelterContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductService(ShelterContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<Product> CreateProductAsync(ProductDto productDto)
        {
            // Mapowanie DTO na encję. Możesz użyć AutoMapper lub zwykłego mapowania.
            var product = new Product
            {
                Name = productDto.Name,
                Price = productDto.Price,
                Category = productDto.Category,
                Images = new List<ProductImage>()
            };

            _context.Product.Add(product);
            await _context.SaveChangesAsync(); // Zapisujemy, aby uzyskać wygenerowane ID produktu.

            // Obsługa przesłanych zdjęć
            if (productDto.Images != null && productDto.Images.Any())
            {
                foreach (var image in productDto.Images)
                {
                    var fileName = Path.GetFileNameWithoutExtension(image.FileName) +
                                   "_" + Guid.NewGuid() +
                                   Path.GetExtension(image.FileName);
                    // Katalog docelowy np. wwwroot/images/products
                    var filePath = Path.Combine(_env.WebRootPath, "images", "products", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    var productImage = new ProductImage
                    {
                        ProductId = product.ID,
                        ImageUrl = "/images/products/" + fileName
                    };

                    _context.ProductImages.Add(productImage);
                }

                await _context.SaveChangesAsync();
            }

            return product;
        }

        public async Task<Product> UpdateProductAsync(ProductDto productDto)
        {
            // Wczytanie istniejącego produktu
            var product = await _context.Product.Include(p => p.Images)
                                                 .FirstOrDefaultAsync(p => p.ID == productDto.ID);
            if (product == null)
            {
                throw new Exception("Produkt nie został znaleziony");
            }

            // Aktualizacja podstawowych danych produktu (mapowanie właściwości)
            product.Name = productDto.Name;
            product.Price = productDto.Price;
            product.Category = productDto.Category;

            _context.Product.Update(product);
            await _context.SaveChangesAsync();

            // Obsługa nowych przesłanych zdjęć (dodanie do galerii)
            if (productDto.Images != null && productDto.Images.Any())
            {
                foreach (var image in productDto.Images)
                {
                    var fileName = Path.GetFileNameWithoutExtension(image.FileName) +
                                   "_" + Guid.NewGuid() +
                                   Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(_env.WebRootPath, "images", "products", fileName);
                    var directory = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    var productImage = new ProductImage
                    {
                        ProductId = product.ID,
                        ImageUrl = "/images/products/" + fileName
                    };

                    _context.ProductImages.Add(productImage);
                }
                await _context.SaveChangesAsync();
            }

            return product;
        }
    }

}
