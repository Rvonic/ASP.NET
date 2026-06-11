using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrviLabos.DAL;
using PrviLabos.Model;
using PrviLabos.Models;

namespace PrviLabos.Controllers.Api;

[ApiController]
[Route("api/bookings")]
public sealed class BookingsApiController : ControllerBase
{
    private readonly PrviLabosDbContext _context;

    public BookingsApiController(PrviLabosDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookingDto>>> GetAll([FromQuery] string? query)
    {
        var bookings = IncludeDetails(_context.Bookings.AsNoTracking())
            .OrderByDescending(b => b.PickupAt)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var normalized = query.Trim();
            bookings = bookings.Where(b =>
                b.ReservationCode.Contains(normalized) ||
                b.Customer.FirstName.Contains(normalized) ||
                b.Customer.LastName.Contains(normalized) ||
                b.Vehicle.PlateNumber.Contains(normalized) ||
                b.Vehicle.Brand.Contains(normalized) ||
                b.Vehicle.Model.Contains(normalized) ||
                b.PickupLocation.City.Contains(normalized) ||
                b.PlannedDropoffLocation.City.Contains(normalized));
        }

        return Ok(await bookings.Take(100).Select(b => b.ToDto()).ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookingDto>> GetById(int id)
    {
        var booking = await IncludeDetails(_context.Bookings.AsNoTracking()).FirstOrDefaultAsync(b => b.Id == id);
        return booking is null ? NotFound() : Ok(booking.ToDto());
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<BookingDto>> Post(BookingUpsertDto model)
    {
        var validation = await ValidateRelations(model);
        if (validation is not null)
        {
            return validation;
        }

        var booking = new Booking();
        Apply(model, booking);

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        var created = await IncludeDetails(_context.Bookings.AsNoTracking()).FirstAsync(b => b.Id == booking.Id);
        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, created.ToDto());
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<BookingDto>> Put(int id, BookingUpsertDto model)
    {
        var validation = await ValidateRelations(model);
        if (validation is not null)
        {
            return validation;
        }

        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id);
        if (booking is null)
        {
            return NotFound();
        }

        Apply(model, booking);
        await _context.SaveChangesAsync();

        var updated = await IncludeDetails(_context.Bookings.AsNoTracking()).FirstAsync(b => b.Id == id);
        return Ok(updated.ToDto());
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id);
        if (booking is null)
        {
            return NotFound();
        }

        booking.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static IQueryable<Booking> IncludeDetails(IQueryable<Booking> bookings) =>
        bookings
            .Include(b => b.Customer)
            .Include(b => b.Vehicle)
            .Include(b => b.PickupLocation)
            .Include(b => b.PlannedDropoffLocation)
            .Include(b => b.Attachments);

    private async Task<ActionResult<BookingDto>?> ValidateRelations(BookingUpsertDto model)
    {
        if (!await _context.Customers.AnyAsync(c => c.Id == model.CustomerId))
        {
            return BadRequest("Customer does not exist.");
        }

        if (!await _context.Vehicles.AnyAsync(v => v.Id == model.VehicleId))
        {
            return BadRequest("Vehicle does not exist.");
        }

        if (!await _context.Locations.AnyAsync(l => l.Id == model.PickupLocationId))
        {
            return BadRequest("Pickup location does not exist.");
        }

        if (!await _context.Locations.AnyAsync(l => l.Id == model.PlannedDropoffLocationId))
        {
            return BadRequest("Planned dropoff location does not exist.");
        }

        if (model.PlannedDropoffAt <= model.PickupAt)
        {
            return BadRequest("Planned dropoff must be after pickup.");
        }

        return null;
    }

    private static void Apply(BookingUpsertDto model, Booking booking)
    {
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
    }
}
