using Microsoft.AspNetCore.Mvc;
using zoplannerservice.Services;
using zoplannerservice.Models;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace zoplannerservice.Controllers;

/// <summary>
/// Assignment controller - handles HTTP requests for assignments
/// All CRUD operations are supported
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AssignmentController : ControllerBase
{
    private readonly IAssignmentService _assignmentService;
    private readonly ISpringApiClient _springClient;
    private readonly ILogger<AssignmentController> _logger;

    public AssignmentController(IAssignmentService assignmentService, ISpringApiClient springClient, ILogger<AssignmentController> logger)
    {
        _assignmentService = assignmentService;
        _springClient = springClient;
        _logger = logger;
    }


    /// <summary>
    /// Get all assignments
    /// GET /api/assignment
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Assignment>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Assignment>>> GetAll(CancellationToken ct)
    {
        try
        {
        var assignments = await _assignmentService.GetAllAsync(ct);
        return Ok(assignments);
       }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching all assignments");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Unexpected error occurred", details = ex.Message });
        }
    }

    /// <summary>
    /// Get assignment by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Assignment), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        try
        {
            var assignment = await _assignmentService.GetByIdAsync((int)id, ct);

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
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error for assignment {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Error getting assignment", details = ex.Message });
        }
    }


    /// <summary>
    /// Get sessions for a specific assignment
    /// GET /api/assignment/{id}/sessions
    /// </summary>
    [HttpGet("{id}/sessions")]
    [ProducesResponseType(typeof(IEnumerable<Session>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]

    public async Task<ActionResult<IEnumerable<Session>>> GetSessionsByAssignmentId(long id, CancellationToken ct)
    {
        try
        {
            var assignment = await _assignmentService.GetByIdAsync((int)id, ct);
            if (assignment == null)
            {
                return NotFound(new { message = $"Assignment with ID {id} not found" });
            }

            var sessions = await _springClient.GetAsync<IEnumerable<Session>>($"assignments/{id}/sessions", ct);
            return Ok(sessions ?? Enumerable.Empty<Session>());
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error for assignment {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting sessions for assignment {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Error getting sessions for assignment", details = ex.Message });
        }
    }


    /// <summary>
    /// Create a new session for a specific assignment
    /// POST /api/assignment/{id}/sessions
    /// </summary>
    [HttpPost("{id}/sessions")]
    [ProducesResponseType(typeof(Session), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Session>> CreateSession(long id, [FromBody] CreateSessionRequest request, CancellationToken ct)
    {
        try
        {
            var assignment = await _assignmentService.GetByIdAsync((int)id, ct);
            if (assignment == null)
            {
                return NotFound(new { message = $"Assignment with ID {id} not found" });
            }

            // Send CreateSessionRequest to Java API
            var session = await _springClient.PostAsync<CreateSessionRequest, Session>(
                $"assignments/{id}/sessions", request, ct);

            if (session == null)
            {
                throw new InvalidOperationException("Backend returned null when creating session");
            }

            return CreatedAtAction(nameof(GetSessionsByAssignmentId), new { id = id }, session);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session for assignment {Id}", id);
            return StatusCode(500, new { message = "Error creating session", details = ex.Message });
        }
    }
 
    
    

    /// <summary>
    /// Get assignments by Consultant ID
    /// </summary>

    [HttpGet("consultant/{consultantId}")]
    [ProducesResponseType(typeof(IEnumerable<Assignment>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Assignment>>> GetByConsultantId(long consultantId, CancellationToken ct)
    {
        try
        {
            var assignments = await _assignmentService.GetByConsultantIdAsync(consultantId, ct);
            return Ok(assignments);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error for consultant {ConsultantId}", consultantId);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error for consultant {ConsultantId}", consultantId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Error getting assignments by consultant", details = ex.Message });
        }
    }

    /// <summary>
    /// Create a new assignment
    /// </summary>

    [HttpPost]
    [ProducesResponseType(typeof(Assignment), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Assignment>> Create([FromBody] CreateAssignmentRequest request, CancellationToken ct)
    {
        try
        {
            var createdAssignment = await _assignmentService.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = createdAssignment.Id }, createdAssignment);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while creating assignment");
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error while creating assignment");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Failed to create assignment", details = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing assignment
    /// </summary>

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Assignment), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateAssignmentRequest request, CancellationToken ct)
    {
        try
        {
            var assignment = new Assignment
            {
                Id = id,
                ConsultantId = request.ConsultantId,
                DateStart = request.DateStart,
                DateEnd = request.DateEnd,
            };
            var updated = await _assignmentService.UpdateAsync((int)id, assignment, ct);
            if (updated == null)
            {
                return NotFound(new { message = $"Assignment with ID {id} not found in database" });
            }
            return Ok(updated);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while updating assignment {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error while updating assignment {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Failed to update assignment", details = ex.Message });
        }
    }

    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(Assignment), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Assignment>> Patch(long id, [FromBody] PatchAssignmentRequest request, CancellationToken ct)
    {
        try
        {
            var updated = await _assignmentService.PatchAsync(id, request, ct);
            if (updated == null)
            {
                return NotFound(new { message = $"Assignment with ID {id} not found" });
            }
            return Ok(updated);
        }
       
       catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error patching assignment {Id}", id);
            return StatusCode(500, new { message = "Error patching assignment", details = ex.Message });
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
