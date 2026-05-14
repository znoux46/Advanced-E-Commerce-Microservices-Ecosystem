using Microsoft.EntityFrameworkCore;
using InventoryService.Data;
using InventoryService.Services;
using InventoryService.Events;
using Confluent.Kafka;

var builder = WebApplication.CreateBuilder(args);

// ================================================================================
// Add services to the container
// ================================================================================

// Database Configuration - Using PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=postgres;Port=5432;Database=ecommerce_db;Username=ecommerce_user;Password=SecureP@ss2024";

builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register services
builder.Services.AddScoped<IInventoryService, InventoryServiceImpl>();
builder.Services.AddScoped<IInventoryEventHandler, InventoryEventHandler>();

// Kafka Configuration for consuming OrderPlaced events
builder.Services.AddSingleton<ConsumerConfig>(sp =>
{
    var config = builder.Configuration.GetSection("Kafka");
    return new ConsumerConfig
    {
        BootstrapServers = config["BootstrapServers"] ?? "kafka:29092",
        GroupId = "inventory-service-group",
        AutoOffsetReset = AutoOffsetReset.Earliest,
        EnableAutoCommit = false
    };
});

// Add Controllers
builder.Services.AddControllers();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Inventory Service API", Version = "v1" });
});

// ================================================================================
// Authentication & Authorization - KeyCloak Integration
// ================================================================================

var keycloakUrl = builder.Configuration["KeyCloak:Url"] ?? "http://keycloak:8080";
var realm = builder.Configuration["KeyCloak:Realm"] ?? "ecommerce-realm";
var clientId = builder.Configuration["KeyCloak:ClientId"] ?? "inventory-service";

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = $"{keycloakUrl}/realms/{realm}";
        options.Audience = clientId;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = $"{keycloakUrl}/realms/{realm}",
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });

builder.Services.AddAuthorization();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Build the application
var app = builder.Build();

// ================================================================================
// Configure the HTTP request pipeline
// ================================================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventory Service API v1"));
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "Inventory Service" }))
    .WithTags("Health");

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        await context.Database.MigrateAsync();
        logger.LogInformation("Database migration completed successfully");
        
        // Seed initial inventory data
        await context.SeedDataAsync(logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating or seeding the database");
    }
}

// Start Kafka consumer in background
var kafkaConsumer = app.Services.GetRequiredService<InventoryEventHandler>();
_ = Task.Run(() => kafkaConsumer.StartConsumingAsync());

app.Run();

// ================================================================================
// Database Context Extension Methods
// ================================================================================

public static class InventoryDbContextExtensions
{
    public static async Task SeedDataAsync(this InventoryDbContext context, ILogger logger)
    {
        if (context.InventoryItems.Any())
        {
            return;
        }
        
        logger.LogInformation("Seeding initial inventory data...");
        
        var inventories = new List<InventoryItem>
        {
            new InventoryItem
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                ProductId = "11111111-1111-1111-1111-111111111111",
                ProductName = "Laptop Pro 15",
                AvailableQuantity = 50,
                ReservedQuantity = 0,
                ReorderLevel = 10,
                ReorderQuantity = 50,
                LastRestocked = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new InventoryItem
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                ProductId = "22222222-2222-2222-2222-222222222222",
                ProductName = "Wireless Headphones",
                AvailableQuantity = 100,
                ReservedQuantity = 0,
                ReorderLevel = 20,
                ReorderQuantity = 100,
                LastRestocked = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new InventoryItem
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                ProductId = "33333333-3333-3333-3333-333333333333",
                ProductName = "Smart Watch Series 8",
                AvailableQuantity = 75,
                ReservedQuantity = 0,
                ReorderLevel = 15,
                ReorderQuantity = 75,
                LastRestocked = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new InventoryItem
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                ProductId = "44444444-4444-4444-4444-444444444444",
                ProductName = "Gaming Mouse RGB",
                AvailableQuantity = 200,
                ReservedQuantity = 0,
                ReorderLevel = 50,
                ReorderQuantity = 100,
                LastRestocked = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new InventoryItem
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                ProductId = "55555555-5555-5555-5555-555555555555",
                ProductName = "4K Monitor 27 inch",
                AvailableQuantity = 30,
                ReservedQuantity = 0,
                ReorderLevel = 5,
                ReorderQuantity = 30,
                LastRestocked = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };
        
        context.InventoryItems.AddRange(inventories);
        await context.SaveChangesAsync();
        
        logger.LogInformation("Seeded {Count} inventory items successfully", inventories.Count);
    }
}
