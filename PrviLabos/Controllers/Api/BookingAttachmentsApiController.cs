using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrviLabos.DAL;
using PrviLabos.Model;
using PrviLabos.Models;

namespace PrviLabos.Controllers.Api;

[ApiController]
[Route("api/booking-attachments")]
public sealed class BookingAttachmentsApiController : ControllerBase
{
    private readonly PrviLabosDbContext _context;

    public BookingAttachmentsApiController(PrviLabosDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookingAttachmentDto>>> GetAll(
        [FromQuery] string? query,
        [FromQuery] int? bookingId)
    {
        var attachments = IncludeDetails(_context.BookingAttachments.AsNoTracking())
            .OrderByDescending(a => a.CreatedAt)
            .AsQueryable();

        if (bookingId.HasValue)
        {
            attachments = attachments.Where(a => a.BookingId == bookingId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            var normalized = query.Trim();
            attachments = attachments.Where(a =>
                a.OriginalFileName.Contains(normalized) ||
                a.StoredFileName.Contains(normalized) ||
                a.FilePath.Contains(normalized) ||
                a.ContentType.Contains(normalized) ||
                a.Booking.ReservationCode.Contains(normalized));
        }

        return Ok(await attachments.Take(100).Select(a => a.ToDto()).ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookingAttachmentDto>> GetById(int id)
    {
        var attachment = await IncludeDetails(_context.BookingAttachments.AsNoTracking())
            .FirstOrDefaultAsync(a => a.Id == id);

        return attachment is null ? NotFound() : Ok(attachment.ToDto());
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<BookingAttachmentDto>> Post(BookingAttachmentUpsertDto model)
    {
        var validation = await ValidateBooking(model.BookingId);
        if (validation is not null)
        {
            return validation;
        }

        var attachment = new BookingAttachment
        {
            CreatedAt = DateTime.UtcNow
        };

        Apply(model, attachment);
        _context.BookingAttachments.Add(attachment);
        await _context.SaveChangesAsync();

        var created = await IncludeDetails(_context.BookingAttachments.AsNoTracking())
            .FirstAsync(a => a.Id == attachment.Id);

        return CreatedAtAction(nameof(GetById), new { id = attachment.Id }, created.ToDto());
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<BookingAttachmentDto>> Put(int id, BookingAttachmentUpsertDto model)
    {
        var validation = await ValidateBooking(model.BookingId);
        if (validation is not null)
        {
            return validation;
        }

        var attachment = await _context.BookingAttachments.FirstOrDefaultAsync(a => a.Id == id);
        if (attachment is null)
        {
            return NotFound();
        }

        Apply(model, attachment);
        await _context.SaveChangesAsync();

        var updated = await IncludeDetails(_context.BookingAttachments.AsNoTracking())
            .FirstAsync(a => a.Id == id);

        return Ok(updated.ToDto());
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var attachment = await _context.BookingAttachments.FirstOrDefaultAsync(a => a.Id == id);
        if (attachment is null)
        {
            return NotFound();
        }

        _context.BookingAttachments.Remove(attachment);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static IQueryable<BookingAttachment> IncludeDetails(IQueryable<BookingAttachment> attachments) =>
        attachments.Include(a => a.Booking);

    private async Task<ActionResult<BookingAttachmentDto>?> ValidateBooking(int bookingId)
    {
        if (!await _context.Bookings.AnyAsync(b => b.Id == bookingId))
        {
            return BadRequest("Booking does not exist.");
        }

        return null;
    }

    private static void Apply(BookingAttachmentUpsertDto model, BookingAttachment attachment)
    {
        attachment.BookingId = model.BookingId;
        attachment.OriginalFileName = model.OriginalFileName.Trim();
        attachment.StoredFileName = model.StoredFileName.Trim();
        attachment.FilePath = model.FilePath.Trim();
        attachment.ContentType = model.ContentType.Trim();
        attachment.FileSize = model.FileSize;
    }
}
