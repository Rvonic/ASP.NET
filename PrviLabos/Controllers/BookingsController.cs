using Microsoft.AspNetCore.Mvc;
using PrviLabos.Data;

namespace PrviLabos.Controllers;

public class BookingsController : Controller
{
    private readonly MockRepositorySet _repositories;

    public BookingsController(MockRepositorySet repositories)
    {
        _repositories = repositories;
    }

    public IActionResult Index()
    {
        var bookings = _repositories.Bookings.GetAll()
            .OrderByDescending(b => b.PickupAt)
            .ToList();

        return View(bookings);
    }

    public IActionResult Details(int id)
    {
        var booking = _repositories.Bookings.GetById(id);
        if (booking is null)
        {
            return NotFound();
        }

        return View(booking);
    }
}