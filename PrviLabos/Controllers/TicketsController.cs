using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PrviLabos.DAL;
using PrviLabos.Model;
using PrviLabos.Models;

namespace PrviLabos.Controllers;

[Route("prijave")]
public class TicketsController : Controller
{
    private readonly PrviLabosDbContext _context;

    public TicketsController(PrviLabosDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        var tickets = _context.Tickets
            .AsNoTracking()
            .Include(t => t.Booking)
                .ThenInclude(b => b.Customer)
            .Include(t => t.Booking)
                .ThenInclude(b => b.Vehicle)
            .Include(t => t.Booking)
                .ThenInclude(b => b.PickupLocation)
            .Include(t => t.Booking)
                .ThenInclude(b => b.PlannedDropoffLocation)
            .OrderByDescending(t => t.CreatedAt)
            .ToList();

        return View(tickets);
    }

    [HttpGet("pretraga")]
    public IActionResult Search(string? query)
    {
        var normalizedQuery = query?.Trim();

        var tickets = _context.Tickets
            .AsNoTracking()
            .Include(t => t.Booking)
                .ThenInclude(b => b.Customer)
            .Include(t => t.Booking)
                .ThenInclude(b => b.Vehicle)
            .Include(t => t.Booking)
                .ThenInclude(b => b.PickupLocation)
            .Include(t => t.Booking)
                .ThenInclude(b => b.PlannedDropoffLocation)
            .OrderByDescending(t => t.CreatedAt)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(normalizedQuery))
        {
            tickets = tickets.Where(t =>
                t.TicketNumber.Contains(normalizedQuery) ||
                t.Description.Contains(normalizedQuery) ||
                t.Booking.ReservationCode.Contains(normalizedQuery) ||
                t.Booking.Customer.FirstName.Contains(normalizedQuery) ||
                t.Booking.Customer.LastName.Contains(normalizedQuery) ||
                t.Booking.Vehicle.PlateNumber.Contains(normalizedQuery) ||
                t.RequestedDropoffLocation.City.Contains(normalizedQuery));
        }

        return PartialView("_TicketRows", tickets.Take(50).ToList());
    }

    [HttpGet("novi")]
    [ActionName("Create")]
    public IActionResult CreateGet()
    {
        PrepareLookups();

        return View(new TicketCreateModel
        {
            Priority = TicketPriority.Medium,
            Status = TicketStatus.Open
        });
    }

    [HttpPost("novi")]
    [ActionName("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePost(TicketCreateModel model)
    {
        ValidateTicketModel(model);

        if (!ModelState.IsValid)
        {
            PrepareLookups(model.BookingId, model.RequestedDropoffLocationId, model.Priority, model.Status, model.AssignedAgentIds);
            return View("Create", model);
        }

        var ticket = new SupportTicket
        {
            TicketNumber = model.TicketNumber.Trim(),
            BookingId = model.BookingId,
            RequestedDropoffLocationId = model.RequestedDropoffLocationId,
            Description = model.Description.Trim(),
            CreatedAt = DateTime.UtcNow,
            ResolvedAt = model.ResolvedAt,
            Priority = model.Priority,
            Status = model.Status
        };

        var assignedAgents = await _context.Agents
            .Where(agent => model.AssignedAgentIds.Contains(agent.Id))
            .ToListAsync();

        foreach (var agent in assignedAgents)
        {
            ticket.AssignedAgents.Add(agent);
        }

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = ticket.Id });
    }

    [HttpGet("detalji/{id:int}")]
    public IActionResult Details(int id)
    {
        var ticket = _context.Tickets
            .AsNoTracking()
            .Include(t => t.RequestedDropoffLocation)
            .Include(t => t.AssignedAgents)
            .Include(t => t.Booking)
                .ThenInclude(b => b.Customer)
            .Include(t => t.Booking)
                .ThenInclude(b => b.PlannedDropoffLocation)
            .FirstOrDefault(t => t.Id == id);

        if (ticket is null)
        {
            return NotFound();
        }

        return View(ticket);
    }

    [HttpGet("uredi/{id:int}")]
    [ActionName("Edit")]
    public IActionResult EditGet(int id)
    {
        var ticket = _context.Tickets
            .Include(t => t.AssignedAgents)
            .FirstOrDefault(t => t.Id == id);

        if (ticket is null)
        {
            return NotFound();
        }

        PrepareLookups(ticket.BookingId, ticket.RequestedDropoffLocationId, ticket.Priority, ticket.Status, ticket.AssignedAgents.Select(agent => agent.Id).ToList());

        return View(new TicketEditModel
        {
            Id = ticket.Id,
            TicketNumber = ticket.TicketNumber,
            BookingId = ticket.BookingId,
            RequestedDropoffLocationId = ticket.RequestedDropoffLocationId,
            Description = ticket.Description,
            Priority = ticket.Priority,
            Status = ticket.Status,
            ResolvedAt = ticket.ResolvedAt,
            AssignedAgentIds = ticket.AssignedAgents.Select(agent => agent.Id).ToList()
        });
    }

    [HttpPost("uredi/{id:int}")]
    [ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost(int id, TicketEditModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var ticket = await _context.Tickets
            .Include(t => t.AssignedAgents)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket is null)
        {
            return NotFound();
        }

        ValidateTicketModel(model);

        if (!ModelState.IsValid)
        {
            PrepareLookups(model.BookingId, model.RequestedDropoffLocationId, model.Priority, model.Status, model.AssignedAgentIds);
            return View(model);
        }

        ticket.TicketNumber = model.TicketNumber.Trim();
        ticket.BookingId = model.BookingId;
        ticket.RequestedDropoffLocationId = model.RequestedDropoffLocationId;
        ticket.Description = model.Description.Trim();
        ticket.Priority = model.Priority;
        ticket.Status = model.Status;
        ticket.ResolvedAt = model.ResolvedAt;

        ticket.AssignedAgents.Clear();
        var assignedAgents = await _context.Agents
            .Where(agent => model.AssignedAgentIds.Contains(agent.Id))
            .ToListAsync();

        foreach (var agent in assignedAgents)
        {
            ticket.AssignedAgents.Add(agent);
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = ticket.Id });
    }

    [HttpPost("obrisi/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var ticket = await _context.Tickets
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket is null || ticket.DeletedAt is not null)
        {
            return NotFound();
        }

        ticket.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private void ValidateTicketModel(TicketFormModel model)
    {
        if (model.BookingId <= 0)
        {
            ModelState.AddModelError(nameof(model.BookingId), "Selected booking does not exist.");
        }

        if (model.RequestedDropoffLocationId <= 0)
        {
            ModelState.AddModelError(nameof(model.RequestedDropoffLocationId), "Selected location does not exist.");
        }

        if (model.AssignedAgentIds.Count == 0)
        {
            ModelState.AddModelError(nameof(model.AssignedAgentIds), "Select at least one agent.");
        }
    }

    private void PrepareLookups(int? bookingId = null, int? requestedDropoffLocationId = null, TicketPriority? priority = null, TicketStatus? status = null, IReadOnlyCollection<int>? assignedAgentIds = null)
    {
        ViewBag.Bookings = new SelectList(
            _context.Bookings
                .Where(booking => booking.DeletedAt == null)
                .Include(booking => booking.Customer)
                .OrderByDescending(booking => booking.PickupAt)
                .Select(booking => new { booking.Id, Label = $"{booking.ReservationCode} - {booking.Customer.FirstName} {booking.Customer.LastName}" })
                .ToList(),
            "Id",
            "Label",
            bookingId);

        ViewBag.Locations = new SelectList(
            _context.Locations
                .Where(location => location.DeletedAt == null)
                .OrderBy(location => location.City)
                .ThenBy(location => location.Name)
                .Select(location => new { location.Id, Label = $"{location.Name}, {location.City}" })
                .ToList(),
            "Id",
            "Label",
            requestedDropoffLocationId);

        ViewBag.TicketPriorities = new SelectList(
            Enum.GetValues<TicketPriority>()
                .Select(value => new { Id = value, Label = value.ToString() })
                .ToList(),
            "Id",
            "Label",
            priority);

        ViewBag.TicketStatuses = new SelectList(
            Enum.GetValues<TicketStatus>()
                .Select(value => new { Id = value, Label = value.ToString() })
                .ToList(),
            "Id",
            "Label",
            status);

        ViewBag.Agents = new MultiSelectList(
            _context.Agents
                .OrderByDescending(agent => agent.IsOnDuty)
                .ThenBy(agent => agent.FullName)
                .Select(agent => new { agent.Id, Label = $"{agent.FullName} ({agent.TeamName})" })
                .ToList(),
            "Id",
            "Label",
            assignedAgentIds);
    }
}
