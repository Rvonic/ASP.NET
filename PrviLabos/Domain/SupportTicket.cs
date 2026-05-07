using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrviLabos.Model;

public class SupportTicket
{
    [Key]
    public int Id { get; set; }

    public string TicketNumber { get; set; } = string.Empty;

    [ForeignKey(nameof(Booking))]
    public int BookingId { get; set; }

    public Booking Booking { get; set; } = null!;

    [ForeignKey(nameof(RequestedDropoffLocation))]
    public int RequestedDropoffLocationId { get; set; }

    public Location RequestedDropoffLocation { get; set; } = null!;

    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public TicketPriority Priority { get; set; }
    public TicketStatus Status { get; set; }

    public virtual ICollection<SupportAgent> AssignedAgents { get; set; } = new List<SupportAgent>();
}
