namespace PrviLabos.Domain;

public class Customer
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string DriverLicenseNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public DateTime RegisteredAt { get; set; }
    public List<Booking> Bookings { get; set; } = new();
}
