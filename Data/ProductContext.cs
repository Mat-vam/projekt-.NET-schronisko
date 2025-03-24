using Microsoft.EntityFrameworkCore;
using projekt_.NET_schronisko.Models;


namespace projekt_.NET_schronisko.Data
{
    public class ProductContext:DbContext
    {
        public ProductContext(DbContextOptions options) : base(options) { }
        public DbSet<Product> Product { get; set; }
    }
}
