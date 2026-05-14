using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventoryService.Models;
using InventoryService.Services;

namespace InventoryService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(IInventoryService inventoryService, ILogger<InventoryController> logger)
    {
        _inventoryService = inventoryService;
        _logger = logger;
    }

    // GET: api/inventory
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<InventoryResponseDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var items = await _inventoryService.GetAllAsync(page, pageSize);
        var response = items.Select(MapToResponse).ToList();
        return Ok(response);
    }

    // GET: api/inventory/{productId}
    [HttpGet("{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InventoryResponseDto>> GetByProductId(Guid productId)
    {
        var item = await _inventoryService.GetByProductIdAsync(productId);
        if (item == null)
        {
            return NotFound(new { message = "Inventory not found" });
        }
        return Ok(MapToResponse(item));
    }

    // GET: api/inventory/low-stock
    [HttpGet("low-stock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<InventoryResponseDto>>> GetLowStock()
    {
        var items = await _inventoryService.GetLowStockItemsAsync();
        var response = items.Select(MapToResponse).ToList();
        return Ok(response);
    }

    // POST: api/inventory
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InventoryResponseDto>> Create([FromBody] CreateInventoryDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var item = await _inventoryService.CreateAsync(request);
        return CreatedAtAction(nameof(GetByProductId), new { productId = item.ProductId }, MapToResponse(item));
    }

    // PUT: api/inventory/{productId}
    [HttpPut("{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InventoryResponseDto>> Update(Guid productId, [FromBody] UpdateInventoryDto request)
    {
        try
        {
            var item = await _inventoryService.UpdateAsync(productId, request);
            return Ok(MapToResponse(item));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Inventory not found" });
        }
    }

    // POST: api/inventory/deduct
    [HttpPost("deduct")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> DeductStock([FromBody] DeductStockDto request)
    {
        var success = await _inventoryService.DeductStockAsync(request.ProductId, request.Quantity);
        if (!success)
        {
            return BadRequest(new { message = "Failed to deduct stock. Insufficient inventory or item not found." });
        }
        return Ok(new { message = "Stock deducted successfully" });
    }

    // POST: api/inventory/restock
    [HttpPost("restock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InventoryResponseDto>> Restock([FromBody] RestockDto request)
    {
        try
        {
            var item = await _inventoryService.RestockAsync(request.ProductId, request.Quantity);
            return Ok(MapToResponse(item));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Inventory not found" });
        }
    }

    // POST: api/inventory/check
    [HttpPost("check")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> CheckAvailability([FromBody] DeductStockDto request)
    {
        var available = await _inventoryService.CheckAvailabilityAsync(request.ProductId, request.Quantity);
        return Ok(new { available });
    }

    private static InventoryResponseDto MapToResponse(InventoryItem item)
    {
        return new InventoryResponseDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            AvailableQuantity = item.AvailableQuantity,
            ReservedQuantity = item.ReservedQuantity,
            TotalQuantity = item.TotalQuantity,
            ReorderLevel = item.ReorderLevel,
            ReorderQuantity = item.ReorderQuantity,
            IsLowStock = item.IsLowStock,
            LastRestocked = item.LastRestocked,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        };
    }
}
