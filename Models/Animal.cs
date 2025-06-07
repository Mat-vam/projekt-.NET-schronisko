using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace projekt_.NET_schronisko.Models
{
    public class Animal
    {
        public int ID { get; set; }
        [Required(ErrorMessage = "Imię zwierzaka jest wymagane.")]
        [Display(Name = "Imię")]
        [StringLength(50)]
        public string Name { get; set; }
        [Required(ErrorMessage = "Gatunek jest wymagany.")]
        [Display(Name = "Gatunek")]
        [StringLength(50)]
        public string Species { get; set; }
        [Required(ErrorMessage = "Opis zwierzaka jest wymagany.")]
        [Display(Name = "Opis")]
        [StringLength(1000, ErrorMessage = "Opis nie może mieć więcej niż 1000 znaków.")]
        public string Bio { get; set; }
        [Required]
        [Display(Name = "Status")]
        [RegularExpression("^(Niezadoptowany|Adoptowany)$", ErrorMessage = "Status musi być 'Niezadoptowany' lub 'Adoptowany'.")]
        public string Status { get; set; }
        public ICollection<AnimalImage> Images { get; set; }
    }
}
