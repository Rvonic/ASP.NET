using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrviLabos.DAL;
using PrviLabos.Model;
using PrviLabos.Models;

namespace PrviLabos.Controllers.Api;

[ApiController]
[Route("api/tickets")]
public sealed class TicketsApiController : ControllerBase
{
    private readonly PrviLabosDbContext _context;

    public TicketsApiController(PrviLabosDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupportTicketDto>>> GetAll([FromQuery] string? query)
    {
        var tickets = IncludeDetails(_context.Tickets.AsNoTracking())
            .OrderByDescending(t => t.CreatedAt)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var normalized = query.Trim();
            tickets = tickets.Where(t =>
                t.TicketNumber.Contains(normalized) ||
                t.Description.Contains(normalized) ||
                t.Booking.ReservationCode.Contains(normalized) ||
                t.RequestedDropoffLocation.City.Contains(normalized));
        }

        return Ok(await tickets.Take(100).Select(t => t.ToDto()).ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SupportTicketDto>> GetById(int id)
    {
        var ticket = await IncludeDetails(_context.Tickets.AsNoTracking()).FirstOrDefaultAsync(t => t.Id == id);
        return ticket is null ? NotFound() : Ok(ticket.ToDto());
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<SupportTicketDto>> Post(SupportTicketUpsertDto model)
    {
        var validation = await ValidateRelations(model);
        if (validation is not null)
        {
            return validation;
        }

        var ticket = new SupportTicket
        {
            CreatedAt = DateTime.UtcNow
        };

        await Apply(model, ticket);
        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();

        var created = await IncludeDetails(_context.Tickets.AsNoTracking()).FirstAsync(t => t.Id == ticket.Id);
        return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, created.ToDto());
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<SupportTicketDto>> Put(int id, SupportTicketUpsertDto model)
    {
        var validation = await ValidateRelations(model);
        if (validation is not null)
        {
            return validation;
        }

        var ticket = await IncludeDetails(_context.Tickets).FirstOrDefaultAsync(t => t.Id == id);
        if (ticket is null)
        {
            return NotFound();
        }

        await Apply(model, ticket);
        await _context.SaveChangesAsync();

        var updated = await IncludeDetails(_context.Tickets.AsNoTracking()).FirstAsync(t => t.Id == id);
        return Ok(updated.ToDto());
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
        if (ticket is null)
        {
            return NotFound();
        }

        ticket.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static IQueryable<SupportTicket> IncludeDetails(IQueryable<SupportTicket> tickets) =>
        tickets
            .Include(t => t.Booking)
            .Include(t => t.RequestedDropoffLocation)
            .Include(t => t.AssignedAgents);

    private async Task<ActionResult<SupportTicketDto>?> ValidateRelations(SupportTicketUpsertDto model)
    {
        if (!await _context.Bookings.AnyAsync(b => b.Id == model.BookingId))
        {
            return BadRequest("Booking does not exist.");
        }

        if (!await _context.Locations.AnyAsync(l => l.Id == model.RequestedDropoffLocationId))
        {
            return BadRequest("Requested dropoff location does not exist.");
        }

        var distinctAgentIds = model.AssignedAgentIds.Distinct().ToList();
        var existingAgentCount = await _context.Agents.CountAsync(a => distinctAgentIds.Contains(a.Id));
        if (existingAgentCount != distinctAgentIds.Count)
        {
            return BadRequest("One or more assigned agents do not exist.");
        }

        return null;
    }

    private async Task Apply(SupportTicketUpsertDto model, SupportTicket ticket)
    {
        ticket.TicketNumber = model.TicketNumber.Trim();
        ticket.BookingId = model.BookingId;
        ticket.RequestedDropoffLocationId = model.RequestedDropoffLocationId;
        ticket.Description = model.Description.Trim();
        ticket.Priority = model.Priority;
        ticket.Status = model.Status;
        ticket.ResolvedAt = model.ResolvedAt;

        ticket.AssignedAgents.Clear();
        var agentIds = model.AssignedAgentIds.Distinct().ToList();
        if (agentIds.Count == 0)
        {
            return;
        }

        var agents = await _context.Agents.Where(a => agentIds.Contains(a.Id)).ToListAsync();
        foreach (var agent in agents)
        {
            ticket.AssignedAgents.Add(agent);
        }
    }
}
