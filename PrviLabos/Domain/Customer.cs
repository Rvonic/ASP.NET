using System.ComponentModel.DataAnnotations;

namespace PrviLabos.Model;

public class Customer
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public string DriverLicenseNumber { get; set; } = string.Empty;

    public DateOnly DateOfBirth { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
