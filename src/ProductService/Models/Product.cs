using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductService.Models;

[Table("Products")]
public class Product
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    [MaxLength(100)]
    public string Brand { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Sku { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation property for related order items
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

// DTO for creating a new product
public class CreateProductDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    [MaxLength(100)]
    public string Brand { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Sku { get; set; } = string.Empty;
}

// DTO for updating an existing product
public class UpdateProductDto
{
    [MaxLength(200)]
    public string? Name { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    public decimal? Price { get; set; }

    public int? StockQuantity { get; set; }

    [MaxLength(100)]
    public string? Brand { get; set; }

    [MaxLength(50)]
    public string? Sku { get; set; }

    public bool? IsActive { get; set; }
}

// DTO for product response
public class ProductResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// DTO for paginated product list
public class PaginatedProductResponse
{
    public List<ProductResponseDto> Products { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
