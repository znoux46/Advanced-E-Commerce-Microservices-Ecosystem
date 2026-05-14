using Microsoft.EntityFrameworkCore;
using InventoryService.Models;

namespace InventoryService.Data;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) 
        : base(options)
    {
    }

    // DbSet for Inventory Items
    public DbSet<InventoryItem> InventoryItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure InventoryItem entity
        modelBuilder.Entity<InventoryItem>(entity =>
        {
            // Primary key
            entity.HasKey(e => e.Id);
            
            // Indexes
            entity.HasIndex(e => e.ProductId).IsUnique();
            entity.HasIndex(e => e.AvailableQuantity);
            
            // Required fields
            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
            
            // Default values
            entity.Property(e => e.AvailableQuantity).HasDefaultValue(0);
            entity.Property(e => e.ReservedQuantity).HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
        });
    }
}
