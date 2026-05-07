using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrviLabos.DAL;
using PrviLabos.Model;

namespace PrviLabos.Controllers;

[Route("rezervacije")]
public class BookingsController : Controller
{
    private readonly PrviLabosDbContext _context;

    public BookingsController(PrviLabosDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        var bookings = _context.Bookings
    .Include(b => b.Customer)
    .Include(b => b.Vehicle)
    .Include(b => b.PickupLocation)
    .Include(b => b.PlannedDropoffLocation)
    .OrderByDescending(b => b.PickupAt)
    .ToList();

        return View(bookings);
    }

    [HttpGet("detalji/{id:int}")]
    public IActionResult Details(int id)
    {
        var booking = _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Vehicle)
            .Include(b => b.PickupLocation)
            .Include(b => b.PlannedDropoffLocation)
            .Include(b => b.SupportTickets)
            .FirstOrDefault(b => b.Id == id);

        if (booking is null)
        {
            return NotFound();
        }

        return View(booking);
    }
}

