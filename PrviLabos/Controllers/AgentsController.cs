using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrviLabos.DAL;
using PrviLabos.Model;

namespace PrviLabos.Controllers;

[Route("agenti")]
public class AgentsController : Controller
{
    private readonly PrviLabosDbContext _context;

    public AgentsController(PrviLabosDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        var agents = _context.Agents
            .OrderByDescending(a => a.IsOnDuty)
            .ThenBy(a => a.FullName)
            .ToList();

        return View(agents);
    }

    [HttpGet("{id:int}")]
    public IActionResult Details(int id)
    {
        var agent = _context.Agents
            .Include(a => a.AssignedTickets)
            .ThenInclude(t => t.Booking)
            .FirstOrDefault(a => a.Id == id);

        if (agent is null)
        {
            return NotFound();
        }

        return View(agent);
    }
}
