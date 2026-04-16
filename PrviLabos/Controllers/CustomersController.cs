using Microsoft.AspNetCore.Mvc;
using PrviLabos.Data;

namespace PrviLabos.Controllers;

public class CustomersController : Controller
{
    private readonly MockRepositorySet _repositories;

    public CustomersController(MockRepositorySet repositories)
    {
        _repositories = repositories;
    }

    public IActionResult Index()
    {
        var customers = _repositories.Customers.GetAll()
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToList();

        return View(customers);
    }

    public IActionResult Details(int id)
    {
        var customer = _repositories.Customers.GetById(id);
        if (customer is null)
        {
            return NotFound();
        }

        return View(customer);
    }
}