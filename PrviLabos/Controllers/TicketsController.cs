using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PrviLabos.DAL;
using PrviLabos.Model;
using PrviLabos.Models;
using PrviLabos.Services.Validation;

namespace PrviLabos.Controllers;

[Route("prijave")]
[Authorize]
public class TicketsController : Controller
{
    private readonly PrviLabosDbContext _context;
    private readonly TicketFormValidator _validator;

    public TicketsController(PrviLabosDbContext context, TicketFormValidator validator)
    {
        _context = context;
        _validator = validator;
    }

    [HttpGet("")]
    [AllowAnonymous]
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
    [AllowAnonymous]
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

    [HttpGet("autocomplete/rezervacije")]
    public IActionResult AutocompleteBookings(string? query)
    {
        var normalized = query?.Trim();
        var bookings = _context.Bookings
            .AsNoTracking()
            .Include(b => b.Customer)
            .OrderByDescending(b => b.PickupAt)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(normalized))
        {
            bookings = bookings.Where(b =>
                b.ReservationCode.Contains(normalized) ||
                b.Customer.FirstName.Contains(normalized) ||
                b.Customer.LastName.Contains(normalized));
        }

        var result = bookings
            .Take(15)
            .Select(b => new
            {
                id = b.Id,
                text = b.ReservationCode + " - " + b.Customer.FirstName + " " + b.Customer.LastName
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

    [HttpGet("autocomplete/prioriteti")]
    public IActionResult AutocompletePriorities(string? query)
    {
        var normalized = query?.Trim();
        var priorities = Enum.GetValues<TicketPriority>()
            .Select(value => new { id = (int)value, text = value.ToString() });

        if (!string.IsNullOrWhiteSpace(normalized))
        {
            priorities = priorities.Where(p => p.text.Contains(normalized, StringComparison.OrdinalIgnoreCase));
        }

        return Json(priorities.Take(20).ToList());
    }

    [HttpGet("autocomplete/statusi")]
    public IActionResult AutocompleteStatuses(string? query)
    {
        var normalized = query?.Trim();
        var statuses = Enum.GetValues<TicketStatus>()
            .Select(value => new { id = (int)value, text = value.ToString() });

        if (!string.IsNullOrWhiteSpace(normalized))
        {
            statuses = statuses.Where(s => s.text.Contains(normalized, StringComparison.OrdinalIgnoreCase));
        }

        return Json(statuses.Take(20).ToList());
    }

    [HttpGet("novi")]
    [ActionName("Create")]
    [Authorize(Roles = "Admin,Manager")]
    public IActionResult CreateGet()
    {
        PrepareLookups();
        PopulateAutocompleteSelections(null, null, TicketPriority.Medium, TicketStatus.Open);

        return View(new TicketCreateModel
        {
            ResolvedAt = DateTime.Now,
            Priority = TicketPriority.Medium,
            Status = TicketStatus.Open
        });
    }

    [HttpPost("novi")]
    [ActionName("Create")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreatePost(TicketCreateModel model)
    {
        _validator.Validate(model, ModelState);

        if (!ModelState.IsValid)
        {
            PrepareLookups(model.BookingId, model.RequestedDropoffLocationId, model.Priority, model.Status, model.AssignedAgentIds);
            PopulateAutocompleteSelections(model.BookingId, model.RequestedDropoffLocationId, model.Priority, model.Status);
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
    [Authorize(Roles = "Admin,Manager")]
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
        PopulateAutocompleteSelections(ticket.BookingId, ticket.RequestedDropoffLocationId, ticket.Priority, ticket.Status);

        return View(new TicketEditModel
        {
            Id = ticket.Id,
            TicketNumber = ticket.TicketNumber,
            BookingId = ticket.BookingId,
            RequestedDropoffLocationId = ticket.RequestedDropoffLocationId,
            Description = ticket.Description,
            Priority = ticket.Priority,
            Status = ticket.Status,
            ResolvedAt = ticket.ResolvedAt ?? DateTime.Now,
            AssignedAgentIds = ticket.AssignedAgents.Select(agent => agent.Id).ToList()
        });
    }

    [HttpPost("uredi/{id:int}")]
    [ActionName("Edit")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Manager")]
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

        _validator.Validate(model, ModelState);

        if (!ModelState.IsValid)
        {
            PrepareLookups(model.BookingId, model.RequestedDropoffLocationId, model.Priority, model.Status, model.AssignedAgentIds);
            PopulateAutocompleteSelections(model.BookingId, model.RequestedDropoffLocationId, model.Priority, model.Status);
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
    [Authorize(Roles = "Admin")]
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
        TempData["StatusMessage"] = "Ticket was deleted successfully.";

        return RedirectToAction(nameof(Index));
    }
    private void PrepareLookups(int? bookingId = null, int? requestedDropoffLocationId = null, TicketPriority? priority = null, TicketStatus? status = null, IReadOnlyCollection<int>? assignedAgentIds = null)
    {
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

    private void PopulateAutocompleteSelections(int? bookingId, int? requestedDropoffLocationId, TicketPriority? priority, TicketStatus? status)
    {
        ViewBag.BookingDisplay = bookingId.HasValue
            ? _context.Bookings
                .IgnoreQueryFilters()
                .Include(b => b.Customer)
                .Where(b => b.Id == bookingId.Value)
                .Select(b => b.ReservationCode + " - " + b.Customer.FirstName + " " + b.Customer.LastName)
                .FirstOrDefault()
            : null;

        ViewBag.LocationDisplay = requestedDropoffLocationId.HasValue
            ? _context.Locations
                .IgnoreQueryFilters()
                .Where(l => l.Id == requestedDropoffLocationId.Value)
                .Select(l => l.Name + ", " + l.City)
                .FirstOrDefault()
            : null;

        ViewBag.PriorityDisplay = priority?.ToString();
        ViewBag.StatusDisplay = status?.ToString();
    }
}
