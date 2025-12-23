using IsisStore.Models;
using Microsoft.EntityFrameworkCore;

namespace IsisStore.Data
{
    public class AsosContext : DbContext
    {
        public AsosContext(DbContextOptions<AsosContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductSize> ProductSizes { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<User> Users { get; set; }

        // CHANGED: Linked to your existing Addresses table
        public DbSet<Address> Addresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().ToTable("Products");
            modelBuilder.Entity<ProductSize>().ToTable("ProductSizes");
            modelBuilder.Entity<CartItem>().ToTable("CartItems");
            modelBuilder.Entity<Order>().ToTable("Orders");
            modelBuilder.Entity<OrderItem>().ToTable("OrderItems");
            modelBuilder.Entity<User>().ToTable("Users");

            // EXACT MATCH: Tells C# to look for the table named "Addresses" in SQL
            modelBuilder.Entity<Address>().ToTable("Addresses");
        }
    }
}