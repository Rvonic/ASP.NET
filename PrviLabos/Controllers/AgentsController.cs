using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrviLabos.DAL;
using PrviLabos.Model;
using PrviLabos.Models;

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

    [HttpGet("novi")]
    [ActionName("Create")]
    public IActionResult CreateGet()
    {
        return View(new AgentCreateModel
        {
            ShiftStart = DateTime.Today.AddHours(8),
            ShiftEnd = DateTime.Today.AddHours(16),
            IsOnDuty = true
        });
    }

    [HttpPost("novi")]
    [ActionName("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePost(AgentCreateModel model)
    {
        ValidateShiftOrder(model);

        if (!ModelState.IsValid)
        {
            return View("Create", model);
        }

        var agent = new SupportAgent
        {
            FullName = model.FullName.Trim(),
            Email = model.Email.Trim(),
            TeamName = model.TeamName.Trim(),
            ShiftStart = model.ShiftStart!.Value,
            ShiftEnd = model.ShiftEnd!.Value,
            IsOnDuty = model.IsOnDuty
        };

        _context.Agents.Add(agent);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = agent.Id });
    }

    [HttpGet("pretraga")]
    public IActionResult Search(string? query)
    {
        var normalizedQuery = query?.Trim();

        var agents = _context.Agents
            .OrderByDescending(a => a.IsOnDuty)
            .ThenBy(a => a.FullName)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(normalizedQuery))
        {
            agents = agents.Where(a =>
                a.FullName.Contains(normalizedQuery) ||
                a.Email.Contains(normalizedQuery) ||
                a.TeamName.Contains(normalizedQuery));
        }

        return PartialView("_AgentRows", agents.Take(50).ToList());
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

    [HttpGet("uredi/{id:int}")]
    [ActionName("Edit")]
    public IActionResult EditGet(int id)
    {
        var agent = _context.Agents.FirstOrDefault(a => a.Id == id);
        if (agent is null)
        {
            return NotFound();
        }

        return View(new AgentEditModel
        {
            Id = agent.Id,
            FullName = agent.FullName,
            Email = agent.Email,
            TeamName = agent.TeamName,
            ShiftStart = agent.ShiftStart,
            ShiftEnd = agent.ShiftEnd,
            IsOnDuty = agent.IsOnDuty
        });
    }

    [HttpPost("uredi/{id:int}")]
    [ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost(int id, AgentEditModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var agent = await _context.Agents.FirstOrDefaultAsync(a => a.Id == id);
        if (agent is null)
        {
            return NotFound();
        }

        ValidateShiftOrder(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        agent.FullName = model.FullName.Trim();
        agent.Email = model.Email.Trim();
        agent.TeamName = model.TeamName.Trim();
        agent.ShiftStart = model.ShiftStart!.Value;
        agent.ShiftEnd = model.ShiftEnd!.Value;
        agent.IsOnDuty = model.IsOnDuty;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = agent.Id });
    }

    [HttpPost("obrisi/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var agent = await _context.Agents
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (agent is null || agent.DeletedAt is not null)
        {
            return NotFound();
        }

        agent.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private void ValidateShiftOrder(AgentFormModel model)
    {
        if (model.ShiftStart.HasValue && model.ShiftEnd.HasValue && model.ShiftEnd <= model.ShiftStart)
        {
            ModelState.AddModelError(nameof(model.ShiftEnd), "Kraj smjene mora biti nakon početka smjene.");
        }
    }
}
