using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using projekt_.NET_schronisko.Models;
using System.ComponentModel.DataAnnotations;

namespace projekt_.NET_schronisko.Repository
{
    public class AnimalRepo
    {
        public int ID { get; set; }
        [Display(Name = "Imię")]
        [Required(ErrorMessage = "Imię zwierzaka jest wymagane.")]
        [StringLength(50)]
        public string Name { get; set; }
        [Display(Name = "Gatunek")]
        [Required(ErrorMessage = "Gatunek jest wymagany.")]
        [StringLength(50)]
        public string Species { get; set; }
        [Display(Name = "Opis")]
        [Required(ErrorMessage = "Opis zwierzaka jest wymagany.")]
        [StringLength(1000, ErrorMessage = "Opis nie może mieć więcej niż 1000 znaków.")]
        public string Bio { get; set; }
        [Required]
        [Display(Name = "Status")]
        [RegularExpression("^(Niezadoptowany|Adoptowany)$", ErrorMessage = "Status musi być 'Niezadoptowany' lub 'Adoptowany'.")]
        public string Status { get; set; }
        [ValidateNever]
        public ICollection<IFormFile> Images { get; set; } = new List<IFormFile>();
        [ValidateNever]
        public List<string> ExistingImages { get; set; } = new List<string>();
        [ValidateNever]
        public List<string> ImagesToDelete { get; set; } = new List<string>();
    }
}
