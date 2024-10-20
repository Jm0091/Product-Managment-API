using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CachingDemo.Models
{
    // Represents the Product entity in the database.
    public class Product
    {
        [Key] // Primary key attribute
        public int ProductId { get; set; }

        [Required] // Field cannot be null
        [MaxLength(100)] // Limit the product name to 100 characters
        public string Name { get; set; }

        [Range(0.01, 10000.00)] // Price must be within the specified range
        public decimal Price { get; set; }

        [ForeignKey("Category")] // Specifies the foreign key relationship
        public int CategoryId { get; set; }

        // Navigation property for the relationship with the Category entity
        public Category Category { get; set; }

        // Additional field that might be useful for caching (e.g., product description)
        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
