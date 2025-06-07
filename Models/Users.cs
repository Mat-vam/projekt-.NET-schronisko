using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace projekt_.NET_schronisko.Models
{
    public class Users : IdentityUser
    {
        [Required(ErrorMessage = "Imię i nazwisko są wymagane.")]
        [StringLength(100)]
        public string FullName { get; set; }
    }
}
