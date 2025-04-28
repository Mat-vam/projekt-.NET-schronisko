using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace projekt_.NET_schronisko.Models
{
    public class Product
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        [Required]
        public string Category { get; set; }
        [ValidateNever]
        public ICollection<OrderProduct> ProduktyZamowienia { get; set; }
        public ICollection<ProductImage> Images { get; set; }
    }
}
