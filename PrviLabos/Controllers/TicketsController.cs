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
    {
    var ticket = _context.Tickets
            // 1. Učitaj lokaciju koja je direktno vezana uz Ticket
            .Include(t => t.RequestedDropoffLocation) 
            
            // 2. Učitaj agente (mnogo-na-mnogo veza u migraciji)
            .Include(t => t.AssignedAgents) 
            
            // 3. Učitaj Booking i njegove povezane entitete
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
}