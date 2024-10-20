using CachingDemo.Data;
using CachingDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace CachingDemo.Services
{
    public interface IProductService
    {
        IEnumerable<Product> GetAllProducts();
        Product GetProductById(int id);
        void AddProduct(Product product);
        void UpdateProduct(Product product);
        void DeleteProduct(int id);
    }

    /// <summary>
    /// Handles business logic for managing products.
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly ICachingService _cachingService;
        private readonly ILogger<ProductService> _logger;
        private readonly ApplicationDbContext _context; // EF Core DbContext

        public ProductService(ICachingService cachingService, ILogger<ProductService> logger, ApplicationDbContext context)
        {
            _cachingService = cachingService;
            _logger = logger;
            _context = context; // Dependency injection of the database context
        }

        /// <summary>
        /// Fetches all products, uses caching to optimize performance.
        /// </summary>
        public IEnumerable<Product> GetAllProducts()
        {
            const string cacheKey = "all_products";
            try
            {
                return _cachingService.GetOrAdd(cacheKey, FetchAllProductsFromDb, TimeSpan.FromMinutes(10));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching products: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Fetches a product by ID, uses caching for optimization.
        /// </summary>
        public Product GetProductById(int id)
        {
            string cacheKey = $"product_{id}";
            try
            {
                return _cachingService.GetOrAdd(cacheKey, () => FetchProductByIdFromDb(id), TimeSpan.FromMinutes(5));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching product with ID {id}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Adds a new product and invalidates the product cache.
        /// </summary>
        public void AddProduct(Product product)
        {
            try
            {
                _context.Products.Add(product);
                _context.SaveChanges(); // Persist changes to the database
                InvalidateProductCache(); // Clear cache since the data has changed
                _logger.LogInformation("Product added, cache invalidated.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding product: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Updates an existing product and invalidates the product cache.
        /// </summary>
        public void UpdateProduct(Product product)
        {
            try
            {
                _context.Products.Update(product);
                _context.SaveChanges(); // Update the product in the database
                InvalidateProductCache(); // Clear cache
                _logger.LogInformation($"Product with ID {product.Id} updated, cache invalidated.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating product: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Deletes a product by ID and invalidates the product cache.
        /// </summary>
        public void DeleteProduct(int id)
        {
            try
            {
                var product = _context.Products.Find(id);
                if (product == null) throw new KeyNotFoundException("Product not found.");

                _context.Products.Remove(product);
                _context.SaveChanges(); // Delete the product from the database
                InvalidateProductCache(); // Clear cache
                _logger.LogInformation($"Product with ID {id} deleted, cache invalidated.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting product: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Fetches all products directly from the database (used in caching logic).
        /// </summary>
        private IEnumerable<Product> FetchAllProductsFromDb()
        {
            return _context.Products.Include(p => p.Category).ToList(); // Query products with categories from the DB
        }

        /// <summary>
        /// Fetches a product by ID directly from the database.
        /// </summary>
        private Product FetchProductByIdFromDb(int id)
        {
            return _context.Products.Include(p => p.Category).FirstOrDefault(p => p.Id == id); // Query product by ID
        }

        /// <summary>
        /// Invalidates the product cache when data changes.
        /// </summary>
        private void InvalidateProductCache()
        {
            _cachingService.Remove("all_products"); // Remove all products from cache to refresh data
        }
    }

}
