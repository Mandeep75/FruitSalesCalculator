using FruitSalesCalculator.Core.Domain;
using FruitSalesCalculator.Core.Pricing;
using FruitSalesCalculator.Core.Repositories;
using FruitSalesCalculator.Core.Services;
using Moq;
using Xunit;

namespace FruitSalesCalculator.Tests.Unit;

public class OrderPricingServiceTests
{
    private readonly Mock<IFruitRepository> _repositoryMock = new();
    private readonly Mock<IPricingStrategyFactory> _factoryMock = new();

    private OrderPricingService CreateSut()
        => new(_repositoryMock.Object, _factoryMock.Object);

    [Fact]
    public void CalculateOrderTotal_SingleLine_ReturnsStrategyResultAsTotal()
    {
        // Arrange
        var apple = new Fruit("Apple", 2.00m, PricingType.PerWeight);
        _repositoryMock.Setup(r => r.GetByName("Apple")).Returns(apple);

        var strategyMock = new Mock<IPricingStrategy>();
        strategyMock.Setup(s => s.CalculatePrice(2.00m, 1.5m)).Returns(3.00m);
        _factoryMock.Setup(f => f.CreateFor(apple)).Returns(strategyMock.Object);

        var order = new Order(new[] { new OrderLine("Apple", 1.5m) });
        var sut = CreateSut();

        // Act
        var result = sut.CalculateOrderTotal(order);

        // Assert
        Assert.Equal(3.00m, result.Total);
        Assert.Single(result.Lines);
        strategyMock.Verify(s => s.CalculatePrice(2.00m, 1.5m), Times.Once);
    }

    [Fact]
    public void CalculateOrderTotal_MultipleLines_SumsLineTotals()
    {
        // Arrange
        var apple = new Fruit("Apple", 2.00m, PricingType.PerWeight);
        var banana = new Fruit("Banana", 0.30m, PricingType.PerItem);
        _repositoryMock.Setup(r => r.GetByName("Apple")).Returns(apple);
        _repositoryMock.Setup(r => r.GetByName("Banana")).Returns(banana);

        var appleStrategy = new Mock<IPricingStrategy>();
        appleStrategy.Setup(s => s.CalculatePrice(2.00m, 1.5m)).Returns(3.00m);
        var bananaStrategy = new Mock<IPricingStrategy>();
        bananaStrategy.Setup(s => s.CalculatePrice(0.30m, 4m)).Returns(1.20m);

        _factoryMock.Setup(f => f.CreateFor(apple)).Returns(appleStrategy.Object);
        _factoryMock.Setup(f => f.CreateFor(banana)).Returns(bananaStrategy.Object);

        var order = new Order(new[]
        {
            new OrderLine("Apple", 1.5m),
            new OrderLine("Banana", 4m),
        });
        var sut = CreateSut();

        // Act
        var result = sut.CalculateOrderTotal(order);

        // Assert
        Assert.Equal(4.20m, result.Total);
        Assert.Equal(2, result.Lines.Count);
    }

    [Fact]
    public void CalculateOrderTotal_UnknownFruit_ThrowsKeyNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByName(It.IsAny<string>())).Returns((Fruit?)null);
        var order = new Order(new[] { new OrderLine("Durian", 1m) });
        var sut = CreateSut();

        // Act
        Action act = () => sut.CalculateOrderTotal(order);

        // Assert
        var ex = Assert.Throws<KeyNotFoundException>(act);
        Assert.Contains("Durian", ex.Message);
    }

    [Fact]
    public void CalculateOrderTotal_NullOrder_Throws()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        Action act = () => sut.CalculateOrderTotal(null!);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }
}