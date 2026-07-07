using FruitSalesCalculator.Core.Services;

namespace FruitSalesCalculator.Core.Printing;

/// <summary>
/// Output abstraction: the pricing system produces an OrderPricingResult;
/// how that result is presented (console, invoice file, email...) is a
/// separate concern behind this interface. Console output is simply the
/// first implementation.
/// </summary>
public interface IReceiptPrinter
{
    void Print(OrderPricingResult result);
}