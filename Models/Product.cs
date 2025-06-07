using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace projekt_.NET_schronisko.Models
{
    public class Product
    {
        public int ID { get; set; }
        [Required(ErrorMessage = "Nazwa produktu jest wymagana.")]
        [Display(Name = "Nazwa")]
        [StringLength(100)]
        public string Name { get; set; }
        [Required(ErrorMessage = "Cena produktu jest wymagana.")]
        [Display(Name = "Cena")]
        [Range(0.01, 10000, ErrorMessage = "Cena musi być większa niż 0.")]
        public decimal Price { get; set; }
        [Display(Name = "Kategoria")]
        [Required(ErrorMessage = "Kategoria jest wymagana.")]
        public string Category { get; set; }
        [ValidateNever]
        public ICollection<OrderProduct> ProduktyZamowienia { get; set; }
        public ICollection<ProductImage> Images { get; set; }
    }
}
