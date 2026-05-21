using System.ComponentModel.DataAnnotations;
using PrviLabos.Model;

namespace PrviLabos.Models;

public abstract class BookingFormModel
{
    [Required]
    [StringLength(50)]
    public string ReservationCode { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int CustomerId { get; set; }

    [Range(1, int.MaxValue)]
    public int VehicleId { get; set; }

    [Range(1, int.MaxValue)]
    public int PickupLocationId { get; set; }

    [Range(1, int.MaxValue)]
    public int PlannedDropoffLocationId { get; set; }

    [Required]
    public DateTime PickupAt { get; set; }

    [Required]
    public DateTime PlannedDropoffAt { get; set; }

    public DateTime? ActualDropoffAt { get; set; }

    [Range(typeof(decimal), "0", "9999999999")]
    public decimal TotalPrice { get; set; }

    [Required]
    public BookingStatus Status { get; set; } = BookingStatus.Reserved;
}