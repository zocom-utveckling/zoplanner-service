using Microsoft.AspNetCore.Mvc;
using zoplannerservice.Services;
using zoplannerservice.Models;
using System.ComponentModel.DataAnnotations;

namespace zoplannerservice.Controllers;

/// <summary>
/// Class controller - handles HTTP requests for classes
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ClassController : ControllerBase
{
    private readonly IClassService _classService;
    private readonly ILogger<ClassController> _logger;

    public ClassController(IClassService classService, ILogger<ClassController> logger)
    {
        _classService = classService;
        _logger = logger;
    }

    /// <summary>
    /// Get class by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Class), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<Class>> GetById(int id, CancellationToken ct)
    {
        try
        {
            var classEntity = await _classService.GetByIdAsync(id, ct);

            if (classEntity == null)
            {
                return NotFound(new { message = $"Class with ID {id} not found in database" });
            }

            return Ok(classEntity);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error for class {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Timeout while fetching class {Id}", id);
            return StatusCode(StatusCodes.Status504GatewayTimeout,
                new { message = "Request timed out", details = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error for class {Id}", id);
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { message = "Backend service error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get all classes 
    /// </summary>

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Class>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<IEnumerable<Class>>> GetAll(CancellationToken ct)
    {
        try
        {
            var classes = await _classService.GetAllSync(ct);
            return Ok(classes);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error while fetching all classes");
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { message = "Backend service error", details = ex.Message });
        }
    }

    /// <summary>
    /// Create a new class      
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Class), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Class>> Create([FromBody] CreateClassRequest request, CancellationToken ct)
    {
        try
        {
            var createdClass = await _classService.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = createdClass.Id }, createdClass);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while creating class");
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error while creating class");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Failed to create class", details = ex.Message });
        }
    }

    /// <summary>
    /// Get all classes for a specific customer 
    /// </summary>
    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(typeof(IEnumerable<Class>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Class>>> GetByCustomerId(int customerId, CancellationToken ct)
    {
        try
        {
            var classes = await _classService.GetByCustomerIdAsync(customerId, ct);
            return Ok(classes);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error for customer {CustomerId}", customerId);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error while fetching classes for customer {CustomerId}", customerId);
            return StatusCode(500, new { message = "Error retrieving classes", details = ex.Message });
        }
    }

    /// <summary>
    /// Partially update a class using PATCH semantics
    /// </summary> 

    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(Class), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Class>> Patch(int id, [FromBody] PatchClassRequest request, CancellationToken ct)
    {
        try
        {
            var updated = await _classService.PatchAsync(id, request, ct);
            if (updated == null)
            {
                return NotFound(new { message = $"Class with ID {id} not found" });
            }
            return Ok(updated);
        }
       
       catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error patching class {Id}", id);
            return StatusCode(500, new { message = "Error patching class", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete class by ID
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        try
        {
            var deleted = await _classService.DeleteAsync(id, ct);

            if (!deleted)
            {
                return NotFound(new { message = $"Class with ID {id} not found in database" });
            }

            return NoContent();
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while deleting class {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error while deleting class {Id}", id);
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { message = "Backend service error", details = ex.Message });
        }
    }
}