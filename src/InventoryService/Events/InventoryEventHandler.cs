using Confluent.Kafka;
using InventoryService.Services;
using System.Text.Json;

namespace InventoryService.Events;

/// <summary>
/// Inventory Event Handler
/// 
/// Listens to Kafka topic for order events and automatically deducts inventory.
/// Implements retry pattern for fault tolerance.
/// </summary>
public class InventoryEventHandler : IDisposable
{
    private readonly ConsumerConfig _consumerConfig;
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<InventoryEventHandler> _logger;
    private IConsumer<string, string>? _consumer;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private bool _disposed;

    // Kafka topic
    private const string ORDER_TOPIC = "order-placed";
    
    // Maximum retry attempts
    private const int MAX_RETRY_ATTEMPTS = 3;
    
    // Retry delay in milliseconds
    private const int RETRY_DELAY_MS = 1000;

    public InventoryEventHandler(
        ConsumerConfig consumerConfig,
        IInventoryService inventoryService,
        ILogger<InventoryEventHandler> logger)
    {
        _consumerConfig = consumerConfig;
        _inventoryService = inventoryService;
        _logger = logger;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    /// <summary>
    /// Start consuming Kafka messages
    /// </summary>
    public async Task StartConsumingAsync()
    {
        _logger.LogInformation("Starting Kafka consumer for inventory service");
        
        try
        {
            _consumer = new ConsumerBuilder<string, string>(_consumerConfig).Build();
            _consumer.Subscribe(ORDER_TOPIC);
            
            _logger.LogInformation("Subscribed to Kafka topic: {Topic}", ORDER_TOPIC);
            
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(_cancellationTokenSource.Token);
                    if (result != null)
                    {
                        await ProcessMessageAsync(result.Message);
                        _consumer.Commit(result);
                    }
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error consuming message from Kafka");
                    await Task.Delay(RETRY_DELAY_MS, _cancellationTokenSource.Token);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in Kafka consumer");
            throw;
        }
    }

    /// <summary>
    /// Process incoming Kafka message
    /// </summary>
    private async Task ProcessMessageAsync(Message<string, string> message)
    {
        var json = message.Value;
        _logger.LogDebug("Processing message: {Json}", json);
        
        try
        {
            var orderEvent = JsonSerializer.Deserialize<OrderPlacedEvent>(json);
            if (orderEvent == null)
            {
                _logger.LogWarning("Failed to deserialize order event");
                return;
            }
            
            // Process each item in the order
            foreach (var item in orderEvent.Items)
            {
                var success = await ProcessWithRetryAsync(
                    Guid.Parse(item.Key),
                    item.Value.Quantity);
                
                if (!success)
                {
                    _logger.LogError("Failed to deduct inventory after {MaxRetries} attempts for product {ProductId}",
                        MAX_RETRY_ATTEMPTS, item.Key);
                }
            }
            
            _logger.LogInformation("Processed order event: {OrderId}", orderEvent.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order event");
        }
    }

    /// <summary>
    /// Process inventory deduction with retry pattern
    /// </summary>
    private async Task<bool> ProcessWithRetryAsync(Guid productId, int quantity)
    {
        for (int attempt = 1; attempt <= MAX_RETRY_ATTEMPTS; attempt++)
        {
            try
            {
                var success = await _inventoryService.DeductStockAsync(productId, quantity);
                if (success)
                {
                    _logger.LogInformation("Successfully deducted {Quantity} for product {ProductId} on attempt {Attempt}",
                        quantity, productId, attempt);
                    return true;
                }
                
                _logger.LogWarning("Insufficient stock for product {ProductId}, attempt {Attempt}/{MaxAttempts}",
                    productId, attempt, MAX_RETRY_ATTEMPTS);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error deducting stock for product {ProductId}, attempt {Attempt}/{MaxAttempts}",
                    productId, attempt, MAX_RETRY_ATTEMPTS);
            }
            
            // Wait before retry
            if (attempt < MAX_RETRY_ATTEMPTS)
            {
                await Task.Delay(RETRY_DELAY_MS * attempt);
            }
        }
        
        return false;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _cancellationTokenSource.Cancel();
            _consumer?.Close();
            _consumer?.Dispose();
            _cancellationTokenSource.Dispose();
            _disposed = true;
        }
    }
}

/// <summary>
/// OrderPlaced Event Model
/// </summary>
public class OrderPlacedEvent
{
    public string EventType { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double TotalAmount { get; set; }
    public string Timestamp { get; set; } = string.Empty;
    public Dictionary<string, OrderItemData> Items { get; set; } = new();
}

public class OrderItemData
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public double Price { get; set; }
}
