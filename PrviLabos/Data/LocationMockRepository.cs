using PrviLabos.Domain;

namespace PrviLabos.Data;

public sealed class LocationMockRepository : IMockRepository<Location>
{
    private readonly IReadOnlyList<Location> _items;

    public LocationMockRepository(SupportDataContext context)
    {
        _items = context.Locations;
    }

    public IReadOnlyList<Location> GetAll() => _items;

    public Location? GetById(int id) => _items.FirstOrDefault(x => x.Id == id);
}
