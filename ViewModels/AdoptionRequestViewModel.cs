using System.ComponentModel.DataAnnotations;

namespace projekt_.NET_schronisko.ViewModels
{
    public class AdoptionRequestViewModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public int AnimalId { get; set; }
        [Required(ErrorMessage = "Kod pocztowy jest wymagany")]
        [RegularExpression(@"^\d{2}-\d{3}$", ErrorMessage = "Niepoprawny format kodu pocztowego (np. 00-123)")]

        public string PostalCode { get; set; }

        [Required(ErrorMessage = "Miasto jest wymagane")]
        [MinLength(2, ErrorMessage = "Nazwa miasta jest za krótka")]

        public string City { get; set; }
        [Required(ErrorMessage = "Adres jest wymagany")]
        [MinLength(5, ErrorMessage = "Adres musi zawierać co najmniej 5 znaków")]
        public string Address { get; set; }
        [Required(ErrorMessage = "Numer telefonu jest wymagany")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "Numer telefonu musi zawierać dokładnie 9 cyfr")]


        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "To pole jest wymagane")]
        [MinLength(30, ErrorMessage = "Opowiedz więcej o sobie (minimum 30 znaków)")]

        public string AboutMe { get; set; }
    }
}
