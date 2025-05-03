using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace projekt_.NET_schronisko.Models
{
    public class AnimalImage
    {
        public int Id { get; set; }

        [Required]
        public string ImageUrl { get; set; }

        [ForeignKey("Animal")]
        public int AnimalId { get; set; }
        public Animal Animal { get; set; }
    }
}
