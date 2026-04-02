namespace PrviLabos.Domain;

public class SupportTicket
{
    public int Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public Booking Booking { get; set; } = null!;
    public Location RequestedDropoffLocation { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public TicketPriority Priority { get; set; }
    public TicketStatus Status { get; set; }
    public List<SupportAgent> AssignedAgents { get; set; } = new();
}
