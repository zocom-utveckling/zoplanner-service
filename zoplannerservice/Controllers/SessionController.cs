using Microsoft.AspNetCore.Mvc;
using zoplannerservice.Services;
using zoplannerservice.Models;
using System.ComponentModel.DataAnnotations;
using zoplannerservice.Exceptions;

namespace zoplannerservice.Controllers;

/// <summary>
/// Session controller - handles HTTP requests for sessions
/// Implements full CRUD operations: GET all, GET by ID, PUT, DELETE
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SessionController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly IAssignmentService _assignmentService;

    private readonly ILogger<SessionController> _logger;

    public SessionController(ISessionService sessionService, IAssignmentService assignmentService, ILogger<SessionController> logger)
    {
        _sessionService = sessionService;
        _assignmentService = assignmentService;

        _logger = logger;
    }

    /// <summary>
    /// Create a new session for a specific assignment
    /// POST /api/assignment/{id}/sessions
    /// </summary>
    [HttpPost("{assignmentId}")]
    [ProducesResponseType(typeof(Session), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Session>> CreateSession(long assignmentId, [FromBody] CreateSessionRequest request, CancellationToken ct)
    {
        try
        {
            var assignment = await _assignmentService.GetByIdAsync((int)assignmentId, ct);

            //if (assignment == null)
            //{
            //    return NotFound(new { message = $"Assignment with ID {assignmentId} not found" });
            //}

            // Send CreateSessionRequest to Java API
            var session = await _sessionService.CreateAsync(assignmentId, request, ct);

            if (session == null)
            {
                throw new InvalidOperationException("Backend returned null when creating session");
            }

            return CreatedAtAction(nameof(GetById), new { id = session.Id }, session);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (NotFoundException ex)
        {
            _logger.LogError(ex, "Error creating session for assignment {Id}", assignmentId);
            return StatusCode(404, new { message = "Error creating session", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session for assignment {Id}", assignmentId);
            return StatusCode(500, new { message = "Error creating session", details = ex.Message });
        }
    }

    /// <summary>
    /// Get all sessions
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Session>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        try
        {
            var sessions = await _sessionService.GetAllSync(ct);
            return Ok(sessions);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error while fetching all sessions");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                new { message = "Backend service error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get session by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Session), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        try
        {
            var session = await _sessionService.GetByIdAsync(id, ct);
            
            if (session == null)
            {
                return NotFound(new { message = $"Session with ID {id} not found in database" });
            }

            return Ok(session);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error for session {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Timeout while fetching session {Id}", id);
            return StatusCode(StatusCodes.Status504GatewayTimeout, 
                new { message = "Request timed out", details = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error for session {Id}", id);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                new { message = "Backend service error", details = ex.Message });
        }
    }

    /// <summary>
    /// Update session by ID
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Session), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Update(int id, [FromBody] Session session, CancellationToken ct)
    {
        try
        {
            var updatedSession = await _sessionService.UpdateAsync(id, session, ct);
            
            if (updatedSession == null)
            {
                return NotFound(new { message = $"Session with ID {id} not found in database" });
            }

            return Ok(updatedSession);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while updating session {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error while updating session {Id}", id);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                new { message = "Backend service error", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete session by ID
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
            var deleted = await _sessionService.DeleteAsync(id, ct);
            
            if (!deleted)
            {
                return NotFound(new { message = $"Session with ID {id} not found in database" });
            }

            return NoContent();
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while deleting session {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error while deleting session {Id}", id);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                new { message = "Backend service error", details = ex.Message });
        }
    }
}
