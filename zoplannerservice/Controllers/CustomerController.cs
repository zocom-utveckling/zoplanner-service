using Microsoft.AspNetCore.Mvc;
using zoplannerservice.Services;
using zoplannerservice.Models;
using System.ComponentModel.DataAnnotations;
using zoplannerservice.Services.Interfaces;

namespace zoplannerservice.Controllers;

/// <summary>
/// Customer controller - handles HTTP requests for customers
/// Implements full CRUD: Get by ID, Get all, Create, Update, Delete
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<IEnumerable<Customer>> GetAll(CancellationToken ct)
        => await _customerService.GetAllAsync(ct);

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var customer = await _customerService.GetByIdAsync(id, ct);
        if (customer == null) return NotFound();
        return Ok(customer);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request, CancellationToken ct)
    {
        var created = await _customerService.CreateAsync(request, ct);
        return Ok(created);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateCustomerRequest request, CancellationToken ct)
    {
        var updated = await _customerService.UpdateAsync(id, request, ct);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var success = await _customerService.DeleteAsync(id, ct);
        if (!success) return NotFound();
        return NoContent();
    }
}