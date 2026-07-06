namespace FruitSalesCalculator.Core.Pricing;

/// <summary>
/// Prices a fruit sold by weight, e.g. Apples at $2.00 per kg.
/// Fractional quantities are valid - 1.35kg of apples is a normal order.
/// </summary>
public class PerWeightPricingStrategy : IPricingStrategy
{
    public decimal CalculatePrice(decimal baseUnitPrice, decimal quantity)
        => baseUnitPrice * quantity;
}