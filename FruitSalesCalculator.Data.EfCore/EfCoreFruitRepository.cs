using FruitSalesCalculator.Core.Domain;
using FruitSalesCalculator.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FruitSalesCalculator.Data.EfCore;

/// <summary>
/// EF Core + SQLite implementation of the same IFruitRepository contract the
/// in-memory version satisfies. Exists to demonstrate the repository
/// abstraction genuinely holds: swapping this in is a DI registration
/// change - no service or pricing code is touched.
/// </summary>
public class EfCoreFruitRepository : IFruitRepository
{
    private readonly FruitShopDbContext _context;

    public EfCoreFruitRepository(FruitShopDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public Fruit? GetByName(string name)
        => _context.Fruits
            .AsNoTracking()
            .FirstOrDefault(f => f.Name.ToLower() == name.ToLower());

    public void Add(Fruit fruit)
    {
        ArgumentNullException.ThrowIfNull(fruit);

        var exists = _context.Fruits.Any(f => f.Name.ToLower() == fruit.Name.ToLower());
        if (exists)
            throw new InvalidOperationException(
                $"A fruit named '{fruit.Name}' is already configured.");

        _context.Fruits.Add(fruit);
        _context.SaveChanges();
    }

    public IReadOnlyCollection<Fruit> GetAll()
        => _context.Fruits
            .AsNoTracking()
            .OrderBy(f => f.Name)
            .ToList();
}