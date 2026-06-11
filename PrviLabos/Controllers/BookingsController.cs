using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PrviLabos.DAL;
using PrviLabos.Model;
using PrviLabos.Models;
using PrviLabos.Services.Validation;

namespace PrviLabos.Controllers;

[Route("rezervacije")]
[Authorize]
public class BookingsController : Controller
{
    private readonly PrviLabosDbContext _context;
    private readonly BookingFormValidator _validator;

    public BookingsController(PrviLabosDbContext context, BookingFormValidator validator)
    {
        _context = context;
        _validator = validator;
    }

    [HttpGet("")]
    [AllowAnonymous]
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
    [Authorize(Roles = "Admin,Manager")]
    public IActionResult CreateGet()
    {
        PrepareLookups();
        PopulateAutocompleteSelections(null, null, null, null, BookingStatus.Reserved);

        return View(new BookingCreateModel
        {
            PickupAt = DateTime.Now,
            PlannedDropoffAt = DateTime.Now.AddHours(1),
            Status = BookingStatus.Reserved
        });
    }

    [HttpGet("pretraga")]
    [AllowAnonymous]
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

    [HttpGet("autocomplete/kupci")]
    public IActionResult AutocompleteCustomers(string? query)
    {
        var normalized = query?.Trim();
        var customers = _context.Customers
            .AsNoTracking()
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(normalized))
        {
            customers = customers.Where(c =>
                c.FirstName.Contains(normalized) ||
                c.LastName.Contains(normalized) ||
                c.Email.Contains(normalized));
        }

        var result = customers
            .Take(15)
            .Select(c => new
            {
                id = c.Id,
                text = c.FirstName + " " + c.LastName + " (" + c.Email + ")"
            })
            .ToList();

        return Json(result);
    }

    [HttpGet("autocomplete/vozila")]
    public IActionResult AutocompleteVehicles(string? query)
    {
        var normalized = query?.Trim();
        var vehicles = _context.Vehicles
            .AsNoTracking()
            .OrderBy(v => v.Brand)
            .ThenBy(v => v.Model)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(normalized))
        {
            vehicles = vehicles.Where(v =>
                v.Brand.Contains(normalized) ||
                v.Model.Contains(normalized) ||
                v.PlateNumber.Contains(normalized));
        }

        var result = vehicles
            .Take(15)
            .Select(v => new
            {
                id = v.Id,
                text = v.Brand + " " + v.Model + " (" + v.PlateNumber + ")"
            })
            .ToList();

        return Json(result);
    }

    [HttpGet("autocomplete/lokacije")]
    public IActionResult AutocompleteLocations(string? query)
    {
        var normalized = query?.Trim();
        var locations = _context.Locations
            .AsNoTracking()
            .OrderBy(l => l.City)
            .ThenBy(l => l.Name)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(normalized))
        {
            locations = locations.Where(l =>
                l.Name.Contains(normalized) ||
                l.City.Contains(normalized) ||
                l.Address.Contains(normalized));
        }

        var result = locations
            .Take(15)
            .Select(l => new
            {
                id = l.Id,
                text = l.Name + ", " + l.City
            })
            .ToList();

        return Json(result);
    }

    [HttpGet("autocomplete/statusi")]
    public IActionResult AutocompleteStatuses(string? query)
    {
        var normalized = query?.Trim();
        var statuses = Enum.GetValues<BookingStatus>()
            .Select(value => new { id = (int)value, text = value.ToString() });

        if (!string.IsNullOrWhiteSpace(normalized))
        {
            statuses = statuses.Where(s => s.text.Contains(normalized, StringComparison.OrdinalIgnoreCase));
        }

        return Json(statuses.Take(20).ToList());
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
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create(BookingCreateModel model)
    {
        await _validator.ValidateAsync(model, ModelState);

        if (!ModelState.IsValid)
        {
            PrepareLookups(model.CustomerId, model.VehicleId, model.PickupLocationId, model.PlannedDropoffLocationId, model.Status);
            PopulateAutocompleteSelections(model.CustomerId, model.VehicleId, model.PickupLocationId, model.PlannedDropoffLocationId, model.Status);
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
    [ActionName("Edit")]
    [Authorize(Roles = "Admin,Manager")]
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
        PopulateAutocompleteSelections(booking.CustomerId, booking.VehicleId, booking.PickupLocationId, booking.PlannedDropoffLocationId, booking.Status);

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
            ActualDropoffAt = booking.ActualDropoffAt ?? DateTime.Now,
            TotalPrice = booking.TotalPrice,
            Status = booking.Status
        });
    }

    [HttpPost("uredi/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Manager")]
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

        await _validator.ValidateAsync(model, ModelState, id);

        if (!ModelState.IsValid)
        {
            PrepareLookups(model.CustomerId, model.VehicleId, model.PickupLocationId, model.PlannedDropoffLocationId, model.Status);
            PopulateAutocompleteSelections(model.CustomerId, model.VehicleId, model.PickupLocationId, model.PlannedDropoffLocationId, model.Status);
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
    [Authorize(Roles = "Admin")]
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
        TempData["StatusMessage"] = "Booking was deleted successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{bookingId:int}/datoteke")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UploadAttachment(int bookingId, IFormFile file)
    {
        var bookingExists = await _context.Bookings.AnyAsync(b => b.Id == bookingId);
        if (!bookingExists)
        {
            return NotFound();
        }

        if (file.Length == 0)
        {
            return BadRequest("File is empty.");
        }

        var uploadsPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "uploads",
            "bookings",
            bookingId.ToString());

        Directory.CreateDirectory(uploadsPath);

        var storedFileName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
        var physicalPath = Path.Combine(uploadsPath, storedFileName);

        await using (var stream = System.IO.File.Create(physicalPath))
        {
            await file.CopyToAsync(stream);
        }

        var attachment = new BookingAttachment
        {
            BookingId = bookingId,
            OriginalFileName = Path.GetFileName(file.FileName),
            StoredFileName = storedFileName,
            FilePath = $"/uploads/bookings/{bookingId}/{storedFileName}",
            ContentType = file.ContentType,
            FileSize = file.Length,
            CreatedAt = DateTime.UtcNow
        };

        _context.BookingAttachments.Add(attachment);
        await _context.SaveChangesAsync();

        return Ok(new { attachment.Id });
    }

    [HttpGet("{bookingId:int}/datoteke")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetAttachments(int bookingId)
    {
        var bookingExists = await _context.Bookings.AnyAsync(b => b.Id == bookingId);
        if (!bookingExists)
        {
            return NotFound();
        }

        var attachments = await _context.BookingAttachments
            .AsNoTracking()
            .Where(a => a.BookingId == bookingId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return PartialView("_BookingAttachmentList", attachments);
    }

    [HttpPost("{bookingId:int}/datoteke/{attachmentId:int}/obrisi")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteAttachment(int bookingId, int attachmentId)
    {
        var attachment = await _context.BookingAttachments
            .FirstOrDefaultAsync(a => a.Id == attachmentId && a.BookingId == bookingId);

        if (attachment is null)
        {
            return NotFound();
        }

        var physicalPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            attachment.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

        if (System.IO.File.Exists(physicalPath))
        {
            System.IO.File.Delete(physicalPath);
        }

        _context.BookingAttachments.Remove(attachment);
        await _context.SaveChangesAsync();

        return Ok();
    }

    private void PrepareLookups(int? customerId = null, int? vehicleId = null, int? pickupLocationId = null, int? plannedDropoffLocationId = null, BookingStatus? status = null)
    {
        ViewBag.BookingStatuses = new SelectList(
            Enum.GetValues<BookingStatus>()
                .Select(value => new { Id = value, Label = value.ToString() })
                .ToList(),
            "Id",
            "Label",
            status);
    }

    private void PopulateAutocompleteSelections(int? customerId, int? vehicleId, int? pickupLocationId, int? plannedDropoffLocationId, BookingStatus? status)
    {
        ViewBag.CustomerDisplay = customerId.HasValue
            ? _context.Customers
                .IgnoreQueryFilters()
                .Where(c => c.Id == customerId.Value)
                .Select(c => c.FirstName + " " + c.LastName + " (" + c.Email + ")")
                .FirstOrDefault()
            : null;

        ViewBag.VehicleDisplay = vehicleId.HasValue
            ? _context.Vehicles
                .IgnoreQueryFilters()
                .Where(v => v.Id == vehicleId.Value)
                .Select(v => v.Brand + " " + v.Model + " (" + v.PlateNumber + ")")
                .FirstOrDefault()
            : null;

        ViewBag.PickupLocationDisplay = pickupLocationId.HasValue
            ? _context.Locations
                .IgnoreQueryFilters()
                .Where(l => l.Id == pickupLocationId.Value)
                .Select(l => l.Name + ", " + l.City)
                .FirstOrDefault()
            : null;

        ViewBag.DropoffLocationDisplay = plannedDropoffLocationId.HasValue
            ? _context.Locations
                .IgnoreQueryFilters()
                .Where(l => l.Id == plannedDropoffLocationId.Value)
                .Select(l => l.Name + ", " + l.City)
                .FirstOrDefault()
            : null;

        ViewBag.BookingStatusDisplay = status?.ToString();
    }
}

