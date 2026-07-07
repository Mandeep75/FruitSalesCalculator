using FruitSalesCalculator.Core.Domain;
using FruitSalesCalculator.Data.EfCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FruitSalesCalculator.Tests.Integration;

/// <summary>
/// Exercises the EF Core repository against a real SQLite engine
/// (in-memory database, kept alive by holding the connection open).
/// These are genuine infrastructure tests - hence Integration/.
/// </summary>
public class EfCoreFruitRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly FruitShopDbContext _context;
    private readonly EfCoreFruitRepository _sut;

    public EfCoreFruitRepositoryTests()
    {
        // An in-memory SQLite database lives exactly as long as the
        // connection that created it stays open.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<FruitShopDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new FruitShopDbContext(options);
        _context.Database.EnsureCreated();
        _sut = new EfCoreFruitRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public void Add_ThenGetByName_RoundTripsTheFruit()
    {
        // Arrange
        var apple = new Fruit("Apple", 2.00m, PricingType.PerWeight);

        // Act
        _sut.Add(apple);
        var result = _sut.GetByName("Apple");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Apple", result!.Name);
        Assert.Equal(2.00m, result.BasePrice);
        Assert.Equal(PricingType.PerWeight, result.PricingType);
        Assert.Null(result.Discount);
    }

    [Fact]
    public void Add_FruitWithDiscount_RoundTripsTheOwnedRule()
    {
        // Arrange - the owned DiscountRule must survive the database round trip
        var cherry = new Fruit("Cherry", 5.00m, PricingType.PerWeight,
            new DiscountRule(2m, 0.10m, DiscountKind.Tiered));

        // Act
        _sut.Add(cherry);
        var result = _sut.GetByName("Cherry");

        // Assert
        Assert.NotNull(result!.Discount);
        Assert.Equal(2m, result.Discount!.ThresholdQuantity);
        Assert.Equal(0.10m, result.Discount.DiscountPercentage);
        Assert.Equal(DiscountKind.Tiered, result.Discount.Kind);
    }

    [Fact]
    public void GetByName_DifferentCasing_StillFindsTheFruit()
    {
        // Arrange
        _sut.Add(new Fruit("Apple", 2.00m, PricingType.PerWeight));

        // Act
        var result = _sut.GetByName("aPPle");

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void Add_DuplicateNameDifferentCasing_Throws()
    {
        // Arrange
        _sut.Add(new Fruit("Apple", 2.00m, PricingType.PerWeight));

        // Act
        Action act = () => _sut.Add(new Fruit("apple", 3.00m, PricingType.PerWeight));

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void GetAll_ReturnsFruitsOrderedByName()
    {
        // Arrange
        _sut.Add(new Fruit("Cherry", 5.00m, PricingType.PerWeight));
        _sut.Add(new Fruit("Apple", 2.00m, PricingType.PerWeight));

        // Act
        var all = _sut.GetAll();

        // Assert
        Assert.Equal(new[] { "Apple", "Cherry" }, all.Select(f => f.Name));
    }
}