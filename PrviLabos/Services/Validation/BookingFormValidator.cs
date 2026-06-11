using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using PrviLabos.DAL;
using PrviLabos.Model;
using PrviLabos.Models;

namespace PrviLabos.Services.Validation;

public sealed class BookingFormValidator
{
    private readonly PrviLabosDbContext _context;

    public BookingFormValidator(PrviLabosDbContext context)
    {
        _context = context;
    }

    public async Task ValidateAsync(BookingFormModel model, ModelStateDictionary modelState, int? bookingId = null)
    {
        if (string.IsNullOrWhiteSpace(model.ReservationCode))
        {
            modelState.AddModelError(nameof(BookingFormModel.ReservationCode), "Reservation code is required.");
        }

        if (model.PickupAt >= model.PlannedDropoffAt)
        {
            modelState.AddModelError(nameof(BookingFormModel.PlannedDropoffAt), "Planned dropoff must be after pickup.");
        }

        if (model.ActualDropoffAt.HasValue && model.ActualDropoffAt.Value < model.PickupAt)
        {
            modelState.AddModelError(nameof(BookingFormModel.ActualDropoffAt), "Actual dropoff cannot be earlier than pickup.");
        }

        if (model.TotalPrice < 0)
        {
            modelState.AddModelError(nameof(BookingFormModel.TotalPrice), "Total price cannot be negative.");
        }

        if (!Enum.IsDefined(typeof(BookingStatus), model.Status))
        {
            modelState.AddModelError(nameof(BookingFormModel.Status), "Invalid booking status.");
        }

        if (!await _context.Customers.AnyAsync(c => c.Id == model.CustomerId))
        {
            modelState.AddModelError(nameof(BookingFormModel.CustomerId), "Selected customer does not exist.");
        }

        if (!await _context.Vehicles.AnyAsync(v => v.Id == model.VehicleId))
        {
            modelState.AddModelError(nameof(BookingFormModel.VehicleId), "Selected vehicle does not exist.");
        }

        if (!await _context.Locations.AnyAsync(l => l.Id == model.PickupLocationId))
        {
            modelState.AddModelError(nameof(BookingFormModel.PickupLocationId), "Selected pickup location does not exist.");
        }

        if (!await _context.Locations.AnyAsync(l => l.Id == model.PlannedDropoffLocationId))
        {
            modelState.AddModelError(nameof(BookingFormModel.PlannedDropoffLocationId), "Selected dropoff location does not exist.");
        }

        var duplicateCodeQuery = _context.Bookings.Where(b => b.ReservationCode == model.ReservationCode.Trim());
        if (bookingId.HasValue)
        {
            duplicateCodeQuery = duplicateCodeQuery.Where(b => b.Id != bookingId.Value);
        }

        if (await duplicateCodeQuery.AnyAsync())
        {
            modelState.AddModelError(nameof(BookingFormModel.ReservationCode), "Reservation code must be unique.");
        }
    }
}