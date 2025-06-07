using System.ComponentModel.DataAnnotations;

namespace projekt_.NET_schronisko.Models
{
    public class AdoptionRequest
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Imię i nazwisko jest wymagane.")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email jest wymagany.")]
        [EmailAddress(ErrorMessage = "Nieprawidłowy adres e-mail.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Kod pocztowy jest wymagany.")]
        [RegularExpression(@"^\d{2}-\d{3}$", ErrorMessage = "Kod pocztowy musi być w formacie 00-000.")]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "Miasto jest wymagane.")]
        [StringLength(100)]
        public string City { get; set; }

        [Required(ErrorMessage = "Adres jest wymagany.")]
        [StringLength(200)]
        public string Address { get; set; }

        [Required(ErrorMessage = "Numer telefonu jest wymagany.")]
        [Phone(ErrorMessage = "Nieprawidłowy numer telefonu.")]
        public string PhoneNumber { get; set; }

        // Połączenie z modelem zwierzaka
        public int AnimalId { get; set; }
        public Animal Animal { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Proszę napisać kilka słów o sobie.")]
        [Display(Name = "Opis osoby")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Opis powinien mieć od 10 do 1000 znaków.")]
        public string AboutMe { get; set; }
    }
}
