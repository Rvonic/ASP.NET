using PrviLabos.Domain;

namespace PrviLabos.Data;

public sealed class SupportTicketMockRepository : IMockRepository<SupportTicket>
{
    private readonly IReadOnlyList<SupportTicket> _items;

    public SupportTicketMockRepository(SupportDataContext context)
    {
        _items = context.Tickets;
    }

    public IReadOnlyList<SupportTicket> GetAll() => _items;

    public SupportTicket? GetById(int id) => _items.FirstOrDefault(x => x.Id == id);
}
