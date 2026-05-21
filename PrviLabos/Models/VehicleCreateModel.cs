using System.ComponentModel.DataAnnotations;
using PrviLabos.Model;

namespace PrviLabos.Models;

public sealed class VehicleCreateModel
{
    [Required]
    [StringLength(20)]
    public string PlateNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Brand { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Model { get; set; } = string.Empty;

    [Range(1900, 2100)]
    public int ProductionYear { get; set; }

    [Required]
    public VehicleCategory Category { get; set; }

    [Range(typeof(decimal), "0", "9999999999")]
    public decimal DailyRate { get; set; }

    [Range(0, int.MaxValue)]
    public int CurrentMileage { get; set; }

    public bool IsAvailable { get; set; }

    public int? CurrentLocationId { get; set; }
}