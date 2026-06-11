using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrviLabos.DAL;
using PrviLabos.Model;
using PrviLabos.Models;

namespace PrviLabos.Controllers.Api;

[ApiController]
[Route("api/locations")]
public sealed class LocationsApiController : ControllerBase
{
    private readonly PrviLabosDbContext _context;

    public LocationsApiController(PrviLabosDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LocationDto>>> GetAll([FromQuery] string? query)
    {
        var locations = _context.Locations.AsNoTracking().OrderBy(l => l.City).ThenBy(l => l.Name).AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var normalized = query.Trim();
            locations = locations.Where(l =>
                l.Name.Contains(normalized) ||
                l.City.Contains(normalized) ||
                l.Address.Contains(normalized) ||
                l.ContactPhone.Contains(normalized));
        }

        return Ok(await locations.Take(100).Select(l => l.ToDto()).ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LocationDto>> GetById(int id)
    {
        var location = await _context.Locations.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
        return location is null ? NotFound() : Ok(location.ToDto());
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<LocationDto>> Post(LocationUpsertDto model)
    {
        var location = new Location();
        Apply(model, location);

        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = location.Id }, location.ToDto());
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<LocationDto>> Put(int id, LocationUpsertDto model)
    {
        var location = await _context.Locations.FirstOrDefaultAsync(l => l.Id == id);
        if (location is null)
        {
            return NotFound();
        }

        Apply(model, location);
        await _context.SaveChangesAsync();

        return Ok(location.ToDto());
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var location = await _context.Locations.FirstOrDefaultAsync(l => l.Id == id);
        if (location is null)
        {
            return NotFound();
        }

        location.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static void Apply(LocationUpsertDto model, Location location)
    {
        location.Name = model.Name.Trim();
        location.City = model.City.Trim();
        location.Address = model.Address.Trim();
        location.ContactPhone = model.ContactPhone.Trim();
        location.OpenAt = model.OpenAt;
        location.CloseAt = model.CloseAt;
        location.ParkingCapacity = model.ParkingCapacity;
    }
}
