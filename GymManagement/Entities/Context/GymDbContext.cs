using Microsoft.EntityFrameworkCore;
using System.Configuration;

namespace GymManagement.Entities.Context
{
    public class GymDbContext : DbContext
    {
        // DbSet properties
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<CheckIn> CheckIns { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }

        // Parameterless constructor for EF Tools (will use OnConfiguring) 
        public GymDbContext() { }

        // Constructor to receive options (Used by the Factory)
        public GymDbContext(DbContextOptions<GymDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // This block is used when the context is instantiated without options (e.g., in runtime).
                string connectionString = ConfigurationManager.ConnectionStrings["GymDbConnection"].ConnectionString;
                optionsBuilder
                    .UseSqlite(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // --- Explicitly define relationships to prevent potential Stack Overflow recursion ---

            // CheckIn relationship: One Customer has many CheckIns
            // Configure the relationship for CheckIn to Customer
            modelBuilder.Entity<CheckIn>()
                .HasOne(ci => ci.Customer)
                .WithMany() // Or specify a navigation property on the Customer side if one exists
                .HasForeignKey(ci => ci.CustomerId);

            // Configure the relationships for Purchase
            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.Customer)
                .WithMany()
                .HasForeignKey(p => p.CustomerId);

            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.Product)
                .WithMany()
                .HasForeignKey(p => p.ProductId);

            // Ensure Product Price is stored as TEXT in SQLite for precision (inherited from Models.cs, but safe to configure here)
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("TEXT");

            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Customer)
                .WithMany()
                .HasForeignKey(s => s.CustomerId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
