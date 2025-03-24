using projekt_.NET_schronisko.Models;
using Microsoft.EntityFrameworkCore;


namespace projekt_.NET_schronisko.Data
{
    public class ProductContext : DbContext
    {
        public ProductContext(DbContextOptions options) : base(options) { }
        public DbSet<Product> Product { get; set; }
    }
}