using PrviLabos.Domain;

namespace PrviLabos.Data;

public sealed class SupportAgentMockRepository : IMockRepository<SupportAgent>
{
    private readonly IReadOnlyList<SupportAgent> _items;

    public SupportAgentMockRepository(SupportDataContext context)
    {
        _items = context.Agents;
    }

    public IReadOnlyList<SupportAgent> GetAll() => _items;

    public SupportAgent? GetById(int id) => _items.FirstOrDefault(x => x.Id == id);
}
