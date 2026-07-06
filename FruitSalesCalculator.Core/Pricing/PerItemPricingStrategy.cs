namespace FruitSalesCalculator.Core.Pricing;

/// <summary>
/// Prices a fruit sold by item count, e.g. Bananas at $0.30 each.
/// Item-based fruit can't be bought in fractions, so this strategy
/// rejects non-whole quantities - a rule that genuinely doesn't apply
/// to per-weight pricing.
/// </summary>
public class PerItemPricingStrategy : IPricingStrategy
{
    public decimal CalculatePrice(decimal baseUnitPrice, decimal quantity)
    {
        if (quantity != Math.Floor(quantity))
            throw new ArgumentException(
                "Item-based fruit must be ordered in whole numbers.", nameof(quantity));

        return baseUnitPrice * quantity;
    }
}