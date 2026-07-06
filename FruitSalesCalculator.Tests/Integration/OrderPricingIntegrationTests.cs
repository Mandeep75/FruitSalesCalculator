using FruitSalesCalculator.Core.Domain;
using FruitSalesCalculator.Core.Pricing;
using FruitSalesCalculator.Core.Repositories;
using FruitSalesCalculator.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace FruitSalesCalculator.Tests.Integration;

/// <summary>
/// End-to-end tests running the real repository, factory, strategies and
/// service together - including the exact worked example from the brief,
/// and many orders priced concurrently against the shared catalogue.
/// </summary>
public class OrderPricingIntegrationTests
{
    private static OrderPricingService BuildRealSystem()
    {
        var repository = new InMemoryFruitRepository();
        repository.Add(new Fruit("Apple", 2.00m, PricingType.PerWeight));
        repository.Add(new Fruit("Banana", 0.30m, PricingType.PerItem));
        repository.Add(new Fruit("Cherry", 5.00m, PricingType.PerWeight,
            new DiscountRule(2m, 0.10m)));

        return new OrderPricingService(repository, new PricingStrategyFactory());
    }

    [Fact]
    public void CalculateOrderTotal_ExampleFruitsFromBrief_ProducesExpectedTotal()
    {
        // Arrange
        var sut = BuildRealSystem();
        var order = new Order(new[]
        {
            new OrderLine("Apple", 1.5m),   // 1.5kg * $2.00           = $3.00
            new OrderLine("Banana", 4m),    // 4 * $0.30               = $1.20
            new OrderLine("Cherry", 3m),    // 3kg * $5.00, 10% off    = $13.50
        });

        // Act
        var result = sut.CalculateOrderTotal(order);

        // Assert
        Assert.Equal(17.70m, result.Total);
        Assert.Equal(3, result.Lines.Count);
        Assert.Equal(13.50m, result.Lines.Single(l => l.FruitName == "Cherry").LineTotal);
    }

    [Fact]
    public async Task CalculateOrderTotal_ManyConcurrentOrders_AllPriceCorrectly()
    {
        // Arrange - one shared catalogue, many simultaneous customers.
        // The repository's ConcurrentDictionary makes concurrent lookups
        // safe, and Fruit's immutability makes the shared instances safe
        // to read - so every order must produce the same correct total.
        var sut = BuildRealSystem();
        const int concurrentOrders = 100;

        // Act
        var tasks = Enumerable.Range(0, concurrentOrders)
            .Select(_ => Task.Run(() =>
            {
                var order = new Order(new[]
                {
                    new OrderLine("Apple", 1.5m),   // $3.00
                    new OrderLine("Cherry", 3m),    // $13.50
                });
                return sut.CalculateOrderTotal(order);
            }))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        // Assert - no interference between concurrently priced orders
        Assert.All(results, priced => Assert.Equal(16.50m, priced.Total));
    }
}
