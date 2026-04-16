using PrviLabos.Domain;

namespace PrviLabos.Data;

public sealed class BookingMockRepository : IMockRepository<Booking>
{
    private readonly IReadOnlyList<Booking> _items;

    public BookingMockRepository(SupportDataContext context)
    {
        _items = context.Bookings;
    }

    public IReadOnlyList<Booking> GetAll() => _items;

    public Booking? GetById(int id) => _items.FirstOrDefault(x => x.Id == id);
}
