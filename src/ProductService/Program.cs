using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Services;
using ProductService.Middleware;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// ================================================================================
// Add services to the container
// ================================================================================

// Database Configuration - Using SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=sqlserver;Database=ProductDb;User Id=sa;Password=ComplexP@ssw0rd123!;TrustServerCertificate=True";

builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(connectionString));

// Redis Configuration - For caching product catalog
var redisConnectionString = builder.Configuration.GetConnectionString("RedisConnection")
    ?? "redis:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisConnectionString));
builder.Services.AddScoped<ICacheService, RedisCacheService>();

// Configuration Service - For loading config.yaml
builder.Services.Configure<Config>(builder.Configuration.GetSection("Config"));

// Add Controllers
builder.Services.AddControllers();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Product Service API", Version = "v1" });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new() { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            Array.Empty<string>()
        }
    });
});

// ================================================================================
// Authentication & Authorization - KeyCloak Integration
// ================================================================================

var keycloakUrl = builder.Configuration["KeyCloak:Url"] ?? "http://keycloak:8080";
var realm = builder.Configuration["KeyCloak:Realm"] ?? "ecommerce-realm";
var clientId = builder.Configuration["KeyCloak:ClientId"] ?? "product-service";
var clientSecret = builder.Configuration["KeyCloak:ClientSecret"] ?? "product-secret-key";

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = $"{keycloakUrl}/realms/{realm}";
        options.Audience = clientId;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new()
        {
            // Validate issuer and audience
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            
            // Valid issuer format: http://keycloak:8080/realms/ecommerce-realm
            ValidIssuer = $"{keycloakUrl}/realms/{realm}",
            
            // Clock skew for token validation
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

// Configure Swagger in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Service API v1"));
}

// Custom Exception Handling Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Use CORS
app.UseCors("AllowAll");

// Use Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map Controllers
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "Product Service" }))
    .WithTags("Health");

// Initialize database and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // Apply pending migrations
        await context.Database.MigrateAsync();
        logger.LogInformation("Database migration completed successfully");
        
        // Seed initial data if needed
        await context.SeedDataAsync(logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating or seeding the database");
    }
}

app.Run();

// ================================================================================
// Database Context Extension Methods
// ================================================================================

public static class ProductDbContextExtensions
{
    public static async Task SeedDataAsync(this ProductDbContext context, ILogger logger)
    {
        // Check if data already exists
        if (context.Products.Any())
        {
            return;
        }
        
        logger.LogInformation("Seeding initial product data...");
        
        var products = new List<Product>
        {
            new Product
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Laptop Pro 15",
                Description = "High-performance laptop with 16GB RAM, 512GB SSD",
                Category = "Electronics",
                Price = 1299.99m,
                StockQuantity = 50,
                Brand = "TechMax",
                Sku = "LAP-PRO-15",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Wireless Headphones",
                Description = "Premium noise-cancelling wireless headphones",
                Category = "Electronics",
                Price = 299.99m,
                StockQuantity = 100,
                Brand = "SoundX",
                Sku = "WH-1000",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Smart Watch Series 8",
                Description = "Advanced smartwatch with health monitoring",
                Category = "Wearables",
                Price = 449.99m,
                StockQuantity = 75,
                Brand = "WatchTech",
                Sku = "SW-8",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Name = "Gaming Mouse RGB",
                Description = "Professional gaming mouse with programmable buttons",
                Category = "Accessories",
                Price = 79.99m,
                StockQuantity = 200,
                Brand = "GamePro",
                Sku = "GM-RGB",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Name = "4K Monitor 27 inch",
                Description = "Ultra HD 4K monitor with HDR support",
                Category = "Electronics",
                Price = 599.99m,
                StockQuantity = 30,
                Brand = "ViewMax",
                Sku = "VM-27-4K",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };
        
        context.Products.AddRange(products);
        await context.SaveChangesAsync();
        
        logger.LogInformation("Seeded {Count} products successfully", products.Count);
    }
}
