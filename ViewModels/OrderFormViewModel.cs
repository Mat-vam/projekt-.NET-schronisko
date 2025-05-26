using System.ComponentModel.DataAnnotations;

namespace projekt_.NET_schronisko.ViewModels
{
    public class OrderFormViewModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }

        [Required]
        public string PostalCode { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string PhoneNumber { get; set; }
    }
}
