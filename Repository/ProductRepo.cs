using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace projekt_.NET_schronisko.Repository
{
    public class ProductRepo
    {
        public int ID { get; set; }
        [Display(Name = "Nazwa")]
        [Required(ErrorMessage = "Nazwa produktu jest wymagana.")]
        [StringLength(100)]
        public string Name { get; set; }
        [Display(Name = "Cena")]
        [Required(ErrorMessage = "Cena produktu jest wymagana.")]
        [Range(0.01, 10000, ErrorMessage = "Cena musi być większa niż 0.")]
        public decimal Price { get; set; }
        [Display(Name = "Kategoria")]
        [Required(ErrorMessage = "Kategoria jest wymagana.")]
        public string Category { get; set; }
        [ValidateNever]
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
        [ValidateNever]
        public List<string> ExistingImages { get; set; } = new List<string>();
        [ValidateNever]
        public List<string> ImagesToDelete { get; set; } = new List<string>();


    }
}
