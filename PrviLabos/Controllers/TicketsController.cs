using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrviLabos.DAL;
using PrviLabos.Model;

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
            .OrderByDescending(t => t.CreatedAt)
            .ToList();

        return View(tickets);
    }

    [HttpGet("{id:int}")]
    public IActionResult Details(int id)
    {
        var ticket = _context.Tickets
            .Include(t => t.Booking)
            .ThenInclude(b => b.Customer)
            .Include(t => t.Booking)
            .ThenInclude(b => b.Vehicle)
            .Include(t => t.Booking)
            .ThenInclude(b => b.PickupLocation)
            .Include(t => t.Booking)
            .ThenInclude(b => b.PlannedDropoffLocation)
            .Include(t => t.RequestedDropoffLocation)
            .Include(t => t.AssignedAgents)
            .FirstOrDefault(t => t.Id == id);

        if (ticket is null)
        {
            return NotFound();
        }

        return View(ticket);
    }
}