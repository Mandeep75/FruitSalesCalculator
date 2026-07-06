using FruitSalesCalculator.Core.Domain;

namespace FruitSalesCalculator.Core.Pricing;

/// <summary>
/// Factory contract - an interface so services depending on it can be
/// unit tested with a mock, and so a different composition approach
/// (e.g. a registry) could be swapped in without touching consumers.
/// </summary>
public interface IPricingStrategyFactory
{
    IPricingStrategy CreateFor(Fruit fruit);
}