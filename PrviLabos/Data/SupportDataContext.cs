using PrviLabos.Domain;

namespace PrviLabos.Data;

public class SupportDataContext
{
    public List<Customer> Customers { get; init; } = new();
    public List<Location> Locations { get; init; } = new();
    public List<Vehicle> Vehicles { get; init; } = new();
    public List<Booking> Bookings { get; init; } = new();
    public List<SupportAgent> Agents { get; init; } = new();
    public List<SupportTicket> Tickets { get; init; } = new();
}
