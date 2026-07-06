using FruitSalesCalculator.Core.Domain;
using FruitSalesCalculator.Core.Pricing;
using FruitSalesCalculator.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace FruitSalesCalculator.Core.Services;

/// <summary>
/// Calculates the total cost of an order.
///
/// Note what this class deliberately does NOT know:
///   - how fruit is stored (hidden behind IFruitRepository)
///   - how any fruit is priced (hidden behind IPricingStrategy, composed
///     by the factory)
///
/// Its single responsibility is orchestration: resolve each line's fruit,
/// have the right strategy price it, sum the results. Adding new pricing
/// models or discount kinds never touches this class.
/// </summary>
public class OrderPricingService : IOrderPricingService
{
    private readonly IFruitRepository _fruitRepository;
    private readonly IPricingStrategyFactory _strategyFactory;

    public OrderPricingService(
        IFruitRepository fruitRepository,
        IPricingStrategyFactory strategyFactory)
    {
        _fruitRepository = fruitRepository
            ?? throw new ArgumentNullException(nameof(fruitRepository));
        _strategyFactory = strategyFactory
            ?? throw new ArgumentNullException(nameof(strategyFactory));
    }

    public OrderPricingResult CalculateOrderTotal(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        var pricedLines = new List<LinePricingResult>();

        foreach (var line in order.Lines)
        {
            var fruit = _fruitRepository.GetByName(line.FruitName)
                ?? throw new KeyNotFoundException(
                    $"No fruit named '{line.FruitName}' is configured.");

            var strategy = _strategyFactory.CreateFor(fruit);
            var lineTotal = strategy.CalculatePrice(fruit.BasePrice, line.Quantity);

            pricedLines.Add(new LinePricingResult(fruit.Name, line.Quantity, lineTotal));
        }

        var total = pricedLines.Sum(l => l.LineTotal);

        return new OrderPricingResult(order.Id, pricedLines, total);
    }
}

