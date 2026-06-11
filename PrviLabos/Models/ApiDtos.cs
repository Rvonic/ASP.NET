using System.ComponentModel.DataAnnotations;
using PrviLabos.Model;

namespace PrviLabos.Models;

public sealed class CustomerDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string DriverLicenseNumber { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public DateTime RegisteredAt { get; set; }
}

public sealed class CustomerUpsertDto
{
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

    [Required]
    public DateOnly DateOfBirth { get; set; }
}

public sealed class LocationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public TimeOnly OpenAt { get; set; }
    public TimeOnly CloseAt { get; set; }
    public int ParkingCapacity { get; set; }
}

public sealed class LocationUpsertDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string City { get; set; } = string.Empty;

    [Required]
    public string Address { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string ContactPhone { get; set; } = string.Empty;

    [Required]
    public TimeOnly OpenAt { get; set; }

    [Required]
    public TimeOnly CloseAt { get; set; }

    [Range(1, int.MaxValue)]
    public int ParkingCapacity { get; set; }
}

public sealed class VehicleDto
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
    public LocationSummaryDto? CurrentLocation { get; set; }
}

public sealed class VehicleUpsertDto
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

public sealed class BookingDto
{
    public int Id { get; set; }
    public string ReservationCode { get; set; } = string.Empty;
    public CustomerSummaryDto Customer { get; set; } = null!;
    public VehicleSummaryDto Vehicle { get; set; } = null!;
    public LocationSummaryDto PickupLocation { get; set; } = null!;
    public LocationSummaryDto PlannedDropoffLocation { get; set; } = null!;
    public DateTime PickupAt { get; set; }
    public DateTime PlannedDropoffAt { get; set; }
    public DateTime? ActualDropoffAt { get; set; }
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; }
    public List<BookingAttachmentDto> Attachments { get; set; } = new();
}

public sealed class BookingUpsertDto
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

public sealed class SupportAgentDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public DateTime ShiftStart { get; set; }
    public DateTime ShiftEnd { get; set; }
    public bool IsOnDuty { get; set; }
}

public sealed class SupportAgentUpsertDto
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(120)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string TeamName { get; set; } = string.Empty;

    [Required]
    public DateTime ShiftStart { get; set; }

    [Required]
    public DateTime ShiftEnd { get; set; }

    public bool IsOnDuty { get; set; }
}

public sealed class SupportTicketDto
{
    public int Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public BookingSummaryDto Booking { get; set; } = null!;
    public LocationSummaryDto RequestedDropoffLocation { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public TicketPriority Priority { get; set; }
    public TicketStatus Status { get; set; }
    public List<SupportAgentDto> AssignedAgents { get; set; } = new();
}

public sealed class SupportTicketUpsertDto
{
    [Required]
    [StringLength(50)]
    public string TicketNumber { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int BookingId { get; set; }

    [Range(1, int.MaxValue)]
    public int RequestedDropoffLocationId { get; set; }

    [Required]
    [StringLength(4000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public TicketPriority Priority { get; set; }

    [Required]
    public TicketStatus Status { get; set; }

    public DateTime? ResolvedAt { get; set; }
    public List<int> AssignedAgentIds { get; set; } = new();
}

public sealed class BookingAttachmentDto
{
    public int Id { get; set; }
    public BookingSummaryDto Booking { get; set; } = null!;
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class BookingAttachmentUpsertDto
{
    [Range(1, int.MaxValue)]
    public int BookingId { get; set; }

    [Required]
    [StringLength(255)]
    public string OriginalFileName { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string StoredFileName { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string ContentType { get; set; } = string.Empty;

    [Range(1, long.MaxValue)]
    public long FileSize { get; set; }
}

public sealed class CustomerSummaryDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public sealed class LocationSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}

public sealed class VehicleSummaryDto
{
    public int Id { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
}

public sealed class BookingSummaryDto
{
    public int Id { get; set; }
    public string ReservationCode { get; set; } = string.Empty;
}
