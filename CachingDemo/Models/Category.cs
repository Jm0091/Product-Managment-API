using System.ComponentModel.DataAnnotations;

namespace CachingDemo.Models
{
    // Represents the Category entity in the database.
    public class Category
    {
        [Key] // Primary key
        public int CategoryId { get; set; }

        [Required] // Category name is required
        [MaxLength(50)] // Limit the category name to 50 characters
        public string CategoryName { get; set; }

        // Collection of related products (1-to-many relationship)
        public ICollection<Product> Products { get; set; }
    }
}
