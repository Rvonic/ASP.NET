using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrviLabos.DAL;
using PrviLabos.Model;
using PrviLabos.Models;

namespace PrviLabos.Controllers.Api;

[ApiController]
[Route("api/vehicles")]
public sealed class VehiclesApiController : ControllerBase
{
    private readonly PrviLabosDbContext _context;

    public VehiclesApiController(PrviLabosDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VehicleDto>>> GetAll([FromQuery] string? query)
    {
        var vehicles = _context.Vehicles
            .AsNoTracking()
            .Include(v => v.CurrentLocation)
            .OrderBy(v => v.Brand)
            .ThenBy(v => v.Model)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var normalized = query.Trim();
            vehicles = vehicles.Where(v =>
                v.PlateNumber.Contains(normalized) ||
                v.Brand.Contains(normalized) ||
                v.Model.Contains(normalized) ||
                (v.CurrentLocation != null &&
                    (v.CurrentLocation.Name.Contains(normalized) || v.CurrentLocation.City.Contains(normalized))));
        }

        return Ok(await vehicles.Take(100).Select(v => v.ToDto()).ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VehicleDto>> GetById(int id)
    {
        var vehicle = await _context.Vehicles
            .AsNoTracking()
            .Include(v => v.CurrentLocation)
            .FirstOrDefaultAsync(v => v.Id == id);

        return vehicle is null ? NotFound() : Ok(vehicle.ToDto());
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<VehicleDto>> Post(VehicleUpsertDto model)
    {
        if (!await LocationExists(model.CurrentLocationId))
        {
            return BadRequest("Current location does not exist.");
        }

        var vehicle = new Vehicle();
        Apply(model, vehicle);

        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();
        await _context.Entry(vehicle).Reference(v => v.CurrentLocation).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = vehicle.Id }, vehicle.ToDto());
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<VehicleDto>> Put(int id, VehicleUpsertDto model)
    {
        if (!await LocationExists(model.CurrentLocationId))
        {
            return BadRequest("Current location does not exist.");
        }

        var vehicle = await _context.Vehicles.Include(v => v.CurrentLocation).FirstOrDefaultAsync(v => v.Id == id);
        if (vehicle is null)
        {
            return NotFound();
        }

        Apply(model, vehicle);
        await _context.SaveChangesAsync();
        await _context.Entry(vehicle).Reference(v => v.CurrentLocation).LoadAsync();

        return Ok(vehicle.ToDto());
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id);
        if (vehicle is null)
        {
            return NotFound();
        }

        if (await _context.Bookings.AnyAsync(b => b.VehicleId == id))
        {
            return Conflict("Vehicle cannot be deleted while it is linked to bookings.");
        }

        vehicle.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> LocationExists(int? locationId) =>
        locationId is null || await _context.Locations.AnyAsync(l => l.Id == locationId.Value);

    private static void Apply(VehicleUpsertDto model, Vehicle vehicle)
    {
        vehicle.PlateNumber = model.PlateNumber.Trim();
        vehicle.Brand = model.Brand.Trim();
        vehicle.Model = model.Model.Trim();
        vehicle.ProductionYear = model.ProductionYear;
        vehicle.Category = model.Category;
        vehicle.DailyRate = model.DailyRate;
        vehicle.CurrentMileage = model.CurrentMileage;
        vehicle.IsAvailable = model.IsAvailable;
        vehicle.CurrentLocationId = model.CurrentLocationId;
    }
}
