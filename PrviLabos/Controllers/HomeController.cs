using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PrviLabos.Data;
using PrviLabos.Domain;
using PrviLabos.Models;

namespace PrviLabos.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly MockRepositorySet _repositories;

    public HomeController(ILogger<HomeController> logger, MockRepositorySet repositories)
    {
        _logger = logger;
        _repositories = repositories;
    }

    public IActionResult Index()
    {
        var bookings = _repositories.Bookings.GetAll();
        var tickets = _repositories.Tickets.GetAll();
        var customers = _repositories.Customers.GetAll();

        var model = new HomeDashboardViewModel
        {
            ActiveBookings = bookings.Count(b => b.Status == BookingStatus.Active),
            OpenTickets = tickets.Count(t => t.Status is TicketStatus.Open or TicketStatus.InProgress or TicketStatus.Escalated),
            Customers = customers.Count,
            OnDutyAgents = _repositories.Context.Agents.Count(a => a.IsOnDuty),
            UpcomingDropoffs = bookings.Count(b => b.PlannedDropoffAt >= DateTime.UtcNow),
            EscalatedTickets = tickets.Count(t => t.Status == TicketStatus.Escalated),
            LateDropoffBookings = bookings
                .Where(b => b.Status == BookingStatus.Active && b.PlannedDropoffAt < DateTime.UtcNow && b.ActualDropoffAt == null)
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
