using System.ComponentModel.DataAnnotations;

namespace projekt_.NET_schronisko.Models
{
    public class Orders
    {
        public int Id { get; set; }
        public DateTime DataZamowienia { get; set; } = DateTime.UtcNow;
        [Required]
        public string Status { get; set; } = "Nowe";

        public ICollection<OrderProduct> ProduktyZamowienia { get; set; }
        [Required(ErrorMessage = "Imię i nazwisko klienta są wymagane.")]
        public string CustomerName { get; set; }
        [Required(ErrorMessage = "Adres dostawy jest wymagany.")]
        public string DeliveryAddress { get; set; }
        [Phone(ErrorMessage = "Nieprawidłowy numer telefonu.")]
        public string? PhoneNumber { get; set; }
        public decimal TotalAmount
        {
            get
            {
                return ProduktyZamowienia?.Sum(op => op.Ilosc * op.Product.Price) ?? 0;
            }
        }
    }

    public class OrderProduct
    {
        public int OrdersId { get; set; }
        public Orders Orders { get; set; }

        public int ProductId { get; set; } 
        public Product Product { get; set; }

        public int Ilosc { get; set; }
    }
}
