namespace PrviLabos.Domain;

public class Location
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public TimeOnly OpenAt { get; set; }
    public TimeOnly CloseAt { get; set; }
    public int ParkingCapacity { get; set; }
    public List<Vehicle> Vehicles { get; set; } = new();
}
