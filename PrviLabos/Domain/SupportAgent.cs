using System.ComponentModel.DataAnnotations;

namespace PrviLabos.Model;

public class SupportAgent
{
    [Key]
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public DateTime ShiftStart { get; set; }
    public DateTime ShiftEnd { get; set; }
    public bool IsOnDuty { get; set; }
    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<SupportTicket> AssignedTickets { get; set; } = new List<SupportTicket>();
}
