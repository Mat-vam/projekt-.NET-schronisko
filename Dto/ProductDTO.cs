using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace projekt_.NET_schronisko.Dto
{
    public class ProductDto
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