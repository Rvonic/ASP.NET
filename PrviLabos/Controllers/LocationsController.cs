using Microsoft.AspNetCore.Mvc;
using PrviLabos.Data;

namespace PrviLabos.Controllers;

public class LocationsController : Controller
{
    private readonly MockRepositorySet _repositories;

    public LocationsController(MockRepositorySet repositories)
    {
        _repositories = repositories;
    }

    public IActionResult Index()
    {
        var locations = _repositories.Locations.GetAll()
            .OrderBy(l => l.City)
            .ThenBy(l => l.Name)
            .ToList();

        return View(locations);
    }

    public IActionResult Details(int id)
    {
        var location = _repositories.Locations.GetById(id);
        if (location is null)
        {
            return NotFound();
        }

        return View(location);
    }
}
