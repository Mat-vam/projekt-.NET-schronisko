using System.ComponentModel.DataAnnotations;

namespace projekt_.NET_schronisko.ViewModels
{
    public class OrderFormViewModel
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

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
