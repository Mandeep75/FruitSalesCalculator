using FruitSalesCalculator.Core.Domain;
using FruitSalesCalculator.Core.Repositories;
using Xunit;

namespace FruitSalesCalculator.Tests;

public class InMemoryFruitRepositoryTests
{
    private readonly InMemoryFruitRepository _sut = new();

    [Fact]
    public void GetByName_ExistingFruit_ReturnsIt()
    {
        // Arrange
        _sut.Add(new Fruit("Apple", 2.00m, PricingType.PerWeight));

        // Act
        var result = _sut.GetByName("Apple");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Apple", result!.Name);
    }

    [Fact]
    public void GetByName_DifferentCasing_StillFindsTheFruit()
    {
        // Arrange
        _sut.Add(new Fruit("Apple", 2.00m, PricingType.PerWeight));

        // Act
        var result = _sut.GetByName("aPPle");

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void GetByName_UnknownFruit_ReturnsNull()
    {
        // Arrange - empty repository

        // Act
        var result = _sut.GetByName("Durian");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Add_DuplicateNameDifferentCasing_Throws()
    {
        // Arrange
        _sut.Add(new Fruit("Apple", 2.00m, PricingType.PerWeight));

        // Act
        Action act = () => _sut.Add(new Fruit("apple", 3.00m, PricingType.PerWeight));

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void GetAll_ReturnsAllFruit_OrderedByName()
    {
        // Arrange
        _sut.Add(new Fruit("Cherry", 5.00m, PricingType.PerWeight));
        _sut.Add(new Fruit("Apple", 2.00m, PricingType.PerWeight));
        _sut.Add(new Fruit("Banana", 0.30m, PricingType.PerItem));

        // Act
        var all = _sut.GetAll();

        // Assert
        Assert.Equal(3, all.Count);
        Assert.Equal(new[] { "Apple", "Banana", "Cherry" }, all.Select(f => f.Name));
    }

    [Fact]
    public async Task Add_ManyConcurrentWrites_AllFruitsAreStored()
    {
        // Arrange - e.g. a bulk catalogue import writing from many threads
        const int count = 100;

        // Act
        var tasks = Enumerable.Range(0, count)
            .Select(i => Task.Run(() =>
                _sut.Add(new Fruit($"Fruit{i}", 1.00m, PricingType.PerWeight))))
            .ToArray();

        await Task.WhenAll(tasks);

        // Assert - no lost writes: every one of the 100 adds landed
        Assert.Equal(count, _sut.GetAll().Count);
    }
}