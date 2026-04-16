using PrviLabos.Domain;

namespace PrviLabos.Data;

public sealed class CustomerMockRepository : IMockRepository<Customer>
{
    private readonly IReadOnlyList<Customer> _items;

    public CustomerMockRepository(SupportDataContext context)
    {
        _items = context.Customers;
    }

    public IReadOnlyList<Customer> GetAll() => _items;

    public Customer? GetById(int id) => _items.FirstOrDefault(x => x.Id == id);
}
