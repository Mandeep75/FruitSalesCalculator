namespace FruitSalesCalculator.Core.Pricing;

/// <summary>
/// Decorator applying a marginal ("tariff-style") discount: quantity up to
/// the threshold is priced in full; only the excess is discounted.
/// E.g. threshold 2kg at $5.00/kg with 10% off: 3kg = 2kg full + 1kg at 90%.
/// </summary>
public class TieredDiscountDecorator : IPricingStrategy
{
    private readonly IPricingStrategy _inner;
    private readonly decimal _thresholdQuantity;
    private readonly decimal _discountPercentage;

    public TieredDiscountDecorator(
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
        if (quantity <= _thresholdQuantity)
            return _inner.CalculatePrice(baseUnitPrice, quantity);

        var fullPricedPortion = _inner.CalculatePrice(baseUnitPrice, _thresholdQuantity);
        var discountedPortion =
            _inner.CalculatePrice(baseUnitPrice, quantity - _thresholdQuantity)
            * (1 - _discountPercentage);

        return fullPricedPortion + discountedPortion;
    }
}