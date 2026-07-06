namespace FruitSalesCalculator.Core.Pricing;

/// <summary>
/// Strategy pattern: one interchangeable way of turning a base unit price
/// and an ordered quantity into a line price. Concrete strategies
/// (per-weight, per-item) are interchangeable behind this interface, and
/// decorators (e.g. bulk discount) can wrap any of them without callers
/// knowing the difference.
/// </summary>
public interface IPricingStrategy
{
    decimal CalculatePrice(decimal baseUnitPrice, decimal quantity);
}