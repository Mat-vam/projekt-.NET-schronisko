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
    }
}