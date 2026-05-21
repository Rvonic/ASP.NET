using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrviLabos.DAL;
using PrviLabos.Model;
using PrviLabos.Models;

namespace PrviLabos.Controllers;

[Route("lokacije")]
public class LocationsController : Controller
{
    private readonly PrviLabosDbContext _context;

    public LocationsController(PrviLabosDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        var locations = _context.Locations
            .Where(l => l.DeletedAt == null)
            .Include(l => l.Vehicles)
            .OrderBy(l => l.City)
            .ThenBy(l => l.Name)
            .ToList();

        return View(locations);
    }

    [HttpGet("pretraga")]
    public IActionResult Search(string? query)
    {
        var normalizedQuery = query?.Trim();

        var locations = _context.Locations
            .Where(l => l.DeletedAt == null)
            .Include(l => l.Vehicles)
            .OrderBy(l => l.City)
            .ThenBy(l => l.Name)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(normalizedQuery))
        {
            locations = locations.Where(l =>
                l.Name.Contains(normalizedQuery) ||
                l.City.Contains(normalizedQuery) ||
                l.Address.Contains(normalizedQuery) ||
                l.ContactPhone.Contains(normalizedQuery));
        }

        return PartialView("_LocationRows", locations.Take(50).ToList());
    }

    [HttpGet("novi")]
    public IActionResult Create()
    {
        return View(new LocationCreateModel());
    }

    [HttpPost("novi")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LocationCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var location = new Location
        {
            Name = model.Name.Trim(),
            City = model.City.Trim(),
            Address = model.Address.Trim(),
            ContactPhone = PhoneCountryCatalog.ComposePhoneNumber(model.PhoneCountryCode, model.PhoneLocalNumber),
            OpenAt = model.OpenAt,
            CloseAt = model.CloseAt,
            ParkingCapacity = model.ParkingCapacity
        };

        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = location.Id });
    }

    [HttpGet("detalji/{id:int}")]
    public IActionResult Details(int id)
    {
        var location = _context.Locations
            .Include(l => l.Vehicles)
            .Include(l => l.SupportTickets)
            .FirstOrDefault(l => l.Id == id);

        if (location is null || location.DeletedAt is not null)
        {
            return NotFound();
        }

        return View(location);
    }

    [HttpGet("uredi/{id:int}")]
    [ActionName("Edit")]
    public IActionResult EditGet(int id)
    {
        var location = _context.Locations.FirstOrDefault(l => l.Id == id);
        if (location is null)
        {
            return NotFound();
        }

        if (location.DeletedAt is not null)
        {
            return NotFound();
        }

        return View(new LocationEditModel
        {
            Id = location.Id,
            Name = location.Name,
            City = location.City,
            Address = location.Address,
            PhoneCountryCode = PhoneCountryCatalog.TrySplitPhoneNumber(location.ContactPhone, out var dialCode, out var localNumber)
                ? dialCode
                : PhoneCountryCatalog.DefaultDialCode,
            PhoneLocalNumber = PhoneCountryCatalog.TrySplitPhoneNumber(location.ContactPhone, out _, out var existingLocalNumber)
                ? existingLocalNumber
                : string.Empty,
            OpenAt = location.OpenAt,
            CloseAt = location.CloseAt,
            ParkingCapacity = location.ParkingCapacity
        });
    }

    [HttpPost("uredi/{id:int}")]
    [ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost(int id, LocationEditModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var location = await _context.Locations.FirstOrDefaultAsync(l => l.Id == id);
        if (location is null)
        {
            return NotFound();
        }

        if (location.DeletedAt is not null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        location.Name = model.Name.Trim();
        location.City = model.City.Trim();
        location.Address = model.Address.Trim();
        location.ContactPhone = PhoneCountryCatalog.ComposePhoneNumber(model.PhoneCountryCode, model.PhoneLocalNumber);
        location.OpenAt = model.OpenAt;
        location.CloseAt = model.CloseAt;
        location.ParkingCapacity = model.ParkingCapacity;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = location.Id });
    }

    [HttpPost("obrisi/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var location = await _context.Locations.FirstOrDefaultAsync(l => l.Id == id);

        if (location is null || location.DeletedAt is not null)
        {
            return NotFound();
        }

        var hasVehicles = await _context.Vehicles.AnyAsync(v => v.CurrentLocationId == id);
        var hasBookings = await _context.Bookings.AnyAsync(b => b.PickupLocationId == id || b.PlannedDropoffLocationId == id);
        var hasTickets = await _context.Tickets.AnyAsync(t => t.RequestedDropoffLocationId == id);

        if (hasVehicles || hasBookings || hasTickets)
        {
            return Conflict("Location cannot be deleted while it is referenced by vehicles, bookings, or tickets.");
        }

        location.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
