using System.ComponentModel.DataAnnotations;

namespace PrviLabos.Model;

public class Location
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public TimeOnly OpenAt { get; set; }
    public TimeOnly CloseAt { get; set; }
    public int ParkingCapacity { get; set; }

    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

    public virtual ICollection<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();
}
