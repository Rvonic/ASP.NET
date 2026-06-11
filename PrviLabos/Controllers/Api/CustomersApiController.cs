using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrviLabos.DAL;
using PrviLabos.Model;
using PrviLabos.Models;

namespace PrviLabos.Controllers.Api;

[ApiController]
[Route("api/customers")]
public sealed class CustomersApiController : ControllerBase
{
    private readonly PrviLabosDbContext _context;

    public CustomersApiController(PrviLabosDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll([FromQuery] string? query)
    {
        var customers = _context.Customers.AsNoTracking().OrderBy(c => c.LastName).ThenBy(c => c.FirstName).AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var normalized = query.Trim();
            customers = customers.Where(c =>
                c.FirstName.Contains(normalized) ||
                c.LastName.Contains(normalized) ||
                c.Email.Contains(normalized) ||
                c.PhoneNumber.Contains(normalized) ||
                c.DriverLicenseNumber.Contains(normalized));
        }

        return Ok(await customers.Take(100).Select(c => c.ToDto()).ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerDto>> GetById(int id)
    {
        var customer = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        return customer is null ? NotFound() : Ok(customer.ToDto());
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<CustomerDto>> Post(CustomerUpsertDto model)
    {
        var customer = new Customer
        {
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            Email = model.Email.Trim(),
            PhoneNumber = model.PhoneNumber.Trim(),
            DriverLicenseNumber = model.DriverLicenseNumber.Trim(),
            DateOfBirth = model.DateOfBirth,
            RegisteredAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer.ToDto());
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<CustomerDto>> Put(int id, CustomerUpsertDto model)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);
        if (customer is null)
        {
            return NotFound();
        }

        customer.FirstName = model.FirstName.Trim();
        customer.LastName = model.LastName.Trim();
        customer.Email = model.Email.Trim();
        customer.PhoneNumber = model.PhoneNumber.Trim();
        customer.DriverLicenseNumber = model.DriverLicenseNumber.Trim();
        customer.DateOfBirth = model.DateOfBirth;

        await _context.SaveChangesAsync();

        return Ok(customer.ToDto());
    }

    [HttpDelete("{id:int}")]
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

        return NoContent();
    }
}
