using FruitSalesCalculator.Core.Domain;
using FruitSalesCalculator.Core.Pricing;
using FruitSalesCalculator.Core.Printing;
using FruitSalesCalculator.Core.Repositories;
using FruitSalesCalculator.Core.Services;
using Microsoft.Extensions.DependencyInjection;

// -----------------------------------------------------------------
// Composition root: the one place that knows which concrete types
// fulfil which interfaces. Swapping the repository for a database-
// backed one, or the printer for an invoice writer, is a one-line
// change here - nothing in Core would move.
// -----------------------------------------------------------------
var services = new ServiceCollection();

services.AddSingleton<IFruitRepository, InMemoryFruitRepository>();
services.AddSingleton<IPricingStrategyFactory, PricingStrategyFactory>();
services.AddTransient<IOrderPricingService, OrderPricingService>();
services.AddTransient<IReceiptPrinter, ConsoleReceiptPrinter>();

using var provider = services.BuildServiceProvider();

// -----------------------------------------------------------------
// 1. Configuration: the brief's three fruits, plus a tiered-discount
//    fruit to demonstrate the second DiscountKind. Cherry's kind
//    defaults to WholeLine.
// -----------------------------------------------------------------
var repository = provider.GetRequiredService<IFruitRepository>();

repository.Add(new Fruit("Apple", 2.00m, PricingType.PerWeight));
repository.Add(new Fruit("Banana", 0.30m, PricingType.PerItem));
repository.Add(new Fruit("Cherry", 5.00m, PricingType.PerWeight,
    new DiscountRule(thresholdQuantity: 2m, discountPercentage: 0.10m)));
repository.Add(new Fruit("Mango", 5.00m, PricingType.PerWeight,
    new DiscountRule(thresholdQuantity: 2m, discountPercentage: 0.10m, DiscountKind.Tiered)));

var pricingService = provider.GetRequiredService<IOrderPricingService>();
var printer = provider.GetRequiredService<IReceiptPrinter>();

// -----------------------------------------------------------------
// 2. The brief's example order, using all three pricing behaviours.
//    Expected: 3.00 + 1.20 + 13.50 = $17.70
// -----------------------------------------------------------------
var order = new Order(new[]
{
    new OrderLine("Apple", 1.5m),   // 1.5 kg  @ $2.00/kg            = $3.00
    new OrderLine("Banana", 4m),    // 4 items @ $0.30 each          = $1.20
    new OrderLine("Cherry", 3m),    // 3 kg    @ $5.00/kg, 10% off   = $13.50
});

printer.Print(pricingService.CalculateOrderTotal(order));

// -----------------------------------------------------------------
// 3. Whole-line vs tiered, side by side: identical price, threshold,
//    percentage and quantity - only the DiscountKind differs.
//    Cherry (whole line): 3kg * $5.00, all discounted   = $13.50
//    Mango  (tiered):     2kg full + 1kg at 90%          = $14.50
// -----------------------------------------------------------------
var discountComparison = new Order(new[]
{
    new OrderLine("Cherry", 3m),
    new OrderLine("Mango", 3m),
});

printer.Print(pricingService.CalculateOrderTotal(discountComparison));

// -----------------------------------------------------------------
// 4. At the discount threshold: exactly 2kg pays full price for
//    both kinds ("more than 2kg" is exclusive). Expected: $20.00
// -----------------------------------------------------------------
var thresholdOrder = new Order(new[]
{
    new OrderLine("Cherry", 2m),    // $10.00 - no discount
    new OrderLine("Mango", 2m),     // $10.00 - no discount
});

printer.Print(pricingService.CalculateOrderTotal(thresholdOrder));