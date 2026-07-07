using FruitSalesCalculator.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace FruitSalesCalculator.Data.EfCore;

/// <summary>
/// EF Core context for the fruit catalogue. DiscountRule is mapped as an
/// owned entity - it has no identity of its own, so it lives in the Fruits
/// table as extra columns rather than in a table of its own, mirroring the
/// domain's view that a rule is part of its fruit.
/// </summary>
public class FruitShopDbContext : DbContext
{
    public FruitShopDbContext(DbContextOptions<FruitShopDbContext> options)
        : base(options) { }

    public DbSet<Fruit> Fruits => Set<Fruit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var fruit = modelBuilder.Entity<Fruit>();

        fruit.HasKey(f => f.Name);
        fruit.Property(f => f.Name).HasMaxLength(100);
        fruit.Property(f => f.BasePrice).HasPrecision(18, 4);
        fruit.Property(f => f.PricingType).HasConversion<string>().HasMaxLength(20);

        fruit.OwnsOne(f => f.Discount, d =>
        {
            d.Property(x => x.ThresholdQuantity).HasPrecision(18, 4);
            d.Property(x => x.DiscountPercentage).HasPrecision(5, 4);
            d.Property(x => x.Kind).HasConversion<string>().HasMaxLength(20);
        });
    }
}