using projekt_.NET_schronisko.Models;
using Microsoft.EntityFrameworkCore;


namespace projekt_.NET_schronisko.Data
{
    public class ShelterContext : DbContext
    {
        public ShelterContext(DbContextOptions options) : base(options) { }
        public DbSet<Product> Product { get; set; }
        public DbSet<Animal> Animal { get; set; }
    }
}