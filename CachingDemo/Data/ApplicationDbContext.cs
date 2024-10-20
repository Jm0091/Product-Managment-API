using CachingDemo.Models;
using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;


namespace CachingDemo.Data
{
    public class ApplicationDbContext : DbContext
    {


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) // Passes DbContextOptions to the base class constructor
        {
        }

        // These DbSet properties represent the tables in the database
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }


        // OnModelCreating method is used to configure model relationships and table properties
        // Fluent API for additional database configuration
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuring a 1-to-many relationship between Category and Products
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete

            // Seeding initial data (optional)
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, CategoryName = "Electronics" },
                new Category { CategoryId = 2, CategoryName = "Books" }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product { ProductId = 1, Name = "Laptop", Price = 1200.99M, CategoryId = 1 },
                new Product { ProductId = 2, Name = "Smartphone", Price = 800.50M, CategoryId = 1 },
                new Product { ProductId = 3, Name = "Programming Book", Price = 49.99M, CategoryId = 2 }
            );
        }


    }
}
