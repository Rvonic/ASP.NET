using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrviLabos.DAL;
using PrviLabos.Model;

namespace PrviLabos.Controllers;

[Route("vozila")]
public class VehiclesController : Controller
{
    private readonly PrviLabosDbContext _context;

    public VehiclesController(PrviLabosDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        var vehicles = _context.Vehicles
            .OrderBy(v => v.IsAvailable ? 0 : 1)
            .ThenBy(v => v.Brand)
            .ThenBy(v => v.Model)
            .ToList();

        return View(vehicles);
    }

    [HttpGet("detalji/{id:int}")]
    public IActionResult Details(int id)
    {
        var vehicle = _context.Vehicles
            .Include(v => v.CurrentLocation)
            .Include(v => v.Bookings)
            .ThenInclude(b => b.Customer)
            .FirstOrDefault(v => v.Id == id);

        if (vehicle is null)
        {
            return NotFound();
        }

        return View(vehicle);
    }
}
