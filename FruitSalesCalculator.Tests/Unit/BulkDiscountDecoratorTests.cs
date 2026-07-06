using FruitSalesCalculator.Core.Pricing;
using Xunit;

namespace FruitSalesCalculator.Tests.Unit;

public class BulkDiscountDecoratorTests
{
    [Fact]
    public void CalculatePrice_QuantityAboveThreshold_AppliesDiscount()
    {
        // Arrange - the Cherry scenario from the brief: $5.00/kg, 10% off above 2kg
        var sut = new BulkDiscountDecorator(
            new PerWeightPricingStrategy(), thresholdQuantity: 2m, discountPercentage: 0.10m);

        // Act
        var result = sut.CalculatePrice(5.00m, 3m);

        // Assert - 3kg * $5.00 = $15.00, minus 10% = $13.50
        Assert.Equal(13.50m, result);
    }

    [Fact]
    public void CalculatePrice_QuantityExactlyAtThreshold_DoesNotApplyDiscount()
    {
        // Arrange - the spec says "more than 2kg", so exactly 2kg pays full price
        var sut = new BulkDiscountDecorator(
            new PerWeightPricingStrategy(), thresholdQuantity: 2m, discountPercentage: 0.10m);

        // Act
        var result = sut.CalculatePrice(5.00m, 2m);

        // Assert
        Assert.Equal(10.00m, result);
    }

    [Fact]
    public void CalculatePrice_QuantityBelowThreshold_DoesNotApplyDiscount()
    {
        // Arrange
        var sut = new BulkDiscountDecorator(
            new PerWeightPricingStrategy(), thresholdQuantity: 2m, discountPercentage: 0.10m);

        // Act
        var result = sut.CalculatePrice(5.00m, 1m);

        // Assert
        Assert.Equal(5.00m, result);
    }

    [Fact]
    public void CalculatePrice_WrapsPerItemStrategy_DiscountAlsoApplies()
    {
        // Arrange - proves the decorator composes with ANY strategy:
        // "$0.50 per item, 20% off when buying more than 10"
        var sut = new BulkDiscountDecorator(
            new PerItemPricingStrategy(), thresholdQuantity: 10m, discountPercentage: 0.20m);

        // Act
        var result = sut.CalculatePrice(0.50m, 12m);

        // Assert - 12 * $0.50 = $6.00, minus 20% = $4.80
        Assert.Equal(4.80m, result);
    }

    [Fact]
    public void Constructor_DiscountOfOneOrMore_Throws()
    {
        // Arrange
        var inner = new PerWeightPricingStrategy();

        // Act
        var act = () => new BulkDiscountDecorator(inner, thresholdQuantity: 2m, discountPercentage: 1.5m);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }
}