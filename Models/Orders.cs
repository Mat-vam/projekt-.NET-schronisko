namespace projekt_.NET_schronisko.Models
{
    public class Orders
    {
        public int Id { get; set; }
        public DateTime DataZamowienia { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Nowe";

        public ICollection<OrderProduct> ProduktyZamowienia { get; set; }
        public string CustomerName { get; set; }
        public string DeliveryAddress { get; set; }
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
