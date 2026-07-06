namespace FruitSalesCalculator.Core.Services;


/// <summary>
/// The fully priced order: per-line breakdown plus grand total.
/// </summary>
public record OrderPricingResult(Guid OrderId, IReadOnlyList<LinePricingResult> Lines, decimal Total);
/// <summary>
/// The priced result for a single order line - kept so receipts can show
/// a per-line breakdown, not just a grand total.
/// </summary>
public record LinePricingResult(string FruitName, decimal Quantity, decimal LineTotal);

