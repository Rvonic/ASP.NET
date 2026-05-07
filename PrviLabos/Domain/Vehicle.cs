using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrviLabos.Model;

public class Vehicle
{
    [Key]
    public int Id { get; set; }

    public string PlateNumber { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int ProductionYear { get; set; }
    public VehicleCategory Category { get; set; }
    public decimal DailyRate { get; set; }
    public int CurrentMileage { get; set; }
    public bool IsAvailable { get; set; }

    [ForeignKey(nameof(CurrentLocation))]
    public int? CurrentLocationId { get; set; }

    public Location? CurrentLocation { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
