using Microsoft.AspNetCore.Mvc;
using zoplannerservice.Services;
using zoplannerservice.Models;
using System.ComponentModel.DataAnnotations;
using zoplannerservice.Exceptions;

namespace zoplannerservice.Controllers;

/// <summary>
/// Session controller - handles HTTP requests for sessions
/// Implements full CRUD operations + cancel/uncancel
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SessionController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly IAssignmentService _assignmentService;
    private readonly ILogger<SessionController> _logger;

    public SessionController(
        ISessionService sessionService,
        IAssignmentService assignmentService,
        ILogger<SessionController> logger)
    {
        _sessionService = sessionService;
        _assignmentService = assignmentService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new session for a specific assignment
    /// </summary>
    [HttpPost("{assignmentId}")]
    public async Task<ActionResult<Session>> CreateSession(
        long assignmentId,
        [FromBody] CreateSessionRequest request,
        CancellationToken ct)
    {
        try
        {
            var session = await _sessionService.CreateAsync(assignmentId, request, ct);

            if (session == null)
                throw new InvalidOperationException("Backend returned null when creating session");

            return CreatedAtAction(nameof(GetById), new { id = session.Id }, session);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session");
            return StatusCode(500, new { message = "Error creating session", details = ex.Message });
        }
    }

    /// <summary>
    /// Get all sessions
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var sessions = await _sessionService.GetAllSync(ct);
        return Ok(sessions);
    }

    /// <summary>
    /// Get session by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var session = await _sessionService.GetByIdAsync(id, ct);

        if (session == null)
            return NotFound(new { message = $"Session with ID {id} not found" });

        return Ok(session);
    }

    /// <summary>
    /// Update session
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] Session session, CancellationToken ct)
    {
        var updated = await _sessionService.UpdateAsync(id, session, ct);

        if (updated == null)
            return NotFound(new { message = $"Session with ID {id} not found" });

        return Ok(updated);
    }

    /// <summary>
    /// Delete session
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var deleted = await _sessionService.DeleteAsync(id, ct);

        if (!deleted)
            return NotFound(new { message = $"Session with ID {id} not found" });

        return NoContent();
    }

    /// <summary>
    /// Cancel a session (e.g. sick leave)
    /// POST /api/session/{id}/cancel
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(
        long id,
        [FromBody] CancelSessionRequest request,
        CancellationToken ct)
    {
        try
        {
            var updated = await _sessionService.CancelAsync(id, request, ct);
            return Ok(updated);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling session {Id}", id);
            return StatusCode(500, new { message = "Error cancelling session", details = ex.Message });
        }
    }

    /// <summary>
    /// Uncancel a session
    /// POST /api/session/{id}/uncancel
    /// </summary>
    [HttpPost("{id}/uncancel")]
    public async Task<IActionResult> Uncancel(long id, CancellationToken ct)
    {
        try
        {
            var updated = await _sessionService.UncancelAsync(id, ct);
            return Ok(updated);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uncancelling session {Id}", id);
            return StatusCode(500, new { message = "Error uncancelling session", details = ex.Message });
        }
    }
}
