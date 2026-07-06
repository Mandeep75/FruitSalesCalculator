namespace FruitSalesCalculator.Core.Domain;

/// <summary>
/// A fruit type the shop sells, along with how it should be priced.
/// Configured once (the "admin" side), then referenced whenever orders are priced.
/// </summary>
public class Fruit
{
    public string Name { get; }
    public decimal BasePrice { get; }
    public PricingType PricingType { get; }
    public DiscountRule? Discount { get; }

    public Fruit(string name, decimal basePrice, PricingType pricingType, DiscountRule? discount = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Fruit name is required.", nameof(name));
        if (basePrice <= 0)
            throw new ArgumentOutOfRangeException(nameof(basePrice),
                "Base price must be greater than zero.");

        Name = name;
        BasePrice = basePrice;
        PricingType = pricingType;
        Discount = discount;
    }
}

/// <summary>
/// How a fruit's quantity is measured when calculating price.
/// </summary>
public enum PricingType
{
    PerWeight,
    PerItem
}

/// <summary>
/// Optional bulk-discount rule attached to a fruit, e.g.
/// "10% off when the ordered quantity exceeds 2kg".
/// </summary>
public class DiscountRule
{
    public decimal ThresholdQuantity { get; }
    public decimal DiscountPercentage { get; }
    public DiscountKind Kind { get; }

    public DiscountRule(decimal thresholdQuantity, decimal discountPercentage, DiscountKind kind)
    {
        if (thresholdQuantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(thresholdQuantity),
                "Threshold must be greater than zero.");
        if (discountPercentage is <= 0 or >= 1)
            throw new ArgumentOutOfRangeException(nameof(discountPercentage),
                "Discount must be a fraction between 0 and 1 (e.g. 0.10 for 10%).");

        ThresholdQuantity = thresholdQuantity;
        DiscountPercentage = discountPercentage;
        Kind = kind;
    }
}
/// <summary>
/// How a discount applies once the threshold is exceeded.
/// </summary>
public enum DiscountKind
{
    /// <summary>The entire line is discounted (retail promotion style).</summary>
    WholeLine,

    /// <summary>Only the quantity above the threshold is discounted (tariff style).</summary>
    Tiered
}