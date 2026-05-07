using Microsoft.EntityFrameworkCore;
using PrviLabos.DAL;
using PrviLabos.Model;

namespace PrviLabos.Services;

public class DropoffSupportService
{
    private readonly PrviLabosDbContext _context;

    public DropoffSupportService(PrviLabosDbContext context)
    {
        _context = context;
    }

    public List<Booking> GetLateDropoffs(DateTime now)
    {
        return _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Vehicle)
            .Where(b => b.Status == BookingStatus.Active)
            .Where(b => b.ActualDropoffAt == null && b.PlannedDropoffAt < now)
            .OrderByDescending(b => b.PlannedDropoffAt)
            .ToList();
    }

    public List<SupportTicket> GetActiveEscalations()
    {
        return _context.Tickets
            .Include(t => t.Booking)
            .ThenInclude(b => b.Customer)
            .Where(t => t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress || t.Status == TicketStatus.Escalated)
            .Where(t => t.Priority == TicketPriority.High || t.Priority == TicketPriority.Critical)
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .ToList();
    }

    public List<(string location, int requests)> GetMostRequestedDropoffLocations()
    {
        return _context.Tickets
            .Include(t => t.RequestedDropoffLocation)
            .GroupBy(t => t.RequestedDropoffLocation.Name)
            .AsEnumerable()
            .Select(g => (location: g.Key, requests: g.Count()))
            .OrderByDescending(x => x.requests)
            .ThenBy(x => x.location)
            .ToList();
    }

    public List<(string customer, decimal totalSpent)> GetTopCustomersBySpending(int top)
    {
        return _context.Customers
            .Include(c => c.Bookings)
            .Select(c => new
            {
                FullName = $"{c.FirstName} {c.LastName}",
                TotalSpent = c.Bookings.Sum(b => b.TotalPrice)
            })
            .AsEnumerable()
            .OrderByDescending(x => x.TotalSpent)
            .Take(top)
            .Select(x => (x.FullName, x.TotalSpent))
            .ToList();
    }

    public async Task<int> ProcessOpenTicketsAsync()
    {
        var openTickets = _context.Tickets
            .Include(t => t.Booking)
            .Include(t => t.RequestedDropoffLocation)
            .Where(t => t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress || t.Status == TicketStatus.Escalated)
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
