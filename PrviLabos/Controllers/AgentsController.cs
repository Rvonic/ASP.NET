using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrviLabos.DAL;
using PrviLabos.Model;
using PrviLabos.Models;
using PrviLabos.Services.Validation;

namespace PrviLabos.Controllers;

[Route("agenti")]
[Authorize]
public class AgentsController : Controller
{
    private readonly PrviLabosDbContext _context;
    private readonly AgentFormValidator _validator;

    public AgentsController(PrviLabosDbContext context, AgentFormValidator validator)
    {
        _context = context;
        _validator = validator;
    }

    [HttpGet("")]
    [AllowAnonymous]
    public IActionResult Index(string? sort)
    {
        var agents = _context.Agents.AsQueryable();

        switch (sort)
        {
            case "name_asc":
                agents = agents.OrderBy(a => a.FullName);
                break;
            case "name_desc":
                agents = agents.OrderByDescending(a => a.FullName);
                break;
            case "team_asc":
                agents = agents.OrderBy(a => a.TeamName).ThenBy(a => a.FullName);
                break;
            case "team_desc":
                agents = agents.OrderByDescending(a => a.TeamName).ThenBy(a => a.FullName);
                break;
            case "status_asc":
                agents = agents.OrderBy(a => a.IsOnDuty).ThenBy(a => a.FullName);
                break;
            case "status_desc":
                agents = agents.OrderByDescending(a => a.IsOnDuty).ThenBy(a => a.FullName);
                break;
            case "tickets_asc":
                agents = agents.OrderBy(a => a.AssignedTickets.Count).ThenBy(a => a.FullName);
                break;
            case "tickets_desc":
                agents = agents.OrderByDescending(a => a.AssignedTickets.Count).ThenBy(a => a.FullName);
                break;
            default:
                agents = agents.OrderByDescending(a => a.IsOnDuty).ThenBy(a => a.FullName);
                break;
        }

        return View(agents.ToList());
    }

    [HttpGet("novi")]
    [ActionName("Create")]
    [Authorize(Roles = "Admin,Manager")]
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
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreatePost(AgentCreateModel model)
    {
        _validator.Validate(model, ModelState);

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
    [AllowAnonymous]
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
    [Authorize(Roles = "Admin,Manager")]
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
    [Authorize(Roles = "Admin,Manager")]
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

        _validator.Validate(model, ModelState);

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
    [Authorize(Roles = "Admin")]
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
        TempData["StatusMessage"] = "Agent was deleted successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("obrisi")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkDelete([FromForm] int[] ids)
    {
        if (ids == null || ids.Length == 0)
        {
            TempData["StatusMessage"] = "No agents selected.";
            return RedirectToAction(nameof(Index));
        }

        var agents = _context.Agents.Where(a => ids.Contains(a.Id)).ToList();
        foreach (var agent in agents)
        {
            agent.DeletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = $"Deleted {agents.Count} agents.";

        return RedirectToAction(nameof(Index));
    }
}
