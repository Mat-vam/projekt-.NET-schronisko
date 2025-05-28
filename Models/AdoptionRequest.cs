using System.ComponentModel.DataAnnotations;

namespace projekt_.NET_schronisko.Models
{
    public class AdoptionRequest
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string PostalCode { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        // Połączenie z modelem zwierzaka
        public int AnimalId { get; set; }
        public Animal Animal { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Proszę napisać kilka słów o sobie.")]
        [Display(Name = "Opis osoby")]
        [MaxLength(1000)]
        public string AboutMe { get; set; }
    }
}
