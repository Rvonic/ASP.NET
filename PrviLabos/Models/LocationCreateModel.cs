using System.ComponentModel.DataAnnotations;

namespace PrviLabos.Models;

public sealed class LocationCreateModel
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string City { get; set; } = string.Empty;

    [Required]
    public string Address { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Država")]
    public string PhoneCountryCode { get; set; } = PhoneCountryCatalog.DefaultDialCode;

    [Required]
    [RegularExpression(@"^\d{9}$", ErrorMessage = "Broj telefona mora imati točno 9 znamenki.")]
    [StringLength(9, MinimumLength = 9, ErrorMessage = "Broj telefona mora imati točno 9 znamenki.")]
    [Display(Name = "Broj telefona")]
    public string PhoneLocalNumber { get; set; } = string.Empty;

    [Required]
    public TimeOnly OpenAt { get; set; }

    [Required]
    public TimeOnly CloseAt { get; set; }

    [Range(1, int.MaxValue)]
    public int ParkingCapacity { get; set; }
}