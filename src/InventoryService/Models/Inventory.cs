using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryService.Models;

[Table("InventoryItems")]
public class InventoryItem
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    public int AvailableQuantity { get; set; }

    public int ReservedQuantity { get; set; }

    public int ReorderLevel { get; set; }

    public int ReorderQuantity { get; set; }

    public DateTime? LastRestocked { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Computed property: Total quantity
    [NotMapped]
    public int TotalQuantity => AvailableQuantity + ReservedQuantity;

    // Computed property: Is low on stock
    [NotMapped]
    public bool IsLowStock => AvailableQuantity <= ReorderLevel;
}

// DTO for creating inventory item
public class CreateInventoryDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    public int AvailableQuantity { get; set; }

    public int ReorderLevel { get; set; }

    public int ReorderQuantity { get; set; }
}

// DTO for updating inventory
public class UpdateInventoryDto
{
    public int? AvailableQuantity { get; set; }

    public int? ReservedQuantity { get; set; }

    public int? ReorderLevel { get; set; }

    public int? ReorderQuantity { get; set; }
}

// DTO for inventory response
public class InventoryResponseDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int AvailableQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int TotalQuantity { get; set; }
    public int ReorderLevel { get; set; }
    public int ReorderQuantity { get; set; }
    public bool IsLowStock { get; set; }
    public DateTime? LastRestocked { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// DTO for stock deduction
public class DeductStockDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}

// DTO for reserve stock
public class ReserveStockDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}

// DTO for restock
public class RestockDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
