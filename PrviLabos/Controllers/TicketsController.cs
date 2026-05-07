using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrviLabos.DAL;

namespace PrviLabos.Controllers;

public class TicketsController : Controller
{
      private readonly PrviLabosDbContext _context;

    public TicketsController(PrviLabosDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var tickets = _context.Tickets
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

    public IActionResult Details(int id)
    {var ticket = _context.Tickets
            .Include(t => t.AssignedAgents) // Za listu agenata na dnu
            .Include(t => t.Booking)
                .ThenInclude(b => b.Customer) // Za ime kupca
            .Include(t => t.Booking)
                .ThenInclude(b => b.PlannedDropoffLocation) // Ključno za "Requested dropoff" sekciju
            .FirstOrDefault(t => t.Id == id);

        return View(ticket);
    }
}