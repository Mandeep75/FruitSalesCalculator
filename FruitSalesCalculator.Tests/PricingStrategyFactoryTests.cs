using FruitSalesCalculator.Core.Domain;
using FruitSalesCalculator.Core.Pricing;
using Xunit;

namespace FruitSalesCalculator.Tests;

public class PricingStrategyFactoryTests
{
    private readonly PricingStrategyFactory _sut = new();

    [Fact]
    public void CreateFor_PerWeightFruit_ReturnsPerWeightStrategy()
    {
        // Arrange
        var fruit = new Fruit("Apple", 2.00m, PricingType.PerWeight);

        // Act
        var strategy = _sut.CreateFor(fruit);

        // Assert
        Assert.IsType<PerWeightPricingStrategy>(strategy);
    }

    [Fact]
    public void CreateFor_PerItemFruit_ReturnsPerItemStrategy()
    {
        // Arrange
        var fruit = new Fruit("Banana", 0.30m, PricingType.PerItem);

        // Act
        var strategy = _sut.CreateFor(fruit);

        // Assert
        Assert.IsType<PerItemPricingStrategy>(strategy);
    }

    [Fact]
    public void CreateFor_FruitWithWholeLineDiscount_ReturnsBulkDiscountDecorator()
    {
        // Arrange - DiscountKind defaults to WholeLine, matching the brief's Cherry
        var fruit = new Fruit("Cherry", 5.00m, PricingType.PerWeight,
            new DiscountRule(2m, 0.10m));

        // Act
        var strategy = _sut.CreateFor(fruit);

        // Assert
        Assert.IsType<BulkDiscountDecorator>(strategy);
    }

    [Fact]
    public void CreateFor_FruitWithTieredDiscount_ReturnsTieredDiscountDecorator()
    {
        // Arrange
        var fruit = new Fruit("Mango", 4.00m, PricingType.PerWeight,
            new DiscountRule(5m, 0.15m, DiscountKind.Tiered));

        // Act
        var strategy = _sut.CreateFor(fruit);

        // Assert
        Assert.IsType<TieredDiscountDecorator>(strategy);
    }

    [Fact]
    public void CreateFor_DiscountedFruit_ProducesCorrectEndToEndPrice()
    {
        // Arrange - behavioral check, not just type check: the composed
        // strategy must actually price the brief's Cherry example correctly
        var cherry = new Fruit("Cherry", 5.00m, PricingType.PerWeight,
            new DiscountRule(2m, 0.10m));

        // Act
        var strategy = _sut.CreateFor(cherry);
        var price = strategy.CalculatePrice(cherry.BasePrice, 3m);

        // Assert - 3kg * $5.00 = $15.00, minus 10% = $13.50
        Assert.Equal(13.50m, price);
    }

    [Fact]
    public void CreateFor_TieredDiscountFruit_ProducesCorrectEndToEndPrice()
    {
        // Arrange - same shape as the Cherry rule, but Tiered:
        // 2kg at full price, only the excess kg discounted
        var mango = new Fruit("Mango", 5.00m, PricingType.PerWeight,
            new DiscountRule(2m, 0.10m, DiscountKind.Tiered));

        // Act
        var strategy = _sut.CreateFor(mango);
        var price = strategy.CalculatePrice(mango.BasePrice, 3m);

        // Assert - (2 * 5.00) + (1 * 5.00 * 0.90) = 10.00 + 4.50 = 14.50
        Assert.Equal(14.50m, price);
    }

    [Fact]
    public void CreateFor_NullFruit_Throws()
    {
        // Arrange
        Fruit fruit = null!;

        // Act
        Action act = () => _sut.CreateFor(fruit);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }
}