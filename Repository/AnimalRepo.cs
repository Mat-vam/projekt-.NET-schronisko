using projekt_.NET_schronisko.Models;
using System.ComponentModel.DataAnnotations;

namespace projekt_.NET_schronisko.Repository
{
    public class AnimalRepo
    {
        public int ID { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Species { get; set; }
        [Required]
        public string Bio { get; set; }
        [Required]
        public string Status { get; set; }
        public ICollection<IFormFile> Images { get; set; } = new List<IFormFile>();
    }
}
