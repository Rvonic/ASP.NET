using PrviLabos.Model;

namespace PrviLabos.Models;

public static class ApiMapping
{
    public static CustomerDto ToDto(this Customer customer) => new()
    {
        Id = customer.Id,
        FirstName = customer.FirstName,
        LastName = customer.LastName,
        Email = customer.Email,
        PhoneNumber = customer.PhoneNumber,
        DriverLicenseNumber = customer.DriverLicenseNumber,
        DateOfBirth = customer.DateOfBirth,
        RegisteredAt = customer.RegisteredAt
    };

    public static CustomerSummaryDto ToSummaryDto(this Customer customer) => new()
    {
        Id = customer.Id,
        FullName = $"{customer.FirstName} {customer.LastName}",
        Email = customer.Email
    };

    public static LocationDto ToDto(this Location location) => new()
    {
        Id = location.Id,
        Name = location.Name,
        City = location.City,
        Address = location.Address,
        ContactPhone = location.ContactPhone,
        OpenAt = location.OpenAt,
        CloseAt = location.CloseAt,
        ParkingCapacity = location.ParkingCapacity
    };

    public static LocationSummaryDto ToSummaryDto(this Location location) => new()
    {
        Id = location.Id,
        Name = location.Name,
        City = location.City
    };

    public static VehicleDto ToDto(this Vehicle vehicle) => new()
    {
        Id = vehicle.Id,
        PlateNumber = vehicle.PlateNumber,
        Brand = vehicle.Brand,
        Model = vehicle.Model,
        ProductionYear = vehicle.ProductionYear,
        Category = vehicle.Category,
        DailyRate = vehicle.DailyRate,
        CurrentMileage = vehicle.CurrentMileage,
        IsAvailable = vehicle.IsAvailable,
        CurrentLocation = vehicle.CurrentLocation?.ToSummaryDto()
    };

    public static VehicleSummaryDto ToSummaryDto(this Vehicle vehicle) => new()
    {
        Id = vehicle.Id,
        PlateNumber = vehicle.PlateNumber,
        Brand = vehicle.Brand,
        Model = vehicle.Model
    };

    public static BookingDto ToDto(this Booking booking) => new()
    {
        Id = booking.Id,
        ReservationCode = booking.ReservationCode,
        Customer = booking.Customer.ToSummaryDto(),
        Vehicle = booking.Vehicle.ToSummaryDto(),
        PickupLocation = booking.PickupLocation.ToSummaryDto(),
        PlannedDropoffLocation = booking.PlannedDropoffLocation.ToSummaryDto(),
        PickupAt = booking.PickupAt,
        PlannedDropoffAt = booking.PlannedDropoffAt,
        ActualDropoffAt = booking.ActualDropoffAt,
        TotalPrice = booking.TotalPrice,
        Status = booking.Status,
        Attachments = booking.Attachments.Select(attachment => attachment.ToDto(booking)).ToList()
    };

    public static BookingSummaryDto ToSummaryDto(this Booking booking) => new()
    {
        Id = booking.Id,
        ReservationCode = booking.ReservationCode
    };

    public static BookingAttachmentDto ToDto(this BookingAttachment attachment) =>
        attachment.ToDto(attachment.Booking);

    public static BookingAttachmentDto ToDto(this BookingAttachment attachment, Booking booking) => new()
    {
        Id = attachment.Id,
        Booking = booking.ToSummaryDto(),
        OriginalFileName = attachment.OriginalFileName,
        StoredFileName = attachment.StoredFileName,
        FilePath = attachment.FilePath,
        ContentType = attachment.ContentType,
        FileSize = attachment.FileSize,
        CreatedAt = attachment.CreatedAt
    };

    public static SupportAgentDto ToDto(this SupportAgent agent) => new()
    {
        Id = agent.Id,
        FullName = agent.FullName,
        Email = agent.Email,
        TeamName = agent.TeamName,
        ShiftStart = agent.ShiftStart,
        ShiftEnd = agent.ShiftEnd,
        IsOnDuty = agent.IsOnDuty
    };

    public static SupportTicketDto ToDto(this SupportTicket ticket) => new()
    {
        Id = ticket.Id,
        TicketNumber = ticket.TicketNumber,
        Booking = ticket.Booking.ToSummaryDto(),
        RequestedDropoffLocation = ticket.RequestedDropoffLocation.ToSummaryDto(),
        Description = ticket.Description,
        CreatedAt = ticket.CreatedAt,
        ResolvedAt = ticket.ResolvedAt,
        Priority = ticket.Priority,
        Status = ticket.Status,
        AssignedAgents = ticket.AssignedAgents.Select(agent => agent.ToDto()).ToList()
    };
}
