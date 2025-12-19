using Microsoft.AspNetCore.Mvc;
using zoplannerservice.Services;
using zoplannerservice.Models;
using System.ComponentModel.DataAnnotations;

namespace zoplannerservice.Controllers;

/// <summary>
/// Consultant controller - handles HTTP requests for consultants
/// Implements full CRUD operations: GET all, GET by ID, GET by username, POST, PUT, DELETE
/// </summary>


[ApiController]
[Route("api/[controller]")]
public class ConsultantController : ControllerBase
{
    private readonly IConsultantService _consultantService;
    private readonly ILogger<ConsultantController> _logger;

    public ConsultantController(IConsultantService consultantService, ILogger<ConsultantController> logger)
    {
        _consultantService = consultantService;
        _logger = logger;
    }

    /// <summary>
    /// Get all consultants
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Consultant>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        try
        {
            var consultants = await _consultantService.GetAllSync(ct);
            return Ok(consultants);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error while fetching all consultants");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                new { message = "Backend service error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get consultant by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Consultant), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        try
        {
            var consultant = await _consultantService.GetByIdAsync(id, ct);
            
            if (consultant == null)
            {
                return NotFound(new { message = $"Consultant with ID {id} not found in database" });
            }

            return Ok(consultant);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error for consultant {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Timeout while fetching consultant {Id}", id);
            return StatusCode(StatusCodes.Status504GatewayTimeout, 
                new { message = "Request timed out", details = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error for consultant {Id}", id);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                new { message = "Backend service error", details = ex.Message });
        }
    }

    /// <summary>
    /// Create a new consultant
    /// Note: Spring Boot checks if consultantname exists, but this requires custom service method
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Consultant), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Create([FromBody] CreateConsultantRequest request, CancellationToken ct)
    {
        try
        {
            var createdConsultant = await _consultantService.CreateAsync(request, ct);
            
            if (createdConsultant == null)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                    new { message = "Failed to create consultant" });
            }

            return CreatedAtAction(nameof(GetById), new { id = createdConsultant.Id }, createdConsultant);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while creating consultant");
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error while creating consultant");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                new { message = "Backend service error", details = ex.Message });
        }
    }

    /// <summary>
    /// Update consultant by ID
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Consultant), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Update(int id, [FromBody] Consultant consultant, CancellationToken ct)
    {
        try
        {
            var updatedConsultant = await _consultantService.UpdateAsync(id, consultant, ct);
            
            if (updatedConsultant == null)
            {
                return NotFound(new { message = $"Consultant with ID {id} not found in database" });
            }

            return Ok(updatedConsultant);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while updating consultant {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error while updating consultant {Id}", id);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                new { message = "Backend service error", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete consultant by ID
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
            var deleted = await _consultantService.DeleteAsync(id, ct);
            
            if (!deleted)
            {
                return NotFound(new { message = $"Consultant with ID {id} not found in database" });
            }

            return NoContent();
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while deleting consultant {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error while deleting consultant {Id}", id);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                new { message = "Backend service error", details = ex.Message });
        }
    }


    ///// <summary>
    ///// Get user by username
    ///// Note: This requires a custom endpoint in Spring Boot: GET /users/username/{username}
    ///// </summary>
    //[HttpGet("username/{username}")]
    //[ProducesResponseType(typeof(Consultant), StatusCodes.Status200OK)]
    //[ProducesResponseType(StatusCodes.Status404NotFound)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    //public async Task<IActionResult> GetByConsultantname(string username, CancellationToken ct)
    //{
    //    try
    //    {
    //        if (string.IsNullOrWhiteSpace(username))
    //        {
    //            return BadRequest(new { message = "Consultantname cannot be empty" });
    //        }

    //        var user = await _userService.GetByConsultantnameAsync(username, ct);
            
    //        if (user == null)
    //        {
    //            return NotFound(new { message = $"Consultant with username '{username}' not found in database" });
    //        }

    //        return Ok(user);
    //    }
    //    catch (ValidationException ex)
    //    {
    //        _logger.LogWarning(ex, "Validation error for username {Username}", username);
    //        return BadRequest(new { message = ex.Message });
    //    }
    //    catch (InvalidOperationException ex)
    //    {
    //        _logger.LogError(ex, "Service error for username {Username}", username);
    //        return StatusCode(StatusCodes.Status503ServiceUnavailable, 
    //            new { message = "Backend service error", details = ex.Message });
    //    }
    //}
}
