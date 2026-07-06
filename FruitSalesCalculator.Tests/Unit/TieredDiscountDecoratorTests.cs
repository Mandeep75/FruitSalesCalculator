using FruitSalesCalculator.Core.Pricing;
using Xunit;

namespace FruitSalesCalculator.Tests.Unit;

public class TieredDiscountDecoratorTests
{
    [Fact]
    public void CalculatePrice_QuantityAboveThreshold_DiscountsOnlyTheExcess()
    {
        // Arrange - 3kg cherries: 2kg full price + 1kg at 10% off
        var sut = new TieredDiscountDecorator(
            new PerWeightPricingStrategy(), thresholdQuantity: 2m, discountPercentage: 0.10m);

        // Act
        var result = sut.CalculatePrice(5.00m, 3m);

        // Assert - (2 * 5.00) + (1 * 5.00 * 0.90) = 10.00 + 4.50
        Assert.Equal(14.50m, result);
    }

    [Fact]
    public void CalculatePrice_QuantityAtThreshold_NoDiscount()
    {
        // Arrange
        var sut = new TieredDiscountDecorator(
            new PerWeightPricingStrategy(), thresholdQuantity: 2m, discountPercentage: 0.10m);

        // Act
        var result = sut.CalculatePrice(5.00m, 2m);

        // Assert
        Assert.Equal(10.00m, result);
    }

    [Fact]
    public void CalculatePrice_BelowThreshold_NoDiscount()
    {
        // Arrange
        var sut = new TieredDiscountDecorator(
            new PerWeightPricingStrategy(), thresholdQuantity: 2m, discountPercentage: 0.10m);

        // Act
        var result = sut.CalculatePrice(5.00m, 1.5m);

        // Assert
        Assert.Equal(7.50m, result);
    }

    [Fact]
    public void CalculatePrice_ComparedToWholeLine_TieredIsNeverCheaperAtSameInputs()
    {
        // Arrange - documents the behavioral difference between the two kinds:
        // whole-line discounts the full 3kg ($13.50); tiered only the excess ($14.50)
        var tiered = new TieredDiscountDecorator(
            new PerWeightPricingStrategy(), 2m, 0.10m);
        var wholeLine = new BulkDiscountDecorator(
            new PerWeightPricingStrategy(), 2m, 0.10m);

        // Act
        var tieredPrice = tiered.CalculatePrice(5.00m, 3m);
        var wholeLinePrice = wholeLine.CalculatePrice(5.00m, 3m);

        // Assert
        Assert.Equal(14.50m, tieredPrice);
        Assert.Equal(13.50m, wholeLinePrice);
        Assert.True(tieredPrice > wholeLinePrice);
    }
}