using PrviLabos.Domain;

namespace PrviLabos.Data;

public sealed class MockRepositorySet
{
    private MockRepositorySet(SupportDataContext context)
    {
        Context = context;
        Customers = new CustomerMockRepository(context);
        Locations = new LocationMockRepository(context);
        Vehicles = new VehicleMockRepository(context);
        Bookings = new BookingMockRepository(context);
        Agents = new SupportAgentMockRepository(context);
        Tickets = new SupportTicketMockRepository(context);
    }

    public SupportDataContext Context { get; }
    public IMockRepository<Customer> Customers { get; }
    public IMockRepository<Location> Locations { get; }
    public IMockRepository<Vehicle> Vehicles { get; }
    public IMockRepository<Booking> Bookings { get; }
    public IMockRepository<SupportAgent> Agents { get; }
    public IMockRepository<SupportTicket> Tickets { get; }

    public static MockRepositorySet CreateDefault()
    {
        var context = SupportDataSeeder.CreateSeededContext();
        return new MockRepositorySet(context);
    }
}
