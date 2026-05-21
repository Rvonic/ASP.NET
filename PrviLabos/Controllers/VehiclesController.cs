using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PrviLabos.DAL;
using PrviLabos.Model;
using PrviLabos.Models;

namespace PrviLabos.Controllers;

[Route("vozila")]
public class VehiclesController : Controller
{
    private readonly PrviLabosDbContext _context;

    public VehiclesController(PrviLabosDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
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
    public IActionResult Create()
    {
        PrepareLookups();
        PopulateAutocompleteSelections(null, VehicleCategory.Economy);
        return View(new VehicleCreateModel
        {
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
    public async Task<IActionResult> Create(VehicleCreateModel vehicleModel)
    {
        await ValidateVehicleModelAsync(vehicleModel);

        if (!ModelState.IsValid)
        {
            PrepareLookups(vehicleModel.CurrentLocationId, vehicleModel.Category);
            PopulateAutocompleteSelections(vehicleModel.CurrentLocationId, vehicleModel.Category);
            return View(vehicleModel);
        }

        var vehicle = new Vehicle
        {
            PlateNumber = vehicleModel.PlateNumber.Trim(),
            Brand = vehicleModel.Brand.Trim(),
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
    public IActionResult EditGet(int id)
    {
        var vehicle = _context.Vehicles.FirstOrDefault(v => v.Id == id);
        if (vehicle is null)
        {
            return NotFound();
        }

        PrepareLookups(vehicle.CurrentLocationId, vehicle.Category);
        PopulateAutocompleteSelections(vehicle.CurrentLocationId, vehicle.Category);
        return View(new VehicleEditModel
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
            CurrentLocationId = vehicle.CurrentLocationId
        });
    }

    [HttpPost("uredi/{id:int}")]
    [ActionName("Edit")]
    [ValidateAntiForgeryToken]
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

        await ValidateVehicleModelAsync(vehicleModel, id);

        if (!ModelState.IsValid)
        {
            PrepareLookups(vehicleModel.CurrentLocationId, vehicleModel.Category);
            PopulateAutocompleteSelections(vehicleModel.CurrentLocationId, vehicleModel.Category);
            return View(vehicleModel);
        }

        vehicle.PlateNumber = vehicleModel.PlateNumber.Trim();
        vehicle.Brand = vehicleModel.Brand.Trim();
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
            return Conflict("Vehicle cannot be deleted while it is linked to bookings.");
        }

        vehicle.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private void PrepareLookups(int? selectedLocationId = null, VehicleCategory? selectedCategory = null)
    {
        ViewBag.VehicleCategories = new SelectList(
            Enum.GetValues<VehicleCategory>()
                .Select(category => new { Id = category, Label = category.ToString() })
                .ToList(),
            "Id",
            "Label",
            selectedCategory);
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

    private async Task ValidateVehicleModelAsync(VehicleCreateModel model, int? vehicleId = null)
    {
        await ValidateVehicleModelCoreAsync(model.PlateNumber, model.CurrentLocationId, vehicleId);
    }

    private async Task ValidateVehicleModelAsync(VehicleEditModel model, int? vehicleId = null)
    {
        await ValidateVehicleModelCoreAsync(model.PlateNumber, model.CurrentLocationId, vehicleId);
    }

    private async Task ValidateVehicleModelCoreAsync(string plateNumber, int? currentLocationId, int? vehicleId)
    {
        if (string.IsNullOrWhiteSpace(plateNumber))
        {
            ModelState.AddModelError(nameof(plateNumber), "Plate number is required.");
        }

        if (!string.IsNullOrWhiteSpace(plateNumber))
        {
            var duplicatePlateQuery = _context.Vehicles.Where(v => v.PlateNumber == plateNumber.Trim());
            if (vehicleId.HasValue)
            {
                duplicatePlateQuery = duplicatePlateQuery.Where(v => v.Id != vehicleId.Value);
            }

            if (await duplicatePlateQuery.AnyAsync())
            {
                ModelState.AddModelError(nameof(plateNumber), "Plate number must be unique.");
            }
        }

        if (currentLocationId.HasValue)
        {
            var locationExists = await _context.Locations.AnyAsync(l => l.Id == currentLocationId.Value);
            if (!locationExists)
            {
                ModelState.AddModelError(nameof(currentLocationId), "Selected location does not exist.");
            }
        }
    }
}
