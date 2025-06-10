using projekt_.NET_schronisko.Data;
using projekt_.NET_schronisko.Repository;
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

        public async Task<Product> CreateProductAsync(ProductRepo productRepo)
        {
            var product = new Product
            {
                Name = productRepo.Name,
                Price = productRepo.Price,
                Category = productRepo.Category,
                Images = new List<ProductImage>()
            };

            _context.Product.Add(product);
            await _context.SaveChangesAsync();

            if (productRepo.Images != null && productRepo.Images.Any())
            {
                foreach (var image in productRepo.Images)
                {
                    var fileName = Path.GetFileNameWithoutExtension(image.FileName) +
                                   "_" + Guid.NewGuid() +
                                   Path.GetExtension(image.FileName);

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

        public async Task<Product> UpdateProductAsync(ProductRepo productRepo)
        {
            var product = await _context.Product.Include(p => p.Images)
                                                 .FirstOrDefaultAsync(p => p.ID == productRepo.ID);
            if (product == null)
            {
                throw new Exception("Produkt nie został znaleziony");
            }

            product.Name = productRepo.Name;
            product.Price = productRepo.Price;
            product.Category = productRepo.Category;

            _context.Product.Update(product);
            await _context.SaveChangesAsync();

            if (productRepo.Images != null && productRepo.Images.Any())
            {
                foreach (var image in productRepo.Images)
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
        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _context.Product
                                        .Include(p => p.Images)
                                        .FirstOrDefaultAsync(p => p.ID == id);

            if (product == null)
                return false;

            foreach (var image in product.Images)
            {
                var fullPath = Path.Combine(_env.WebRootPath, image.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }

            _context.Product.Remove(product);
            await _context.SaveChangesAsync();

            return true;
        }

    }

}
