using System.ComponentModel.DataAnnotations;

namespace projekt_.NET_schronisko.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email jest wymagany")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Hasło jest wymagane")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Zapamietaj mnie")]
        public bool RememberMe { get; set; }    

    }
}
