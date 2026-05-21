using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrviLabos.DAL;
using PrviLabos.Model;
using PrviLabos.Models;

namespace PrviLabos.Controllers;

[Route("")]

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly PrviLabosDbContext _context;

    public HomeController(ILogger<HomeController> logger, PrviLabosDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpGet("")]
    [HttpGet("naslovnica")]
    public IActionResult Index()
    {
        var now = DateTime.UtcNow;

        var bookings = _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.PickupLocation)
            .Include(b => b.PlannedDropoffLocation)
            .AsNoTracking()
            .ToList();

        var tickets = _context.Tickets
            .Include(t => t.Booking)
            .ThenInclude(b => b.Customer)
            .AsNoTracking()
            .ToList();

        var customers = _context.Customers.AsNoTracking().ToList();

        var model = new HomeDashboardViewModel
        {
            ActiveBookings = bookings.Count(b => b.Status == BookingStatus.Active),
            OpenTickets = tickets.Count(t => t.Status is TicketStatus.Open or TicketStatus.InProgress or TicketStatus.Escalated),
            Customers = customers.Count,
            OnDutyAgents = _context.Agents.Count(a => a.IsOnDuty),
            UpcomingDropoffs = bookings.Count(b => b.PlannedDropoffAt >= now),
            EscalatedTickets = tickets.Count(t => t.Status == TicketStatus.Escalated),
            LateDropoffBookings = bookings
                .Where(b => b.Status == BookingStatus.Active && b.PlannedDropoffAt < now && b.ActualDropoffAt == null)
                .OrderBy(b => b.PlannedDropoffAt)
                .Take(5)
                .ToList(),
            RecentBookings = bookings.OrderByDescending(b => b.PickupAt).Take(5).ToList(),
            RecentTickets = tickets.OrderByDescending(t => t.CreatedAt).Take(5).ToList()
        };

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
