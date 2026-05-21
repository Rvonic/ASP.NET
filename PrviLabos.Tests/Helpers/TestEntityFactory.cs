using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PrviLabos.Model;

namespace PrviLabos.Tests.Helpers;

public static class TestEntityFactory
{
    public static Customer ValidCustomer()
    {
        return new Customer
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            // PhoneNumber: 3-digit country code + 9-digit local number, digits only
            PhoneNumber = "385123456789",
            // Driver license: 9 alphanumeric characters
            DriverLicenseNumber = "AA1234567",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-30)),
            RegisteredAt = DateTime.UtcNow
        };
    }

    public static Location ValidLocation()
    {
        return new Location
        {
            Name = "Main",
            City = "City",
            Address = "Street 1",
            ContactPhone = "+1-555-0000",
            OpenAt = new TimeOnly(8, 0),
            CloseAt = new TimeOnly(20, 0),
            ParkingCapacity = 10
        };
    }

    public static Vehicle ValidVehicle()
    {
        return new Vehicle
        {
            PlateNumber = "ABC-123",
            Brand = "Make",
            Model = "Model",
            ProductionYear = DateTime.Now.Year - 1,
            Category = VehicleCategory.Sedan,
            DailyRate = 49.99m,
            CurrentMileage = 10000,
            IsAvailable = true
        };
    }

    public static Booking ValidBooking()
    {
        var customer = ValidCustomer();
        var vehicle = ValidVehicle();
        var pickup = ValidLocation();
        var dropoff = ValidLocation();

        return new Booking
        {
            ReservationCode = "R-001",
            Customer = customer,
            CustomerId = 1,
            Vehicle = vehicle,
            VehicleId = 1,
            PickupLocation = pickup,
            PickupLocationId = 1,
            PlannedDropoffLocation = dropoff,
            PlannedDropoffLocationId = 2,
            PickupAt = DateTime.UtcNow.AddDays(1),
            PlannedDropoffAt = DateTime.UtcNow.AddDays(3),
            TotalPrice = 100m,
            Status = BookingStatus.Reserved
        };
    }

    public static SupportAgent ValidSupportAgent()
    {
        return new SupportAgent
        {
            FullName = "Agent Smith",
            Email = "agent@example.com",
            TeamName = "Support",
            ShiftStart = DateTime.Today.AddHours(8),
            ShiftEnd = DateTime.Today.AddHours(16),
            IsOnDuty = true
        };
    }

    public static SupportTicket ValidSupportTicket()
    {
        var booking = ValidBooking();
        var loc = ValidLocation();

        return new SupportTicket
        {
            TicketNumber = "T-001",
            Booking = booking,
            BookingId = booking.Id,
            RequestedDropoffLocation = loc,
            RequestedDropoffLocationId = loc.Id,
            Description = "Issue",
            CreatedAt = DateTime.UtcNow,
            Priority = TicketPriority.Medium,
            Status = TicketStatus.Open
        };
    }

    public static bool IsValid(object model, out List<ValidationResult> results)
    {
        results = new List<ValidationResult>();
        var context = new ValidationContext(model);
        var valid = Validator.TryValidateObject(model, context, results, true);

        switch (model)
        {
            case Booking b:
                if (b.Customer == null) results.Add(new ValidationResult("Customer is required", new[] { nameof(Booking.Customer) }));
                if (b.Vehicle == null) results.Add(new ValidationResult("Vehicle is required", new[] { nameof(Booking.Vehicle) }));
                if (b.PickupLocation == null) results.Add(new ValidationResult("PickupLocation is required", new[] { nameof(Booking.PickupLocation) }));
                if (b.PlannedDropoffAt <= b.PickupAt) results.Add(new ValidationResult("PlannedDropoffAt must be after PickupAt", new[] { nameof(Booking.PlannedDropoffAt), nameof(Booking.PickupAt) }));
                break;
            case Vehicle v:
                var year = DateTime.Now.Year;
                if (v.ProductionYear < 1886 || v.ProductionYear > year) results.Add(new ValidationResult("ProductionYear is out of realistic range", new[] { nameof(Vehicle.ProductionYear) }));
                if (v.DailyRate <= 0) results.Add(new ValidationResult("DailyRate must be positive", new[] { nameof(Vehicle.DailyRate) }));
                if (string.IsNullOrWhiteSpace(v.PlateNumber)) results.Add(new ValidationResult("PlateNumber is required", new[] { nameof(Vehicle.PlateNumber) }));
                break;
            case Location l:
                if (l.ParkingCapacity < 0) results.Add(new ValidationResult("ParkingCapacity cannot be negative", new[] { nameof(Location.ParkingCapacity) }));
                if (l.OpenAt >= l.CloseAt) results.Add(new ValidationResult("OpenAt must be before CloseAt", new[] { nameof(Location.OpenAt), nameof(Location.CloseAt) }));
                break;
            case SupportAgent s:
                if (string.IsNullOrWhiteSpace(s.FullName)) results.Add(new ValidationResult("FullName is required", new[] { nameof(SupportAgent.FullName) }));
                if (s.ShiftEnd <= s.ShiftStart) results.Add(new ValidationResult("ShiftEnd must be after ShiftStart", new[] { nameof(SupportAgent.ShiftEnd), nameof(SupportAgent.ShiftStart) }));
                break;
            case SupportTicket t:
                if (t.Booking == null) results.Add(new ValidationResult("Booking is required", new[] { nameof(SupportTicket.Booking) }));
                if (t.RequestedDropoffLocation == null) results.Add(new ValidationResult("RequestedDropoffLocation is required", new[] { nameof(SupportTicket.RequestedDropoffLocation) }));
                if (string.IsNullOrWhiteSpace(t.Description)) results.Add(new ValidationResult("Description is required", new[] { nameof(SupportTicket.Description) }));
                break;
            case Customer c:
                    // Phone: must be digits only and equal to 3 (country) + 9 (local) = 12 digits
                    if (!string.IsNullOrWhiteSpace(c.PhoneNumber))
                    {
                        var digitsOnly = true;
                        var digits = 0;
                        foreach (var ch in c.PhoneNumber)
                        {
                            if (!char.IsDigit(ch)) digitsOnly = false;
                            else digits++;
                        }
                        if (!digitsOnly || digits != 12) results.Add(new ValidationResult("PhoneNumber must be 12 digits (3-digit country + 9-digit local)", new[] { nameof(Customer.PhoneNumber) }));
                    }

                    // Driver license: exactly 9 alphanumeric characters
                    if (!string.IsNullOrWhiteSpace(c.DriverLicenseNumber))
                    {
                        var trimmed = c.DriverLicenseNumber.Trim();
                        if (trimmed.Length != 9) results.Add(new ValidationResult("DriverLicenseNumber must have exactly 9 characters", new[] { nameof(Customer.DriverLicenseNumber) }));
                        else
                        {
                            foreach (var ch in trimmed)
                            {
                                if (!char.IsLetterOrDigit(ch))
                                {
                                    results.Add(new ValidationResult("DriverLicenseNumber must be alphanumeric", new[] { nameof(Customer.DriverLicenseNumber) }));
                                    break;
                                }
                            }
                        }
                    }
                break;
        }

        return valid && results.Count == 0;
    }
}
