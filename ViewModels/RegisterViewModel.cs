using System.ComponentModel.DataAnnotations;

namespace projekt_.NET_schronisko.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Imie jest wymagane")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email jest wymagany")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Hasło jest wymagane")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "{0} musi mieć conajmniej {2} i conajwyżej {1} znaków")]
        [DataType(DataType.Password)]
        [Compare("ConfirmPassword", ErrorMessage = "Hasła się nie zgadzają")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Potwierdzenie hasła jest wymagane")]
        [DataType(DataType.Password)]
        [Display(Name = "Potwierdź nowe hasło")]
        public string ConfirmPassword { get; set; }
    }
}
