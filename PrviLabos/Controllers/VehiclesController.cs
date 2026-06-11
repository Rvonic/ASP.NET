using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PrviLabos.DAL;
using PrviLabos.Model;
using PrviLabos.Models;
using PrviLabos.Services.Validation;

namespace PrviLabos.Controllers;

[Route("vozila")]
[Authorize]
public class VehiclesController : Controller
{
    private readonly PrviLabosDbContext _context;
    private readonly VehicleFormValidator _validator;

    public VehiclesController(PrviLabosDbContext context, VehicleFormValidator validator)
    {
        _context = context;
        _validator = validator;
    }

    [HttpGet("")]
    [AllowAnonymous]
    public IActionResult Index()
    {
        var vehicles = _context.Vehicles
            .Where(v => v.DeletedAt == null)
            .Include(v => v.CurrentLocation)
            .OrderBy(v => v.IsAvailable ? 0 : 1)
            .ThenBy(v => v.Brand)
            .ThenBy(v => v.Model)
            .ToList();

        return View(vehicles);
    }

    [HttpGet("pretraga")]
    [AllowAnonymous]
    public IActionResult Search(string? query)
    {
        var normalizedQuery = query?.Trim();

        var vehicles = _context.Vehicles
            .Where(v => v.DeletedAt == null)
            .Include(v => v.CurrentLocation)
            .OrderBy(v => v.IsAvailable ? 0 : 1)
            .ThenBy(v => v.Brand)
            .ThenBy(v => v.Model)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(normalizedQuery))
        {
            vehicles = vehicles.Where(v =>
                v.PlateNumber.Contains(normalizedQuery) ||
                v.Brand.Contains(normalizedQuery) ||
                v.Model.Contains(normalizedQuery) ||
                (v.CurrentLocation != null && (
                    v.CurrentLocation.Name.Contains(normalizedQuery) ||
                    v.CurrentLocation.City.Contains(normalizedQuery))));
        }

        return PartialView("_VehicleRows", vehicles.Take(50).ToList());
    }

    [HttpGet("novi")]
    [Authorize(Roles = "Admin,Manager")]
    public IActionResult Create()
    {
        PrepareLookups();
        PopulateAutocompleteSelections(null, VehicleCategory.Economy);
        return View(new VehicleCreateModel
        {
            Brand = VehicleBrand.Skoda,
            Category = VehicleCategory.Economy,
            IsAvailable = true
        });
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

    [HttpGet("autocomplete/kategorije")]
    public IActionResult AutocompleteCategories(string? query)
    {
        var normalized = query?.Trim();
        var categories = Enum.GetValues<VehicleCategory>()
            .Select(value => new { id = (int)value, text = value.ToString() });

        if (!string.IsNullOrWhiteSpace(normalized))
        {
            categories = categories.Where(c => c.text.Contains(normalized, StringComparison.OrdinalIgnoreCase));
        }

        return Json(categories.Take(20).ToList());
    }

    [HttpPost("novi")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create(VehicleCreateModel vehicleModel)
    {
        await _validator.ValidateAsync(vehicleModel, ModelState);

        if (!ModelState.IsValid)
        {
            PrepareLookups(vehicleModel.CurrentLocationId, vehicleModel.Category, vehicleModel.Brand);
            PopulateAutocompleteSelections(vehicleModel.CurrentLocationId, vehicleModel.Category);
            return View(vehicleModel);
        }

        var vehicle = new Vehicle
        {
            PlateNumber = vehicleModel.PlateNumber.Trim(),
            Brand = vehicleModel.Brand.ToString(),
            Model = vehicleModel.Model.Trim(),
            ProductionYear = vehicleModel.ProductionYear,
            Category = vehicleModel.Category,
            DailyRate = vehicleModel.DailyRate,
            CurrentMileage = vehicleModel.CurrentMileage,
            IsAvailable = vehicleModel.IsAvailable,
            CurrentLocationId = vehicleModel.CurrentLocationId
        };

        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = vehicle.Id });
    }

    [HttpGet("detalji/{id:int}")]
    public IActionResult Details(int id)
    {
        var vehicle = _context.Vehicles
            .Include(v => v.CurrentLocation)
            .Include(v => v.Bookings)
            .ThenInclude(b => b.Customer)
            .FirstOrDefault(v => v.Id == id);

        if (vehicle is null)
        {
            return NotFound();
        }

        return View(vehicle);
    }

    [HttpGet("uredi/{id:int}")]
    [ActionName("Edit")]
    [Authorize(Roles = "Admin,Manager")]
    public IActionResult EditGet(int id)
    {
        var vehicle = _context.Vehicles.FirstOrDefault(v => v.Id == id);
        if (vehicle is null)
        {
            return NotFound();
        }

        var vehicleBrand = ParseVehicleBrand(vehicle.Brand);
        PrepareLookups(vehicle.CurrentLocationId, vehicle.Category, vehicleBrand);
        PopulateAutocompleteSelections(vehicle.CurrentLocationId, vehicle.Category);
        return View(new VehicleEditModel
        {
            Id = vehicle.Id,
            PlateNumber = vehicle.PlateNumber,
            Brand = vehicleBrand,
            Model = vehicle.Model,
            ProductionYear = vehicle.ProductionYear,
            Category = vehicle.Category,
            DailyRate = vehicle.DailyRate,
            CurrentMileage = vehicle.CurrentMileage,
            IsAvailable = vehicle.IsAvailable,
            CurrentLocationId = vehicle.CurrentLocationId
        });
    }

    [HttpPost("uredi/{id:int}")]
    [ActionName("Edit")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> EditPost(int id, VehicleEditModel vehicleModel)
    {
        if (id != vehicleModel.Id)
        {
            return BadRequest();
        }

        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id);
        if (vehicle is null)
        {
            return NotFound();
        }

        await _validator.ValidateAsync(vehicleModel, ModelState, id);

        if (!ModelState.IsValid)
        {
            PrepareLookups(vehicleModel.CurrentLocationId, vehicleModel.Category, vehicleModel.Brand);
            PopulateAutocompleteSelections(vehicleModel.CurrentLocationId, vehicleModel.Category);
            return View(vehicleModel);
        }

        vehicle.PlateNumber = vehicleModel.PlateNumber.Trim();
        vehicle.Brand = vehicleModel.Brand.ToString();
        vehicle.Model = vehicleModel.Model.Trim();
        vehicle.ProductionYear = vehicleModel.ProductionYear;
        vehicle.Category = vehicleModel.Category;
        vehicle.DailyRate = vehicleModel.DailyRate;
        vehicle.CurrentMileage = vehicleModel.CurrentMileage;
        vehicle.IsAvailable = vehicleModel.IsAvailable;
        vehicle.CurrentLocationId = vehicleModel.CurrentLocationId;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = vehicle.Id });
    }

    [HttpPost("obrisi/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id);

        if (vehicle is null)
        {
            return NotFound();
        }

        var hasBookings = await _context.Bookings.AnyAsync(b => b.VehicleId == id);
        if (hasBookings)
        {
            TempData["DeleteError"] = "Vehicle cannot be deleted while it is linked to bookings.";
            return RedirectToAction(nameof(Details), new { id });
        }

        vehicle.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Vehicle was deleted successfully.";

        return RedirectToAction(nameof(Index));
    }

    private void PrepareLookups(int? selectedLocationId = null, VehicleCategory? selectedCategory = null, VehicleBrand? selectedBrand = null)
    {
        ViewBag.VehicleCategories = new SelectList(
            Enum.GetValues<VehicleCategory>()
                .Select(category => new { Id = category, Label = category.ToString() })
                .ToList(),
            "Id",
            "Label",
            selectedCategory);

        ViewBag.VehicleBrands = new SelectList(
            Enum.GetValues<VehicleBrand>()
                .Select(brand => new { Id = brand, Label = brand.ToString() })
                .ToList(),
            "Id",
            "Label",
            selectedBrand);
    }

    private void PopulateAutocompleteSelections(int? currentLocationId, VehicleCategory? category)
    {
        ViewBag.CurrentLocationDisplay = currentLocationId.HasValue
            ? _context.Locations
                .IgnoreQueryFilters()
                .Where(l => l.Id == currentLocationId.Value)
                .Select(l => l.Name + ", " + l.City)
                .FirstOrDefault()
            : null;

        ViewBag.CategoryDisplay = category?.ToString();
    }

    private static VehicleBrand ParseVehicleBrand(string brand)
    {
        return Enum.TryParse<VehicleBrand>(brand, ignoreCase: true, out var parsed)
            ? parsed
            : VehicleBrand.Skoda;
    }
}
