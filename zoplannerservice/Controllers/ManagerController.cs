using Microsoft.AspNetCore.Mvc;
using zoplannerservice.Services;
using zoplannerservice.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;

namespace zoplannerservice.Controllers;

/// <summary>
/// Manager controller - handles HTTP requests for managers
/// Implements full CRUD operations: GET all, GET by ID, GET by username, POST, PUT, DELETE
/// </summary>


//ENDPOINTS FROM SPRING BOOT TO IMPLEMENT

//Description                    | Spring Boot endpoint
//ADD consultant to manager      | PUT     /api/{managerId}/consultants/{consultantId}
//ADD customer to manager        | PUT     /api/{managerId}/consultants/{customerId}
//REMOVE consultant from manager | DELETE  /api/{managerId}/consultants/{consultantId}
//REMOVE customer from manager   | DELETE  /api/{managerId}/consultants/{customerId}
//GET associated consultants     | GET     /api/{id}/consultants
//GET associated customers       | GET     /api/{id}/customers



[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "ManagerOnly")]

public class ManagerController : ControllerBase
{
    private readonly IManagerService _managerService;
    private readonly ILogger<ManagerController> _logger;

    public ManagerController(IManagerService managerService, ILogger<ManagerController> logger)
    {
        _managerService = managerService;
        _logger = logger;
    }

    /// <summary>
    /// Get all managers
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Manager>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        try
        {
            var managers = await _managerService.GetAllSync(ct);
            return Ok(managers);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error while fetching all managers");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                new { message = "Backend service error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get manager by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Manager), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        try
        {
            var manager = await _managerService.GetByIdAsync(id, ct);
            
            if (manager == null)
            {
                return NotFound(new { message = $"Manager with ID {id} not found in database" });
            }

            return Ok(manager);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error for manager {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Timeout while fetching manager {Id}", id);
            return StatusCode(StatusCodes.Status504GatewayTimeout, 
                new { message = "Request timed out", details = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error for manager {Id}", id);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                new { message = "Backend service error", details = ex.Message });
        }
    }

    /// <summary>
    /// Create a new manager
    /// Note: Spring Boot checks if managername exists, but this requires custom service method
    /// </summary>
    [HttpPost("{userId}")]
    [ProducesResponseType(typeof(Manager), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Create(int userId, CancellationToken ct)
    {
        try
        {
            var createdManager = await _managerService.CreateAsync(userId, ct);
            
            if (createdManager == null)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                    new { message = "Failed to create manager" });
            }

            return CreatedAtAction(nameof(GetById), new { id = createdManager.Id }, createdManager);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while creating manager");
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error while creating manager");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                new { message = "Backend service error", details = ex.Message });
        }
    }


 

    /// <summary>
    /// Delete manager by ID
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
            var deleted = await _managerService.DeleteAsync(id, ct);
            
            if (!deleted)
            {
                return NotFound(new { message = $"Manager with ID {id} not found in database" });
            }

            return NoContent();
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while deleting manager {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error while deleting manager {Id}", id);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                new { message = "Backend service error", details = ex.Message });
        }
    }


    ///// <summary>
    ///// Get user by username
    ///// Note: This requires a custom endpoint in Spring Boot: GET /users/username/{username}
    ///// </summary>
    //[HttpGet("username/{username}")]
    //[ProducesResponseType(typeof(Manager), StatusCodes.Status200OK)]
    //[ProducesResponseType(StatusCodes.Status404NotFound)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    //public async Task<IActionResult> GetByManagername(string username, CancellationToken ct)
    //{
    //    try
    //    {
    //        if (string.IsNullOrWhiteSpace(username))
    //        {
    //            return BadRequest(new { message = "Managername cannot be empty" });
    //        }

    //        var user = await _userService.GetByManagernameAsync(username, ct);
            
    //        if (user == null)
    //        {
    //            return NotFound(new { message = $"Manager with username '{username}' not found in database" });
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
