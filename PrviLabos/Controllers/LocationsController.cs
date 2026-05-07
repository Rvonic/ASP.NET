using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrviLabos.DAL;
using PrviLabos.Model;

namespace PrviLabos.Controllers;

[Route("lokacije")]
public class LocationsController : Controller
{
    private readonly PrviLabosDbContext _context;

    public LocationsController(PrviLabosDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        var locations = _context.Locations
            .OrderBy(l => l.City)
            .ThenBy(l => l.Name)
            .ToList();

        return View(locations);
    }

    [HttpGet("detalji/{id:int}")]
    public IActionResult Details(int id)
    {
        var location = _context.Locations
            .Include(l => l.Vehicles)
            .FirstOrDefault(l => l.Id == id);

        if (location is null)
        {
            return NotFound();
        }

        return View(location);
    }
}
