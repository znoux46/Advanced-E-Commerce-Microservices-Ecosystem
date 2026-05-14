using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;
using ProductService.Services;

namespace ProductService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly ProductDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        ProductDbContext context, 
        ICacheService cacheService,
        ILogger<ProductsController> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _logger = logger;
    }

    // GET: api/products
    // Get all products with pagination and caching
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PaginatedProductResponse>> GetProducts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? category = null,
        [FromQuery] string? search = null)
    {
        try
        {
            // Generate cache key based on query parameters
            var cacheKey = $"products:page:{pageNumber}:size:{pageSize}:category:{category}:search:{search}";
            
            // Try to get from cache first
            var cachedResult = await _cacheService.GetAsync<PaginatedProductResponse>(cacheKey);
            if (cachedResult != null)
            {
                _logger.LogInformation("Retrieved products from cache");
                return Ok(cachedResult);
            }

            // Build query
            var query = _context.Products.Where(p => p.IsActive).AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category == category);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => 
                    p.Name.Contains(search) || 
                    p.Description.Contains(search) ||
                    p.Brand.Contains(search));
            }

            // Get total count
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Apply pagination
            var products = await query
                .OrderBy(p => p.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Category = p.Category,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    Brand = p.Brand,
                    Sku = p.Sku,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .ToListAsync();

            var response = new PaginatedProductResponse
            {
                Products = products,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            // Cache result for 5 minutes
            await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            return StatusCode(500, new { message = "An error occurred while retrieving products" });
        }
    }

    // GET: api/products/{id}
    // Get a specific product by ID
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponseDto>> GetProduct(Guid id)
    {
        try
        {
            var cacheKey = $"product:{id}";
            
            // Try to get from cache first
            var cachedProduct = await _cacheService.GetAsync<ProductResponseDto>(cacheKey);
            if (cachedProduct != null)
            {
                return Ok(cachedProduct);
            }

            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            var response = new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Category = product.Category,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                Brand = product.Brand,
                Sku = product.Sku,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };

            // Cache for 10 minutes
            await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the product" });
        }
    }

    // POST: api/products
    // Create a new product
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductResponseDto>> CreateProduct([FromBody] CreateProductDto request)
    {
        try
        {
            // Validate model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Category = request.Category,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                Brand = request.Brand,
                Sku = request.Sku,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var response = new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Category = product.Category,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                Brand = product.Brand,
                Sku = product.Sku,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };

            // Invalidate product list cache
            await _cacheService.RemoveAsync("products:*");

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, new { message = "An error occurred while creating the product" });
        }
    }

    // PUT: api/products/{id}
    // Update an existing product
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponseDto>> UpdateProduct(Guid id, [FromBody] UpdateProductDto request)
    {
        try
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            // Apply updates if provided
            if (!string.IsNullOrEmpty(request.Name))
                product.Name = request.Name;
            
            if (!string.IsNullOrEmpty(request.Description))
                product.Description = request.Description;
            
            if (!string.IsNullOrEmpty(request.Category))
                product.Category = request.Category;
            
            if (request.Price.HasValue)
                product.Price = request.Price.Value;
            
            if (request.StockQuantity.HasValue)
                product.StockQuantity = request.StockQuantity.Value;
            
            if (!string.IsNullOrEmpty(request.Brand))
                product.Brand = request.Brand;
            
            if (!string.IsNullOrEmpty(request.Sku))
                product.Sku = request.Sku;
            
            if (request.IsActive.HasValue)
                product.IsActive = request.IsActive.Value;

            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var response = new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Category = product.Category,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                Brand = product.Brand,
                Sku = product.Sku,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };

            // Invalidate caches
            await _cacheService.RemoveAsync($"product:{id}");
            await _cacheService.RemoveAsync("products:*");

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the product" });
        }
    }

    // DELETE: api/products/{id}
    // Soft delete a product
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        try
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            // Soft delete - just mark as inactive
            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Invalidate caches
            await _cacheService.RemoveAsync($"product:{id}");
            await _cacheService.RemoveAsync("products:*");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the product" });
        }
    }

    // GET: api/products/categories
    // Get all product categories
    [HttpGet("categories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<string>>> GetCategories()
    {
        try
        {
            var categories = await _context.Products
                .Where(p => p.IsActive)
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories");
            return StatusCode(500, new { message = "An error occurred while retrieving categories" });
        }
    }

    // GET: api/products/brands
    // Get all product brands
    [HttpGet("brands")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<string>>> GetBrands()
    {
        try
        {
            var brands = await _context.Products
                .Where(p => p.IsActive)
                .Select(p => p.Brand)
                .Distinct()
                .OrderBy(b => b)
                .ToListAsync();

            return Ok(brands);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving brands");
            return StatusCode(500, new { message = "An error occurred while retrieving brands" });
        }
    }
}
