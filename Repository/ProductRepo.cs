using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace projekt_.NET_schronisko.Repository
{
    public class ProductRepo
    {
        public int ID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public string Category { get; set; }
        [ValidateNever]
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
        [ValidateNever]
        public List<string> ExistingImages { get; set; } = new List<string>();
        [ValidateNever]
        public List<string> ImagesToDelete { get; set; } = new List<string>();


    }
}
