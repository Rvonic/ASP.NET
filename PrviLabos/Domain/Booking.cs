namespace PrviLabos.Domain;

public class Booking
{
    public int Id { get; set; }
    public string ReservationCode { get; set; } = string.Empty;
    public Customer Customer { get; set; } = null!;
    public Vehicle Vehicle { get; set; } = null!;
    public Location PickupLocation { get; set; } = null!;
    public Location PlannedDropoffLocation { get; set; } = null!;
    public DateTime PickupAt { get; set; }
    public DateTime PlannedDropoffAt { get; set; }
    public DateTime? ActualDropoffAt { get; set; }
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; }
    public List<SupportTicket> SupportTickets { get; set; } = new();
}
