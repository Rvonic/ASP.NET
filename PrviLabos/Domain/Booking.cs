using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrviLabos.Model;

public class Booking
{
    [Key]
    public int Id { get; set; }

    public string ReservationCode { get; set; } = string.Empty;

    [ForeignKey(nameof(Customer))]
    public int CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;

    [ForeignKey(nameof(Vehicle))]
    public int VehicleId { get; set; }

    public Vehicle Vehicle { get; set; } = null!;

    [ForeignKey(nameof(PickupLocation))]
    public int PickupLocationId { get; set; }

    public Location PickupLocation { get; set; } = null!;

    [ForeignKey(nameof(PlannedDropoffLocation))]
    public int PlannedDropoffLocationId { get; set; }

    public Location PlannedDropoffLocation { get; set; } = null!;

    public DateTime PickupAt { get; set; }
    public DateTime PlannedDropoffAt { get; set; }
    public DateTime? ActualDropoffAt { get; set; }
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; }

    public virtual ICollection<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();
}
