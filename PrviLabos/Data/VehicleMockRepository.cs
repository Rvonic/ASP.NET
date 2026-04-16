using PrviLabos.Domain;

namespace PrviLabos.Data;

public sealed class VehicleMockRepository : IMockRepository<Vehicle>
{
    private readonly IReadOnlyList<Vehicle> _items;

    public VehicleMockRepository(SupportDataContext context)
    {
        _items = context.Vehicles;
    }

    public IReadOnlyList<Vehicle> GetAll() => _items;

    public Vehicle? GetById(int id) => _items.FirstOrDefault(x => x.Id == id);
}
