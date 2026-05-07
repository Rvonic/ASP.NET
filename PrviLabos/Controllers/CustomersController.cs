using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrviLabos.DAL;
using PrviLabos.Model;

namespace PrviLabos.Controllers;

[Route("kupci")]
public class CustomersController : Controller
{
    private readonly PrviLabosDbContext _context;

    public CustomersController(PrviLabosDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        var customers = _context.Customers
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToList();

        return View(customers);
    }

    [HttpGet("detalji/{id:int}")]
    public IActionResult Details(int id)
    {
        var customer = _context.Customers
            .Include(c => c.Bookings)
            .ThenInclude(b => b.Vehicle)
            .Include(c => c.Bookings)
            .ThenInclude(b => b.SupportTickets)
            .FirstOrDefault(c => c.Id == id);

        if (customer is null)
        {
            return NotFound();
        }

        return View(customer);
    }
}