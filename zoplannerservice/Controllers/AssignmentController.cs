using Microsoft.AspNetCore.Mvc;
using zoplannerservice.Services;
using zoplannerservice.Models;
using System.ComponentModel.DataAnnotations;

namespace zoplannerservice.Controllers;

/// <summary>
/// Assignment controller - handles HTTP requests for assignments
/// Only GET by ID and DELETE operations are implemented
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AssignmentController : ControllerBase
{
    private readonly IBaseService<Assignment> _assignmentService;
    private readonly ILogger<AssignmentController> _logger;

    public AssignmentController(IBaseService<Assignment> assignmentService, ILogger<AssignmentController> logger)
    {
        _assignmentService = assignmentService;
        _logger = logger;
    }

    /// <summary>
    /// Get assignment by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Assignment), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        try
        {
            var assignment = await _assignmentService.GetByIdAsync(id, ct);
            
            if (assignment == null)
            {
                return NotFound(new { message = $"Assignment with ID {id} not found in database" });
            }

            return Ok(assignment);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error for assignment {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Timeout while fetching assignment {Id}", id);
            return StatusCode(StatusCodes.Status504GatewayTimeout, 
                new { message = "Request timed out", details = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error for assignment {Id}", id);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                new { message = "Backend service error", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete assignment by ID
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
            var deleted = await _assignmentService.DeleteAsync(id, ct);
            
            if (!deleted)
            {
                return NotFound(new { message = $"Assignment with ID {id} not found in database" });
            }

            return NoContent();
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while deleting assignment {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error while deleting assignment {Id}", id);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                new { message = "Backend service error", details = ex.Message });
        }
    }
}
