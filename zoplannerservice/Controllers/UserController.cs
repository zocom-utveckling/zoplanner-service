using Microsoft.AspNetCore.Mvc;
using zoplannerservice.Services;
using zoplannerservice.Models;
using System.ComponentModel.DataAnnotations;

namespace zoplannerservice.Controllers;

/// <summary>
/// User controller - handles HTTP requests for users
/// Only GET by ID and DELETE operations are implemented
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IBaseService<User> _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IBaseService<User> userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        try
        {
            var user = await _userService.GetByIdAsync(id, ct);
            
            if (user == null)
            {
                return NotFound(new { message = $"User with ID {id} not found in database" });
            }

            return Ok(user);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error for user {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Timeout while fetching user {Id}", id);
            return StatusCode(StatusCodes.Status504GatewayTimeout, 
                new { message = "Request timed out", details = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error for user {Id}", id);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                new { message = "Backend service error", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete user by ID
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
            var deleted = await _userService.DeleteAsync(id, ct);
            
            if (!deleted)
            {
                return NotFound(new { message = $"User with ID {id} not found in database" });
            }

            return NoContent();
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while deleting user {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error while deleting user {Id}", id);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                new { message = "Backend service error", details = ex.Message });
        }
    }
}
