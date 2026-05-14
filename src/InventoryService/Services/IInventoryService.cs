using InventoryService.Models;

namespace InventoryService.Services;

public interface IInventoryService
{
    // Get inventory by product ID
    Task<InventoryItem?> GetByProductIdAsync(Guid productId);
    
    // Get all inventories with pagination
    Task<List<InventoryItem>> GetAllAsync(int page = 1, int pageSize = 10);
    
    // Create inventory item
    Task<InventoryItem> CreateAsync(CreateInventoryDto dto);
    
    // Update inventory
    Task<InventoryItem> UpdateAsync(Guid productId, UpdateInventoryDto dto);
    
    // Deduct stock (when order is placed)
    Task<bool> DeductStockAsync(Guid productId, int quantity);
    
    // Reserve stock (temporary hold)
    Task<bool> ReserveStockAsync(Guid productId, int quantity);
    
    // Release reserved stock
    Task<bool> ReleaseStockAsync(Guid productId, int quantity);
    
    // Commit reserved stock (finalize order)
    Task<bool> CommitStockAsync(Guid productId, int quantity);
    
    // Restock inventory
    Task<InventoryItem> RestockAsync(Guid productId, int quantity);
    
    // Get low stock items
    Task<List<InventoryItem>> GetLowStockItemsAsync();
    
    // Check availability
    Task<bool> CheckAvailabilityAsync(Guid productId, int quantity);
}

// Inventory Service Implementation
public class InventoryServiceImpl : IInventoryService
{
    private readonly InventoryDbContext _context;
    private readonly ILogger<InventoryServiceImpl> _logger;

    public InventoryServiceImpl(InventoryDbContext context, ILogger<InventoryServiceImpl> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<InventoryItem?> GetByProductIdAsync(Guid productId)
    {
        return await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.ProductId == productId);
    }

    public async Task<List<InventoryItem>> GetAllAsync(int page = 1, int pageSize = 10)
    {
        return await _context.InventoryItems
            .OrderBy(i => i.ProductName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<InventoryItem> CreateAsync(CreateInventoryDto dto)
    {
        var item = new InventoryItem
        {
            Id = Guid.NewGuid(),
            ProductId = dto.ProductId,
            ProductName = dto.ProductName,
            AvailableQuantity = dto.AvailableQuantity,
            ReservedQuantity = 0,
            ReorderLevel = dto.ReorderLevel,
            ReorderQuantity = dto.ReorderQuantity,
            LastRestocked = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.InventoryItems.Add(item);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created inventory for product {ProductId}", dto.ProductId);
        return item;
    }

    public async Task<InventoryItem> UpdateAsync(Guid productId, UpdateInventoryDto dto)
    {
        var item = await GetByProductIdAsync(productId);
        if (item == null)
        {
            throw new KeyNotFoundException($"Inventory not found for product {productId}");
        }

        if (dto.AvailableQuantity.HasValue)
            item.AvailableQuantity = dto.AvailableQuantity.Value;
        
        if (dto.ReservedQuantity.HasValue)
            item.ReservedQuantity = dto.ReservedQuantity.Value;
        
        if (dto.ReorderLevel.HasValue)
            item.ReorderLevel = dto.ReorderLevel.Value;
        
        if (dto.ReorderQuantity.HasValue)
            item.ReorderQuantity = dto.ReorderQuantity.Value;

        item.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Updated inventory for product {ProductId}", productId);
        return item;
    }

    public async Task<bool> DeductStockAsync(Guid productId, int quantity)
    {
        var item = await GetByProductIdAsync(productId);
        if (item == null)
        {
            _logger.LogWarning("Inventory not found for product {ProductId}", productId);
            return false;
        }

        if (item.AvailableQuantity < quantity)
        {
            _logger.LogWarning("Insufficient stock for product {ProductId}. Available: {Available}, Requested: {Requested}",
                productId, item.AvailableQuantity, quantity);
            return false;
        }

        item.AvailableQuantity -= quantity;
        item.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Deducted {Quantity} from product {ProductId}. New available: {NewAvailable}",
            quantity, productId, item.AvailableQuantity);
        return true;
    }

    public async Task<bool> ReserveStockAsync(Guid productId, int quantity)
    {
        var item = await GetByProductIdAsync(productId);
        if (item == null)
        {
            return false;
        }

        if (item.AvailableQuantity < quantity)
        {
            return false;
        }

        item.AvailableQuantity -= quantity;
        item.ReservedQuantity += quantity;
        item.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Reserved {Quantity} for product {ProductId}", quantity, productId);
        return true;
    }

    public async Task<bool> ReleaseStockAsync(Guid productId, int quantity)
    {
        var item = await GetByProductIdAsync(productId);
        if (item == null)
        {
            return false;
        }

        if (item.ReservedQuantity < quantity)
        {
            quantity = item.ReservedQuantity;
        }

        item.ReservedQuantity -= quantity;
        item.AvailableQuantity += quantity;
        item.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Released {Quantity} for product {ProductId}", quantity, productId);
        return true;
    }

    public async Task<bool> CommitStockAsync(Guid productId, int quantity)
    {
        var item = await GetByProductIdAsync(productId);
        if (item == null)
        {
            return false;
        }

        // Remove from reserved, don't add back to available
        if (item.ReservedQuantity >= quantity)
        {
            item.ReservedQuantity -= quantity;
        }
        else
        {
            item.ReservedQuantity = 0;
        }

        item.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Committed {Quantity} for product {ProductId}", quantity, productId);
        return true;
    }

    public async Task<InventoryItem> RestockAsync(Guid productId, int quantity)
    {
        var item = await GetByProductIdAsync(productId);
        if (item == null)
        {
            throw new KeyNotFoundException($"Inventory not found for product {productId}");
        }

        item.AvailableQuantity += quantity;
        item.LastRestocked = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Restocked {Quantity} for product {ProductId}. New available: {NewAvailable}",
            quantity, productId, item.AvailableQuantity);
        return item;
    }

    public async Task<List<InventoryItem>> GetLowStockItemsAsync()
    {
        return await _context.InventoryItems
            .Where(i => i.AvailableQuantity <= i.ReorderLevel)
            .OrderBy(i => i.AvailableQuantity)
            .ToListAsync();
    }

    public async Task<bool> CheckAvailabilityAsync(Guid productId, int quantity)
    {
        var item = await GetByProductIdAsync(productId);
        return item != null && item.AvailableQuantity >= quantity;
    }
}
