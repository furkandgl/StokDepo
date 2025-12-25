using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StokDepo.Models;

namespace StokDepo.Data
{
    // Identity tabloları + kendi tablolarımız (Products) aynı DB'de
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = default!;
        public DbSet<StockMovement> StockMovements { get; set; } = default!;

    }
}
