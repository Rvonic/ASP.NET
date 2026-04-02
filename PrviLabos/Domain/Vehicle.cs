namespace PrviLabos.Domain;

public class Vehicle
{
    public int Id { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int ProductionYear { get; set; }
    public VehicleCategory Category { get; set; }
    public decimal DailyRate { get; set; }
    public int CurrentMileage { get; set; }
    public bool IsAvailable { get; set; }
    public Location? CurrentLocation { get; set; }
    public List<Booking> Bookings { get; set; } = new();
}
