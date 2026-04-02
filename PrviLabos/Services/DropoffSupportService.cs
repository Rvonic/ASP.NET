using PrviLabos.Data;
using PrviLabos.Domain;

namespace PrviLabos.Services;

public class DropoffSupportService
{
    private readonly SupportDataContext _context;

    public DropoffSupportService(SupportDataContext context)
    {
        _context = context;
    }

    public List<Booking> GetLateDropoffs(DateTime now)
    {
        return _context.Bookings
            .Where(b => b.Status == BookingStatus.Active)
            .Where(b => b.ActualDropoffAt is null && b.PlannedDropoffAt < now)
            .OrderByDescending(b => b.PlannedDropoffAt)
            .ToList();
    }

    public List<SupportTicket> GetActiveEscalations()
    {
        return _context.Tickets
            .Where(t => t.Status is TicketStatus.Open or TicketStatus.InProgress or TicketStatus.Escalated)
            .Where(t => t.Priority is TicketPriority.High or TicketPriority.Critical)
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .ToList();
    }

    public List<(string location, int requests)> GetMostRequestedDropoffLocations()
    {
        return _context.Tickets
            .GroupBy(t => t.RequestedDropoffLocation.Name)
            .Select(g => (location: g.Key, requests: g.Count()))
            .OrderByDescending(x => x.requests)
            .ThenBy(x => x.location)
            .ToList();
    }

    public List<(string customer, decimal totalSpent)> GetTopCustomersBySpending(int top)
    {
        return _context.Customers
            .Select(c => new
            {
                FullName = $"{c.FirstName} {c.LastName}",
                TotalSpent = c.Bookings.Sum(b => b.TotalPrice)
            })
            .OrderByDescending(x => x.TotalSpent)
            .Take(top)
            .Select(x => (x.FullName, x.TotalSpent))
            .ToList();
    }

    public async Task<int> ProcessOpenTicketsAsync()
    {
        var openTickets = _context.Tickets
            .Where(t => t.Status is TicketStatus.Open or TicketStatus.InProgress or TicketStatus.Escalated)
            .ToList();

        foreach (var ticket in openTickets)
        {
            await SimulateExternalDropoffValidationAsync(ticket);
            ticket.Status = TicketStatus.Resolved;
            ticket.ResolvedAt = DateTime.UtcNow;

            var booking = ticket.Booking;
            booking.PlannedDropoffLocation = ticket.RequestedDropoffLocation;
        }

        return openTickets.Count;
    }

    private static async Task SimulateExternalDropoffValidationAsync(SupportTicket ticket)
    {
        var delayMs = ticket.Priority switch
        {
            TicketPriority.Critical => 600,
            TicketPriority.High => 450,
            TicketPriority.Medium => 300,
            _ => 200
        };

        await Task.Delay(delayMs);
    }
}
