using System.Globalization;
using FruitSalesCalculator.Core.Services;

namespace FruitSalesCalculator.Core.Printing;

/// <summary>
/// Writes a line-by-line receipt to the console. Culture is pinned to en-AU
/// so currency formats consistently ($13.50) regardless of the machine's
/// regional settings - formatting is a presentation decision, so it lives
/// here, not in the pricing logic.
/// </summary>
public class ConsoleReceiptPrinter : IReceiptPrinter
{
    private static readonly CultureInfo Currency = CultureInfo.GetCultureInfo("en-AU");

    public void Print(OrderPricingResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        Console.WriteLine($"Order {result.OrderId}");
        Console.WriteLine(new string('-', 44));

        foreach (var line in result.Lines)
        {
            Console.WriteLine(
                $"{line.FruitName,-12} x {line.Quantity,7:0.###}   {line.LineTotal.ToString("C", Currency),10}");
        }

        Console.WriteLine(new string('-', 44));
        Console.WriteLine($"{"TOTAL",-12}   {"",7}   {result.Total.ToString("C", Currency),10}");
        Console.WriteLine();
    }
}