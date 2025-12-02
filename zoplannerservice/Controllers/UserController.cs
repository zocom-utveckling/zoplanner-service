using Microsoft.AspNetCore.Mvc;
using zoplannerservice.Services;
using zoplannerservice.Models;
using System.ComponentModel.DataAnnotations;

namespace zoplannerservice.Controllers;

/// <summary>
/// User controller - handles HTTP requests for users
/// Implements full CRUD operations: GET all, GET by ID, GET by username, POST, PUT, DELETE
/// </summary>

// TODO: Implement endpoint GetUsersByRole

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get all users
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<User>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        try
        {
            var users = await _userService.GetAllAsync(ct);
            return Ok(users);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error while fetching all users");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                new { message = "Backend service error", details = ex.Message });
        }
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
    /// Create a new user
    /// Note: Spring Boot checks if username exists, but this requires custom service method
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(User), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        try
        {
            var createdUser = await _userService.CreateAsync(request, ct);
            
            if (createdUser == null)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                    new { message = "Failed to create user" });
            }

            return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while creating user");
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error while creating user");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                new { message = "Backend service error", details = ex.Message });
        }
    }

    /// <summary>
    /// Update user by ID
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Update(int id, [FromBody] User user, CancellationToken ct)
    {
        try
        {
            var updatedUser = await _userService.UpdateAsync(id, user, ct);
            
            if (updatedUser == null)
            {
                return NotFound(new { message = $"User with ID {id} not found in database" });
            }

            return Ok(updatedUser);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while updating user {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error while updating user {Id}", id);
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


    /// <summary>
    /// Get user by username
    /// Note: This requires a custom endpoint in Spring Boot: GET /users/username/{username}
    /// </summary>
    [HttpGet("username/{username}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetByUsername(string username, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest(new { message = "Username cannot be empty" });
            }

            var user = await _userService.GetByUsernameAsync(username, ct);
            
            if (user == null)
            {
                return NotFound(new { message = $"User with username '{username}' not found in database" });
            }

            return Ok(user);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error for username {Username}", username);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error for username {Username}", username);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                new { message = "Backend service error", details = ex.Message });
        }
    }
}
