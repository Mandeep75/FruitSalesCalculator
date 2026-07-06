namespace FruitSalesCalculator.Core.Domain;

/// <summary>
/// A customer order consisting of one or more order lines.
/// </summary>
public class Order
{
    public Guid Id { get; }
    public DateTime CreatedAtUtc { get; }
    public IReadOnlyList<OrderLine> Lines { get; }

    public Order(IEnumerable<OrderLine> lines)
    {
        var lineList = lines?.ToList() ?? throw new ArgumentNullException(nameof(lines));
        if (lineList.Count == 0)
            throw new ArgumentException("An order must contain at least one line.", nameof(lines));

        Id = Guid.NewGuid();
        CreatedAtUtc = DateTime.UtcNow;
        Lines = lineList;
    }
}

/// <summary>
/// A single line in an order - a fruit and how much of it was ordered.
/// Quantity is deliberately generic: it means kilograms for per-weight fruit
/// and item count for per-item fruit. The fruit's pricing strategy is what
/// gives the number its meaning.
/// </summary>
public class OrderLine
{
    public string FruitName { get; }
    public decimal Quantity { get; }

    public OrderLine(string fruitName, decimal quantity)
    {
        if (string.IsNullOrWhiteSpace(fruitName))
            throw new ArgumentException("Fruit name is required.", nameof(fruitName));
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity),
                "Quantity must be greater than zero.");

        FruitName = fruitName;
        Quantity = quantity;
    }
}
