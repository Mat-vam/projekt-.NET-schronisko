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

        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
    }
}
