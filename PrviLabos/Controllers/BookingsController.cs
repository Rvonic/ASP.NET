using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PrviLabos.DAL;
using PrviLabos.Model;
using PrviLabos.Models;

namespace PrviLabos.Controllers;

[Route("rezervacije")]
public class BookingsController : Controller
{
    private readonly PrviLabosDbContext _context;

    public BookingsController(PrviLabosDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        var bookings = _context.Bookings
            .Where(b => b.DeletedAt == null)
            .Include(b => b.Customer)
            .Include(b => b.Vehicle)
            .Include(b => b.PickupLocation)
            .Include(b => b.PlannedDropoffLocation)
            .OrderByDescending(b => b.PickupAt)
            .ToList();

        return View(bookings);
    }

    [HttpGet("novi")]
    [ActionName("Create")]
    public IActionResult CreateGet()
    {
        PrepareLookups();

        return View(new BookingCreateModel
        {
            PickupAt = DateTime.Now,
            PlannedDropoffAt = DateTime.Now.AddHours(1),
            Status = BookingStatus.Reserved
        });
    }

    [HttpGet("pretraga")]
    public IActionResult Search(string? query)
    {
        var normalizedQuery = query?.Trim();

        var bookings = _context.Bookings
            .Where(b => b.DeletedAt == null)
            .Include(b => b.Customer)
            .Include(b => b.Vehicle)
            .Include(b => b.PickupLocation)
            .Include(b => b.PlannedDropoffLocation)
            .OrderByDescending(b => b.PickupAt)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(normalizedQuery))
        {
            bookings = bookings.Where(b =>
                b.ReservationCode.Contains(normalizedQuery) ||
                b.Customer.FirstName.Contains(normalizedQuery) ||
                b.Customer.LastName.Contains(normalizedQuery) ||
                b.Vehicle.PlateNumber.Contains(normalizedQuery) ||
                b.Vehicle.Brand.Contains(normalizedQuery) ||
                b.Vehicle.Model.Contains(normalizedQuery) ||
                b.PickupLocation.City.Contains(normalizedQuery) ||
                b.PlannedDropoffLocation.City.Contains(normalizedQuery));
        }

        return PartialView("_BookingRows", bookings.Take(50).ToList());
    }

    [HttpGet("detalji/{id:int}")]
    public IActionResult Details(int id)
    {
        var booking = _context.Bookings
            .IgnoreQueryFilters()
            .Include(b => b.Customer)
            .Include(b => b.Vehicle)
            .Include(b => b.PickupLocation)
            .Include(b => b.PlannedDropoffLocation)
            .Include(b => b.SupportTickets)
            .FirstOrDefault(b => b.Id == id);

        if (booking is null)
        {
            return NotFound();
        }

        if (booking.DeletedAt is not null)
        {
            return NotFound();
        }

        return View(booking);
    }

    [HttpPost("novi")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingCreateModel model)
    {
        await ValidateBookingModelAsync(model);

        if (!ModelState.IsValid)
        {
            PrepareLookups(model.CustomerId, model.VehicleId, model.PickupLocationId, model.PlannedDropoffLocationId, model.Status);
            return View(model);
        }

        var booking = new Booking
        {
            ReservationCode = model.ReservationCode.Trim(),
            CustomerId = model.CustomerId,
            VehicleId = model.VehicleId,
            PickupLocationId = model.PickupLocationId,
            PlannedDropoffLocationId = model.PlannedDropoffLocationId,
            PickupAt = model.PickupAt,
            PlannedDropoffAt = model.PlannedDropoffAt,
            ActualDropoffAt = model.ActualDropoffAt,
            TotalPrice = model.TotalPrice,
            Status = model.Status
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = booking.Id });
    }

    [HttpGet("uredi/{id:int}")]
    public IActionResult EditGet(int id)
    {
        var booking = _context.Bookings
            .IgnoreQueryFilters()
            .FirstOrDefault(b => b.Id == id);

        if (booking is null || booking.DeletedAt is not null)
        {
            return NotFound();
        }

        PrepareLookups(booking.CustomerId, booking.VehicleId, booking.PickupLocationId, booking.PlannedDropoffLocationId, booking.Status);

        return View(new BookingEditModel
        {
            Id = booking.Id,
            ReservationCode = booking.ReservationCode,
            CustomerId = booking.CustomerId,
            VehicleId = booking.VehicleId,
            PickupLocationId = booking.PickupLocationId,
            PlannedDropoffLocationId = booking.PlannedDropoffLocationId,
            PickupAt = booking.PickupAt,
            PlannedDropoffAt = booking.PlannedDropoffAt,
            ActualDropoffAt = booking.ActualDropoffAt,
            TotalPrice = booking.TotalPrice,
            Status = booking.Status
        });
    }

    [HttpPost("uredi/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BookingEditModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id);
        if (booking is null)
        {
            return NotFound();
        }

        await ValidateBookingModelAsync(model, id);

        if (!ModelState.IsValid)
        {
            PrepareLookups(model.CustomerId, model.VehicleId, model.PickupLocationId, model.PlannedDropoffLocationId, model.Status);
            return View(model);
        }

        booking.ReservationCode = model.ReservationCode.Trim();
        booking.CustomerId = model.CustomerId;
        booking.VehicleId = model.VehicleId;
        booking.PickupLocationId = model.PickupLocationId;
        booking.PlannedDropoffLocationId = model.PlannedDropoffLocationId;
        booking.PickupAt = model.PickupAt;
        booking.PlannedDropoffAt = model.PlannedDropoffAt;
        booking.ActualDropoffAt = model.ActualDropoffAt;
        booking.TotalPrice = model.TotalPrice;
        booking.Status = model.Status;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = booking.Id });
    }

    [HttpPost("obrisi/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var booking = await _context.Bookings
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking is null)
        {
            return NotFound();
        }

        if (booking.DeletedAt is not null)
        {
            return NotFound();
        }

        booking.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateBookingModelAsync(BookingFormModel model, int? bookingId = null)
    {
        if (string.IsNullOrWhiteSpace(model.ReservationCode))
        {
            ModelState.AddModelError(nameof(model.ReservationCode), "Reservation code is required.");
        }

        if (model.PickupAt >= model.PlannedDropoffAt)
        {
            ModelState.AddModelError(nameof(model.PlannedDropoffAt), "Planned dropoff must be after pickup.");
        }

        if (model.ActualDropoffAt.HasValue && model.ActualDropoffAt.Value < model.PickupAt)
        {
            ModelState.AddModelError(nameof(model.ActualDropoffAt), "Actual dropoff cannot be earlier than pickup.");
        }

        if (model.TotalPrice < 0)
        {
            ModelState.AddModelError(nameof(model.TotalPrice), "Total price cannot be negative.");
        }

        if (!Enum.IsDefined(typeof(BookingStatus), model.Status))
        {
            ModelState.AddModelError(nameof(model.Status), "Invalid booking status.");
        }

        var customerExists = await _context.Customers.AnyAsync(c => c.Id == model.CustomerId);
        if (!customerExists)
        {
            ModelState.AddModelError(nameof(model.CustomerId), "Selected customer does not exist.");
        }

        var vehicleExists = await _context.Vehicles.AnyAsync(v => v.Id == model.VehicleId);
        if (!vehicleExists)
        {
            ModelState.AddModelError(nameof(model.VehicleId), "Selected vehicle does not exist.");
        }

        var pickupLocationExists = await _context.Locations.AnyAsync(l => l.Id == model.PickupLocationId);
        if (!pickupLocationExists)
        {
            ModelState.AddModelError(nameof(model.PickupLocationId), "Selected pickup location does not exist.");
        }

        var dropoffLocationExists = await _context.Locations.AnyAsync(l => l.Id == model.PlannedDropoffLocationId);
        if (!dropoffLocationExists)
        {
            ModelState.AddModelError(nameof(model.PlannedDropoffLocationId), "Selected dropoff location does not exist.");
        }

        var duplicateCodeQuery = _context.Bookings.Where(b => b.ReservationCode == model.ReservationCode.Trim());
        if (bookingId.HasValue)
        {
            duplicateCodeQuery = duplicateCodeQuery.Where(b => b.Id != bookingId.Value);
        }

        if (await duplicateCodeQuery.AnyAsync())
        {
            ModelState.AddModelError(nameof(model.ReservationCode), "Reservation code must be unique.");
        }
    }

    private void PrepareLookups(int? customerId = null, int? vehicleId = null, int? pickupLocationId = null, int? plannedDropoffLocationId = null, BookingStatus? status = null)
    {
        ViewBag.Customers = new SelectList(
            _context.Customers
                .Where(c => c.DeletedAt == null)
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .Select(c => new { c.Id, Label = $"{c.FirstName} {c.LastName}" })
                .ToList(),
            "Id",
            "Label",
            customerId);

        ViewBag.Vehicles = new SelectList(
            _context.Vehicles
                .Where(v => v.DeletedAt == null)
                .OrderBy(v => v.Brand)
                .ThenBy(v => v.Model)
                .Select(v => new { v.Id, Label = $"{v.Brand} {v.Model} ({v.PlateNumber})" })
                .ToList(),
            "Id",
            "Label",
            vehicleId);

        ViewBag.Locations = new SelectList(
            _context.Locations
                .Where(l => l.DeletedAt == null)
                .OrderBy(l => l.City)
                .ThenBy(l => l.Name)
                .Select(l => new { l.Id, Label = $"{l.Name}, {l.City}" })
                .ToList(),
            "Id",
            "Label",
            pickupLocationId);

        ViewBag.DropoffLocations = new SelectList(
            _context.Locations
                .Where(l => l.DeletedAt == null)
                .OrderBy(l => l.City)
                .ThenBy(l => l.Name)
                .Select(l => new { l.Id, Label = $"{l.Name}, {l.City}" })
                .ToList(),
            "Id",
            "Label",
            plannedDropoffLocationId);

        ViewBag.BookingStatuses = new SelectList(
            Enum.GetValues<BookingStatus>()
                .Select(value => new { Id = value, Label = value.ToString() })
                .ToList(),
            "Id",
            "Label",
            status);
    }
}

