using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrviLabos.DAL;
using PrviLabos.Model;
using PrviLabos.Models;

namespace PrviLabos.Controllers.Api;

[ApiController]
[Route("api/agents")]
public sealed class AgentsApiController : ControllerBase
{
    private readonly PrviLabosDbContext _context;

    public AgentsApiController(PrviLabosDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupportAgentDto>>> GetAll([FromQuery] string? query)
    {
        var agents = _context.Agents.AsNoTracking().OrderBy(a => a.FullName).AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var normalized = query.Trim();
            agents = agents.Where(a =>
                a.FullName.Contains(normalized) ||
                a.Email.Contains(normalized) ||
                a.TeamName.Contains(normalized));
        }

        return Ok(await agents.Take(100).Select(a => a.ToDto()).ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SupportAgentDto>> GetById(int id)
    {
        var agent = await _context.Agents.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
        return agent is null ? NotFound() : Ok(agent.ToDto());
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<SupportAgentDto>> Post(SupportAgentUpsertDto model)
    {
        if (model.ShiftEnd <= model.ShiftStart)
        {
            return BadRequest("Shift end must be after shift start.");
        }

        var agent = new SupportAgent();
        Apply(model, agent);

        _context.Agents.Add(agent);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = agent.Id }, agent.ToDto());
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<SupportAgentDto>> Put(int id, SupportAgentUpsertDto model)
    {
        if (model.ShiftEnd <= model.ShiftStart)
        {
            return BadRequest("Shift end must be after shift start.");
        }

        var agent = await _context.Agents.FirstOrDefaultAsync(a => a.Id == id);
        if (agent is null)
        {
            return NotFound();
        }

        Apply(model, agent);
        await _context.SaveChangesAsync();

        return Ok(agent.ToDto());
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var agent = await _context.Agents.FirstOrDefaultAsync(a => a.Id == id);
        if (agent is null)
        {
            return NotFound();
        }

        agent.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static void Apply(SupportAgentUpsertDto model, SupportAgent agent)
    {
        agent.FullName = model.FullName.Trim();
        agent.Email = model.Email.Trim();
        agent.TeamName = model.TeamName.Trim();
        agent.ShiftStart = model.ShiftStart;
        agent.ShiftEnd = model.ShiftEnd;
        agent.IsOnDuty = model.IsOnDuty;
    }
}
