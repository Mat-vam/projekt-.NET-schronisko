using System.ComponentModel.DataAnnotations;

namespace projekt_.NET_schronisko.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Email jest wymagany")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Hasło jest wymagane")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "{0} musi mieć conajmniej {2} i conajwyżej {1} znaków")]
        [DataType(DataType.Password)]
        [Display(Name = "Nowe hasło")]
        [Compare("ConfirmNewPassword", ErrorMessage = "Hasła się nie zgadzają")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Potwierdzenie hasła jest wymagane")]
        [DataType(DataType.Password)]
        [Display(Name = "Potwierdź nowe hasło")]
        public string ConfirmNewPassword { get; set; }
    }
}
