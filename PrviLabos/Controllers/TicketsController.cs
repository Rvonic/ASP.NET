using Microsoft.AspNetCore.Mvc;
using PrviLabos.Data;

namespace PrviLabos.Controllers;

public class TicketsController : Controller
{
    private readonly MockRepositorySet _repositories;

    public TicketsController(MockRepositorySet repositories)
    {
        _repositories = repositories;
    }

    public IActionResult Index()
    {
        var tickets = _repositories.Tickets.GetAll()
            .OrderByDescending(t => t.CreatedAt)
            .ToList();

        return View(tickets);
    }

    public IActionResult Details(int id)
    {
        var ticket = _repositories.Tickets.GetById(id);
        if (ticket is null)
        {
            return NotFound();
        }

        return View(ticket);
    }
}