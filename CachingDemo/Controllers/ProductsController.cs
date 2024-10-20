using CachingDemo.Data;
using CachingDemo.DTOs;
using CachingDemo.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IValidator<ProductDTO> _validator;
    private readonly ILogger<ProductsController> _logger;
    private readonly IMemoryCache _cache;

    //Cache keys stored to get/set/clear.
    private const string ProductCacheKey = "Product_{0}";
    private const string ProductsPagedCacheKey = "ProductsPaged_{0}_{1}";

    public ProductsController(ApplicationDbContext context, IValidator<ProductDTO> validator, ILogger<ProductsController> logger, IMemoryCache cache)
    {
        _context = context;
        _validator = validator;
        _logger = logger;
        _cache = cache;
    }

    // GET api/product?pageNumber=1&pageSize=10
    [HttpGet]
    public async Task<IActionResult> GetPagedProducts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {

        try
        {

            var cacheKey = string.Format(ProductsPagedCacheKey, pageNumber, pageSize);
            _logger.LogInformation("Checking cache for key: {CacheKey}", cacheKey);

            if (!_cache.TryGetValue(cacheKey, out PagedResult<ProductDTO> pagedResult))
            {
                _logger.LogInformation("Cache miss for key: {CacheKey}", cacheKey);
                _logger.LogInformation("Fetching products with pagination: PageNumber = {PageNumber}, PageSize = {PageSize}", pageNumber, pageSize);

                var totalItems = await _context.Products.CountAsync();
                var products = await _context.Products
                    .Include(p => p.Category)
                    .Select(p => new ProductDTO
                    {
                        ProductId = p.ProductId,
                        Name = p.Name,
                        Price = p.Price,
                        CategoryName = p.Category.CategoryName
                    })
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.LogInformation("Successfully fetched {Count} products for PageNumber = {PageNumber}", products.Count, pageNumber);

                pagedResult = new PagedResult<ProductDTO>
                {
                    Items = products,
                    TotalItems = totalItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
                _logger.LogInformation("Returning paged result of {Count} items.", products.Count);


                // Set cache options and cache the result
                _cache.Set(cacheKey, pagedResult, TimeSpan.FromMinutes(5));
            }
            else
            {
                _logger.LogInformation("Cache hit for key: {CacheKey}", cacheKey);
            }
            return Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching paginated products");
            return StatusCode(500, "Internal server error");
        }
    }

    // GET api/product/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        try
        {
            var cacheKey = string.Format(ProductCacheKey, id);
            _logger.LogInformation("Checking cache for key: {CacheKey}", cacheKey);

            if (!_cache.TryGetValue(cacheKey, out ProductDTO productDto))
            {

                _logger.LogInformation("Fetching product by ID = {Id}", id);

                var product = await _context.Products.Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.ProductId == id);

                if (product == null)
                {
                    _logger.LogWarning("Product with ID = {Id} not found", id);
                    return NotFound("Product not found");
                }

                _logger.LogInformation("Product with ID = {Id} found", id);

                productDto = new ProductDTO
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Price = product.Price,
                    CategoryName = product.Category.CategoryName
                };

                _logger.LogInformation("Returning product with ID = {Id}", id);

                // Cache the product details for 10 minutes
                _cache.Set(cacheKey, productDto, TimeSpan.FromMinutes(10));
            }
            else
            {
                _logger.LogInformation("Returning cached product with ID = {Id}", id);
            }
            return Ok(productDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching product by ID");
            return StatusCode(500, "Internal server error");
        }
    }

    // POST api/product
    [HttpPost]
    public async Task<IActionResult> AddProduct([FromBody] ProductDTO productDto)
    {
        _logger.LogInformation("Validating product input data");

        var validationResult = await _validator.ValidateAsync(productDto);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validation failed for product: {Errors}", validationResult.Errors);
            return BadRequest(validationResult.Errors);
        }

        try
        {
            _logger.LogInformation("Checking category existence for CategoryName = {CategoryName}", productDto.CategoryName);

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryName == productDto.CategoryName);
            if (category == null)
            {
                _logger.LogWarning("Category '{CategoryName}' not found", productDto.CategoryName);
                return BadRequest($"Category '{productDto.CategoryName}' does not exist.");
            }

            _logger.LogInformation("Creating a new product");

            var product = new Product
            {
                Name = productDto.Name,
                Price = productDto.Price,
                CategoryId = category.CategoryId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Clear product cache
            _logger.LogInformation("Removing Cache");
            _cache.Remove(string.Format(ProductsPagedCacheKey, 1, 10));

            var createdProductDto = new ProductDTO
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Price = product.Price,
                CategoryName = category.CategoryName
            };

            _logger.LogInformation("Product created successfully with ID = {ProductId}", product.ProductId);
            return CreatedAtAction(nameof(GetProduct), new { id = product.ProductId }, createdProductDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product");
            return StatusCode(500, "Internal server error");
        }
    }

    // PUT api/product/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductDTO productDto)
    {
        if (id != productDto.ProductId)
        {
            _logger.LogWarning("Product ID mismatch: Request ID = {Id}, DTO Product ID = {ProductId}", id, productDto.ProductId);
            return BadRequest("Product ID mismatch");
        }

        _logger.LogInformation("Validating product input data for update");

        var validationResult = await _validator.ValidateAsync(productDto);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validation failed for product: {Errors}", validationResult.Errors);
            return BadRequest(validationResult.Errors);
        }

        try
        {
            _logger.LogInformation("Fetching product with ID = {Id} for update", id);

            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
            {
                _logger.LogWarning("Product with ID = {Id} not found", id);
                return NotFound("Product not found");
            }

            _logger.LogInformation("Checking category existence for CategoryName = {CategoryName}", productDto.CategoryName);

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryName == productDto.CategoryName);
            if (category == null)
            {
                _logger.LogWarning("Category '{CategoryName}' not found", productDto.CategoryName);
                return BadRequest($"Category '{productDto.CategoryName}' does not exist.");
            }

            _logger.LogInformation("Updating product with ID = {Id}", id);

            existingProduct.Name = productDto.Name;
            existingProduct.Price = productDto.Price;
            existingProduct.CategoryId = category.CategoryId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Product with ID = {Id} updated successfully", id);

            // Update the cache for this product and clear paginated product cache
            var cacheKey = string.Format(ProductCacheKey, id);
            _cache.Set(cacheKey, productDto, TimeSpan.FromMinutes(10));
            _cache.Remove(string.Format(ProductsPagedCacheKey, 1, 10));
            _logger.LogInformation("Refreshing Cache with new data.");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product");
            return StatusCode(500, "Internal server error");
        }
    }

    // DELETE api/product/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            _logger.LogInformation("Fetching product with ID = {Id} for deletion", id);

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product with ID = {Id} not found", id);
                return NotFound("Product not found");
            }

            _logger.LogInformation("Deleting product with ID = {Id}", id);

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            // Remove from cache insuring next time the system fetch the latest data reflecting any changes
            var cacheKey = string.Format(ProductCacheKey, id);
            _cache.Remove(cacheKey);
            _cache.Remove(string.Format(ProductsPagedCacheKey, 1, 10));
            _logger.LogInformation("Removing Cache.");


            _logger.LogInformation("Delete with ID = {Id} successfull!", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product");
            return StatusCode(500, "Internal server error");
        }
    }

}




/*
 //   Products Controller: Code without Cache mechanism
 //  *******************************************************************




using CachingDemo.Data;
using CachingDemo.DTOs;
using CachingDemo.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IValidator<ProductDTO> _validator;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(ApplicationDbContext context, IValidator<ProductDTO> validator, ILogger<ProductsController> logger)
    {
        _context = context;
        _validator = validator;
        _logger = logger;
    }

    // GET api/product?pageNumber=1&pageSize=10
    [HttpGet]
    public async Task<IActionResult> GetPagedProducts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Fetching products with pagination: PageNumber = {PageNumber}, PageSize = {PageSize}", pageNumber, pageSize);

            var totalItems = await _context.Products.CountAsync();
            var products = await _context.Products
                .Include(p => p.Category)
                .Select(p => new ProductDTO
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Price = p.Price,
                    CategoryName = p.Category.CategoryName
                })
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation("Successfully fetched {Count} products for PageNumber = {PageNumber}", products.Count, pageNumber);

            var pagedResult = new PagedResult<ProductDTO>
            {
                Items = products,
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            _logger.LogInformation("Returning paged result of {Count} items.", products.Count);
            return Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching paginated products");
            return StatusCode(500, "Internal server error");
        }
    }

    // GET api/product/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        try
        {
            _logger.LogInformation("Fetching product by ID = {Id}", id);

            var product = await _context.Products.Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                _logger.LogWarning("Product with ID = {Id} not found", id);
                return NotFound("Product not found");
            }

            _logger.LogInformation("Product with ID = {Id} found", id);

            var productDto = new ProductDTO
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Price = product.Price,
                CategoryName = product.Category.CategoryName
            };

            _logger.LogInformation("Returning product with ID = {Id}", id);
            return Ok(productDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching product by ID");
            return StatusCode(500, "Internal server error");
        }
    }

    // POST api/product
    [HttpPost]
    public async Task<IActionResult> AddProduct([FromBody] ProductDTO productDto)
    {
        _logger.LogInformation("Validating product input data");

        var validationResult = await _validator.ValidateAsync(productDto);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validation failed for product: {Errors}", validationResult.Errors);
            return BadRequest(validationResult.Errors);
        }

        try
        {
            _logger.LogInformation("Checking category existence for CategoryName = {CategoryName}", productDto.CategoryName);

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryName == productDto.CategoryName);
            if (category == null)
            {
                _logger.LogWarning("Category '{CategoryName}' not found", productDto.CategoryName);
                return BadRequest($"Category '{productDto.CategoryName}' does not exist.");
            }

            _logger.LogInformation("Creating a new product");

            var product = new Product
            {
                Name = productDto.Name,
                Price = productDto.Price,
                CategoryId = category.CategoryId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var createdProductDto = new ProductDTO
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Price = product.Price,
                CategoryName = category.CategoryName
            };

            _logger.LogInformation("Product created successfully with ID = {ProductId}", product.ProductId);
            return CreatedAtAction(nameof(GetProduct), new { id = product.ProductId }, createdProductDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product");
            return StatusCode(500, "Internal server error");
        }
    }

    // PUT api/product/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductDTO productDto)
    {
        if (id != productDto.ProductId)
        {
            _logger.LogWarning("Product ID mismatch: Request ID = {Id}, DTO Product ID = {ProductId}", id, productDto.ProductId);
            return BadRequest("Product ID mismatch");
        }

        _logger.LogInformation("Validating product input data for update");

        var validationResult = await _validator.ValidateAsync(productDto);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validation failed for product: {Errors}", validationResult.Errors);
            return BadRequest(validationResult.Errors);
        }

        try
        {
            _logger.LogInformation("Fetching product with ID = {Id} for update", id);

            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
            {
                _logger.LogWarning("Product with ID = {Id} not found", id);
                return NotFound("Product not found");
            }

            _logger.LogInformation("Checking category existence for CategoryName = {CategoryName}", productDto.CategoryName);

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryName == productDto.CategoryName);
            if (category == null)
            {
                _logger.LogWarning("Category '{CategoryName}' not found", productDto.CategoryName);
                return BadRequest($"Category '{productDto.CategoryName}' does not exist.");
            }

            _logger.LogInformation("Updating product with ID = {Id}", id);

            existingProduct.Name = productDto.Name;
            existingProduct.Price = productDto.Price;
            existingProduct.CategoryId = category.CategoryId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Product with ID = {Id} updated successfully", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product");
            return StatusCode(500, "Internal server error");
        }
    }

    // DELETE api/product/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            _logger.LogInformation("Fetching product with ID = {Id} for deletion", id);

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product with ID = {Id} not found", id);
                return NotFound("Product not found");
            }

            _logger.LogInformation("Deleting product with ID = {Id}", id);

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();


            _logger.LogInformation("Delete with ID = {Id} successfull!", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product");
            return StatusCode(500, "Internal server error");
        }
    }

}

 */