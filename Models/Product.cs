using System.ComponentModel.DataAnnotations;

namespace projekt_.NET_schronisko.Models
{
    public class Product
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        [Required]
        public string Category { get; set; }
    }
}
