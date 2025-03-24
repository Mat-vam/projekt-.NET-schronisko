using projekt_.NET_schronisko.Models;
using Microsoft.EntityFrameworkCore;


namespace projekt_.NET_schronisko.Data
{
    public class AnimalContext : DbContext
    {
        public AnimalContext(DbContextOptions options) : base(options) { }
        public DbSet<Animal> Animal { get; set; }
    }
}
