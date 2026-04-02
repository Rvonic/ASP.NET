namespace PrviLabos.Domain;

public class SupportAgent
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public DateTime ShiftStart { get; set; }
    public DateTime ShiftEnd { get; set; }
    public bool IsOnDuty { get; set; }
    public List<SupportTicket> AssignedTickets { get; set; } = new();
}
