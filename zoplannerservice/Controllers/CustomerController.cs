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
    private readonly ILogger<CustomerController> _logger;
    private readonly IFileService _fileService;

    public CustomerController(ICustomerService customerService, IFileService fileService, ILogger<CustomerController> logger)
    {
        _customerService = customerService;
        _fileService = fileService;
        _logger = logger;
    }

    /// <summary>
    /// Get all customers
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Customer>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        try
        {
            var customers = await _customerService.GetAllSync(ct);
            return Ok(customers);
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Timeout while fetching customers");
            return StatusCode(StatusCodes.Status504GatewayTimeout,
                new { message = "Request timed out", details = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error while fetching customers");
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { message = "Backend service error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get customer by ID
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Customer object or 404 if not found</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Customer), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        try
        {
            var customer = await _customerService.GetByIdAsync(id, ct);

            if (customer == null)
            {
                return NotFound(new { message = $"Customer with ID {id} not found in database" });
            }

            return Ok(customer);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error for customer {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Timeout while fetching customer {Id}", id);
            return StatusCode(StatusCodes.Status504GatewayTimeout,
                new { message = "Request timed out", details = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error for customer {Id}", id);
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { message = "Backend service error", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete customer by ID
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>204 if deleted, 404 if not found</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        try
        {
            var deleted = await _customerService.DeleteAsync(id, ct);
            
            if (!deleted)
            {
                return NotFound(new { message = $"Customer with ID {id} not found in database" });
            }

            return NoContent();
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while deleting customer {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error while deleting customer {Id}", id);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                new { message = "Backend service error", details = ex.Message });
        }
    }

    /// <summary>
    /// Create a new customer (only Name and City are accepted, ID is generated by backend)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Customer), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request, CancellationToken ct)
    {
        try
        {
            var created = await _customerService.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id}, created);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while creating customer");
            return BadRequest(new { message = ex.Message });
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Timeout while creating customer");
            return StatusCode(StatusCodes.Status504GatewayTimeout,
                new { message = "Request timed out", details = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error while creating customer");
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { message = "Backend service error", details = ex.Message });
        }
    }

    /// <summary>
    /// Patch (partial update) existing customer. Send only fields to change.
    /// </summary>
    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(Customer), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Patch(int id, [FromBody] PatchCustomerRequest request, CancellationToken ct)
    {
        try
        {
            var updated = await _customerService.PatchAsync(id, request, ct);
            if (updated == null)
            {
                return NotFound(new { message = $"Customer with ID {id} not found" });
            }
            return Ok(updated);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while patching customer {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error while patching customer {Id}", id);
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { message = "Backend service error", details = ex.Message });
        }
    }


    /// <summary>
    /// Upload or change customer image
    /// </summary>
    [HttpPost("{id:int}/image")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Customer), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadImage(int id, IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Image file is required" });

        try
        {
            // Pass ALL required parameters: stream, filename, contentType
            using var stream = file.OpenReadStream();
            var uploaded = await _fileService.UploadAsync(
                stream,
                file.FileName,
                file.ContentType,
                ct);

            var patch = new PatchCustomerRequest { ImageUrl = uploaded.Url };
            var updated = await _customerService.PatchAsync(id, patch, ct);

            if (updated == null)
                return NotFound(new { message = $"Customer with ID {id} not found" });

            return Ok(updated);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while uploading image for customer {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error while uploading image for customer {Id}", id);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Backend service error", details = ex.Message });
        }
    }
    /// <summary>
    /// Remove customer image (and optionally delete file in backend)
    /// </summary>
    [HttpDelete("{id:int}/image")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteImage(int id, CancellationToken ct)
    {
        try
        {
            var customer = await _customerService.GetByIdAsync(id, ct);
            if (customer == null)
                return NotFound(new { message = $"Customer with ID {id} not found" });

            // If you track image by Id, call _fileService.DeleteAsync(imageId, ct) here

            var patch = new PatchCustomerRequest
            {
                ImageUrl = null
            };
            await _customerService.PatchAsync(id, patch, ct);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error while deleting image for customer {Id}", id);
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { message = "Backend service error", details = ex.Message });
        }
    }
}