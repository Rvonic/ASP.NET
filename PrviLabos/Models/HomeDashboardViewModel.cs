using PrviLabos.Domain;

namespace PrviLabos.Models;

public class HomeDashboardViewModel
{
    public int ActiveBookings { get; set; }
    public int OpenTickets { get; set; }
    public int Customers { get; set; }
    public int OnDutyAgents { get; set; }
    public int UpcomingDropoffs { get; set; }
    public int EscalatedTickets { get; set; }
    public IReadOnlyList<Booking> LateDropoffBookings { get; set; } = new List<Booking>();
    public IReadOnlyList<Booking> RecentBookings { get; set; } = new List<Booking>();
    public IReadOnlyList<SupportTicket> RecentTickets { get; set; } = new List<SupportTicket>();
}