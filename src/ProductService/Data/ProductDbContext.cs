using Microsoft.EntityFrameworkCore;
using ProductService.Models;

namespace ProductService.Data;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) 
        : base(options)
    {
    }

    // DbSet for Products
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Product entity
        modelBuilder.Entity<Product>(entity =>
        {
            // Primary key
            entity.HasKey(e => e.Id);
            
            // Indexes for frequently queried fields
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.Brand);
            entity.HasIndex(e => e.Sku).IsUnique();
            entity.HasIndex(e => e.IsActive);
            
            // Required fields
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Price).IsRequired().HasPrecision(18, 2);
            
            // Default values
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            // Configure relationship with OrderItem
            entity.HasMany(e => e.OrderItems)
                  .WithOne(oi => oi.Product)
                  .HasForeignKey(oi => oi.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
