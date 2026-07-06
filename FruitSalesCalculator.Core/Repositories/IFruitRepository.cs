using FruitSalesCalculator.Core.Domain;

namespace FruitSalesCalculator.Core.Repositories;

/// <summary>
/// Repository pattern: abstracts how the fruit catalogue is stored.
/// The rest of the system depends only on this interface, so the backing
/// store can be swapped (in-memory, EF Core + SQLite, ...) with a one-line
/// DI registration change and no impact on services or pricing logic.
/// </summary>
public interface IFruitRepository
{
    /// <summary>Returns the fruit with the given name (case-insensitive), or null if not configured.</summary>
    Fruit? GetByName(string name);

    /// <summary>Adds a fruit to the catalogue. Throws if a fruit with the same name already exists.</summary>
    void Add(Fruit fruit);

    /// <summary>Returns all configured fruit, ordered by name.</summary>
    IReadOnlyCollection<Fruit> GetAll();
}