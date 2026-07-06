using System.Collections.Concurrent;
using FruitSalesCalculator.Core.Domain;

namespace FruitSalesCalculator.Core.Repositories;

/// <summary>
/// Default in-memory catalogue, backed by a ConcurrentDictionary so it is
/// safe under concurrent access: many orders being priced (reads) while
/// the catalogue is being set up or extended (writes). Combined with the
/// immutability of Fruit itself, no locking is needed anywhere - the
/// dictionary protects its own structure, and the objects inside it
/// cannot change once created.
///
/// Lookup is case-insensitive: "apple" and "Apple" are the same fruit.
/// </summary>
public class InMemoryFruitRepository : IFruitRepository
{
    private readonly ConcurrentDictionary<string, Fruit> _store =
        new(StringComparer.OrdinalIgnoreCase);

    public Fruit? GetByName(string name)
        => _store.TryGetValue(name, out var fruit) ? fruit : null;

    public void Add(Fruit fruit)
    {
        ArgumentNullException.ThrowIfNull(fruit);

        if (!_store.TryAdd(fruit.Name, fruit))
            throw new InvalidOperationException(
                $"A fruit named '{fruit.Name}' is already configured.");
    }

    public IReadOnlyCollection<Fruit> GetAll()
        => _store.Values.OrderBy(f => f.Name).ToList();
}