using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrviLabos.DAL;
using PrviLabos.Model;
using PrviLabos.Models;
using PrviLabos.Services.Validation;

namespace PrviLabos.Controllers;

[Route("kupci")]
[Authorize]
public class CustomersController : Controller
{
    private readonly PrviLabosDbContext _context;
    private readonly CustomerFormValidator _validator;

    public CustomersController(PrviLabosDbContext context, CustomerFormValidator validator)
    {
        _context = context;
        _validator = validator;
    }

    [HttpGet("")]
    [AllowAnonymous]
    public IActionResult Index()
    {
        var customers = _context.Customers
            .Where(c => c.DeletedAt == null)
            .Include(c => c.Bookings)
            .ThenInclude(b => b.SupportTickets)
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToList();

        return View(customers);
    }

    [HttpGet("pretraga")]
    [AllowAnonymous]
    public IActionResult Search(string? query)
    {
        var normalizedQuery = query?.Trim();

        var customers = _context.Customers
            .Where(c => c.DeletedAt == null)
            .Include(c => c.Bookings)
            .ThenInclude(b => b.SupportTickets)
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(normalizedQuery))
        {
            customers = customers.Where(c =>
                c.FirstName.Contains(normalizedQuery) ||
                c.LastName.Contains(normalizedQuery) ||
                c.Email.Contains(normalizedQuery) ||
                c.PhoneNumber.Contains(normalizedQuery));
        }

        return PartialView("_CustomerRows", customers.Take(50).ToList());
    }

    [HttpGet("novi")]
    [ActionName("Create")]
    [Authorize(Roles = "Admin,Manager")]
    public IActionResult CreateGet()
    {
        return View(new CustomerCreateModel
        {
            PhoneCountryCode = PhoneCountryCatalog.DefaultDialCode
        });
    }

    [HttpGet("detalji/{id:int}")]
    public IActionResult Details(int id)
    {
        var customer = _context.Customers
            .Include(c => c.Bookings)
            .ThenInclude(b => b.Vehicle)
            .Include(c => c.Bookings)
            .ThenInclude(b => b.SupportTickets)
            .FirstOrDefault(c => c.Id == id);

        if (customer is null)
        {
            return NotFound();
        }

        if (customer.DeletedAt is not null)
        {
            return NotFound();
        }

        return View(customer);
    }

    [HttpPost("novi")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create(CustomerCreateModel model)
    {
        _validator.Validate(model, ModelState);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var customer = new Customer
        {
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            Email = model.Email.Trim(),
            PhoneNumber = PhoneCountryCatalog.ComposePhoneNumber(model.PhoneCountryCode, model.PhoneLocalNumber),
            DriverLicenseNumber = model.DriverLicenseNumber.Trim(),
            DateOfBirth = model.DateOfBirth!.Value,
            RegisteredAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = customer.Id });
    }

    [HttpGet("uredi/{id:int}")]
    [ActionName("Edit")]
    [Authorize(Roles = "Admin,Manager")]
    public IActionResult EditGet(int id)
    {
        var customer = _context.Customers.Find(id);
        if (customer is null)
        {
            return NotFound();
        }

        if (customer.DeletedAt is not null)
        {
            return NotFound();
        }

        return View(new CustomerEditModel
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            PhoneCountryCode = PhoneCountryCatalog.TrySplitPhoneNumber(customer.PhoneNumber, out var dialCode, out var localNumber)
                ? dialCode
                : PhoneCountryCatalog.DefaultDialCode,
            PhoneLocalNumber = PhoneCountryCatalog.TrySplitPhoneNumber(customer.PhoneNumber, out _, out var existingLocalNumber)
                ? existingLocalNumber
                : string.Empty,
            DriverLicenseNumber = customer.DriverLicenseNumber,
            DateOfBirth = customer.DateOfBirth
        });
    }

    [HttpPost("uredi/{id:int}")]
    [ActionName("Edit")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> EditPost(int id, CustomerEditModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var customer = await _context.Customers.FindAsync(id);
        if (customer is null)
        {
            return NotFound();
        }

        if (customer.DeletedAt is not null)
        {
            return NotFound();
        }

        _validator.Validate(model, ModelState);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        customer.FirstName = model.FirstName.Trim();
        customer.LastName = model.LastName.Trim();
        customer.Email = model.Email.Trim();
        customer.PhoneNumber = PhoneCountryCatalog.ComposePhoneNumber(model.PhoneCountryCode, model.PhoneLocalNumber);
        customer.DriverLicenseNumber = model.DriverLicenseNumber.Trim();
        customer.DateOfBirth = model.DateOfBirth!.Value;

        await _context.SaveChangesAsync();
        return RedirectToAction("Details", new { id = customer.Id });
    }

    [HttpPost("obrisi/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);

        if (customer is null)
        {
            return NotFound();
        }

        customer.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Customer was deleted successfully.";

        return RedirectToAction(nameof(Index));
    }
}
