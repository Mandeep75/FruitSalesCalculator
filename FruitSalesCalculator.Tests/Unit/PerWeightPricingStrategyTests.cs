using FruitSalesCalculator.Core.Pricing;
using Xunit;

namespace FruitSalesCalculator.Tests.Unit;

public class PerWeightPricingStrategyTests
{
    private readonly PerWeightPricingStrategy _sut = new();

    [Fact]
    public void CalculatePrice_WholeKilograms_MultipliesPriceByWeight()
    {
        // Arrange
        var basePrice = 2.00m;
        var weightKg = 3m;

        // Act
        var result = _sut.CalculatePrice(basePrice, weightKg);

        // Assert
        Assert.Equal(6.00m, result);
    }

    [Fact]
    public void CalculatePrice_FractionalWeight_IsSupported()
    {
        // Arrange
        var basePrice = 2.00m;
        var weightKg = 1.5m;

        // Act
        var result = _sut.CalculatePrice(basePrice, weightKg);

        // Assert
        Assert.Equal(3.00m, result);
    }
}

public class PerItemPricingStrategyTests
{
    private readonly PerItemPricingStrategy _sut = new();

    [Fact]
    public void CalculatePrice_WholeItemCount_MultipliesPriceByCount()
    {
        // Arrange
        var basePrice = 0.30m;
        var itemCount = 4m;

        // Act
        var result = _sut.CalculatePrice(basePrice, itemCount);

        // Assert
        Assert.Equal(1.20m, result);
    }

    [Fact]
    public void CalculatePrice_FractionalItemCount_Throws()
    {
        // Arrange
        var basePrice = 0.30m;
        var fractionalCount = 2.5m;

        // Act
        Action act = () => _sut.CalculatePrice(basePrice, fractionalCount);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }
}