using FruitSalesCalculator.Core.Domain;

namespace FruitSalesCalculator.Core.Pricing;

/// <summary>
/// Factory pattern: centralises the "which strategy does this fruit need?"
/// decision in exactly one place. Composes in two steps:
///
///   1. Pick the base strategy from the fruit's PricingType.
///   2. If the fruit carries a DiscountRule, wrap the base strategy in the
///      decorator matching the rule's DiscountKind.
///
/// Adding a new pricing type or discount kind means one new class plus one
/// new case here - no existing pricing logic or consumers change.
/// </summary>
public class PricingStrategyFactory : IPricingStrategyFactory
{
    public IPricingStrategy CreateFor(Fruit fruit)
    {
        ArgumentNullException.ThrowIfNull(fruit);

        var strategy = CreateBaseStrategy(fruit.PricingType);

        return fruit.Discount is null
            ? strategy
            : ApplyDiscount(strategy, fruit.Discount);
    }

    private static IPricingStrategy CreateBaseStrategy(PricingType pricingType)
        => pricingType switch
        {
            PricingType.PerWeight => new PerWeightPricingStrategy(),
            PricingType.PerItem => new PerItemPricingStrategy(),
            _ => throw new NotSupportedException(
                $"Pricing type '{pricingType}' is not supported.")
        };

    private static IPricingStrategy ApplyDiscount(IPricingStrategy inner, DiscountRule rule)
        => rule.Kind switch
        {
            DiscountKind.WholeLine => new BulkDiscountDecorator(
                inner, rule.ThresholdQuantity, rule.DiscountPercentage),
            DiscountKind.Tiered => new TieredDiscountDecorator(
                inner, rule.ThresholdQuantity, rule.DiscountPercentage),
            _ => throw new NotSupportedException(
                $"Discount kind '{rule.Kind}' is not supported.")
        };
}