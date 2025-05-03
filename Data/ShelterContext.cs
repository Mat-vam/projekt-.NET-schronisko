using projekt_.NET_schronisko.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace projekt_.NET_schronisko.Data
{
    public class ShelterContext : IdentityDbContext<Users>
    {
        public ShelterContext(DbContextOptions options) : base(options) { }
        public DbSet<Product> Product { get; set; }
        public DbSet<Animal> Animal { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<OrderProduct> OrderProducts { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<AnimalImage> AnimalImages { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OrderProduct>(entity =>
            {
                entity.ToTable("OrderProduct");

                entity.HasKey(e => new { e.OrdersId, e.ProductId });

                entity.HasOne(e => e.Orders)
                    .WithMany(o => o.ProduktyZamowienia)
                    .HasForeignKey(e => e.OrdersId);

                entity.HasOne(e => e.Product)
                    .WithMany(p => p.ProduktyZamowienia)
                    .HasForeignKey(e => e.ProductId);
            });
        }
    }
}