namespace projekt_.NET_schronisko.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public string UserId { get; set; } 
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>(); 
    }
    public class CartItem
    {
        public int Id { get; set; }
        public int CartId { get; set; } 
        public Cart Cart { get; set; }

        public int ProductId { get; set; } 
        public Product Product { get; set; }
        public decimal Price { get; set; }

        public int Quantity { get; set; } 
    }
}
