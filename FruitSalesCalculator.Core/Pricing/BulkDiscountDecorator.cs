namespace FruitSalesCalculator.Core.Pricing;

/// <summary>
/// Decorator pattern: wraps ANY inner pricing strategy and applies a
/// percentage discount when the ordered quantity exceeds a threshold.
///
/// Because it implements the same interface it decorates, callers can't
/// tell whether they're holding a plain strategy or a discounted one.
/// This is what lets "Cherry: $5.00/kg with 10% off over 2kg" be composed
/// from existing pieces rather than hard-coded as a special case - and the
/// same decorator works unchanged over per-item pricing ("10% off when
/// you buy more than a dozen").
/// </summary>
public class BulkDiscountDecorator : IPricingStrategy
{
    private readonly IPricingStrategy _inner;
    private readonly decimal _thresholdQuantity;
    private readonly decimal _discountPercentage;

    public BulkDiscountDecorator(
        IPricingStrategy inner,
        decimal thresholdQuantity,
        decimal discountPercentage)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));

        if (thresholdQuantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(thresholdQuantity),
                "Threshold must be greater than zero.");
        if (discountPercentage is <= 0 or >= 1)
            throw new ArgumentOutOfRangeException(nameof(discountPercentage),
                "Discount must be a fraction between 0 and 1 (e.g. 0.10 for 10%).");

        _thresholdQuantity = thresholdQuantity;
        _discountPercentage = discountPercentage;
    }

    public decimal CalculatePrice(decimal baseUnitPrice, decimal quantity)
    {
        var basePrice = _inner.CalculatePrice(baseUnitPrice, quantity);

        return quantity > _thresholdQuantity
            ? basePrice * (1 - _discountPercentage)
            : basePrice;
    }
}