using Microsoft.AspNetCore.Mvc;
using PrviLabos.Data;

namespace PrviLabos.Controllers;

public class VehiclesController : Controller
{
    private readonly MockRepositorySet _repositories;

    public VehiclesController(MockRepositorySet repositories)
    {
        _repositories = repositories;
    }

    public IActionResult Index()
    {
        var vehicles = _repositories.Vehicles.GetAll()
            .OrderBy(v => v.IsAvailable ? 0 : 1)
            .ThenBy(v => v.Brand)
            .ThenBy(v => v.Model)
            .ToList();

        return View(vehicles);
    }

    public IActionResult Details(int id)
    {
        var vehicle = _repositories.Vehicles.GetById(id);
        if (vehicle is null)
        {
            return NotFound();
        }

        return View(vehicle);
    }
}
