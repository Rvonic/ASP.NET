using Microsoft.AspNetCore.Mvc;
using PrviLabos.Data;

namespace PrviLabos.Controllers;

public class AgentsController : Controller
{
    private readonly MockRepositorySet _repositories;

    public AgentsController(MockRepositorySet repositories)
    {
        _repositories = repositories;
    }

    public IActionResult Index()
    {
        var agents = _repositories.Agents.GetAll()
            .OrderByDescending(a => a.IsOnDuty)
            .ThenBy(a => a.FullName)
            .ToList();

        return View(agents);
    }

    public IActionResult Details(int id)
    {
        var agent = _repositories.Agents.GetById(id);
        if (agent is null)
        {
            return NotFound();
        }

        return View(agent);
    }
}
