using System.ComponentModel.DataAnnotations;

namespace projekt_.NET_schronisko.ViewModels
{
    public class VerifyEmailViewModel
    {
        [Required(ErrorMessage = "Email jest wymagany")]
        [EmailAddress]
        public string Email { get; set; }

    }
}
