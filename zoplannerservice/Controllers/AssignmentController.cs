using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zoplannerservice.Services;
using zoplannerservice.Models;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using zoplannerservice.Services;

namespace zoplannerservice.Controllers;

/// <summary>
/// Assignment controller - handles HTTP requests for assignments
/// All CRUD operations are supported
/// </summary>

//TODO: Implement endpoint that retrieves active assignments between week x to y

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AllUsers")]
public class AssignmentController : ControllerBase
{
    private readonly IAssignmentService _assignmentService;
    private readonly ISpringApiClient _springClient;
    private readonly ILogger<AssignmentController> _logger;
    private readonly IConsultantService _consultantService;

    public AssignmentController(
    IAssignmentService assignmentService,
    IConsultantService consultantService,
    ISpringApiClient springClient,
    ILogger<AssignmentController> logger)
    {
        _assignmentService = assignmentService;
        _consultantService = consultantService;
        _springClient = springClient;
        _logger = logger;
    }

    /// <summary>
    /// Get all assignments
    /// GET /api/Assignment
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Assignment>>> GetAll(CancellationToken ct)
    {
        try
        {
            if (User.IsInRole("MANAGER"))
            {
                var allAssignments = await _assignmentService.GetAllSync(ct);
                return Ok(allAssignments);
            }

            var userId = long.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            var consultant = await _consultantService
                .GetByUserIdAsync(userId, ct);

            if (consultant == null)
            {
                return Ok(Enumerable.Empty<Assignment>());
            }

            var assignments = await _assignmentService
                .GetAssignmentsByConsultantAsync(
                    consultant.Id,
                    null,
                    ct
                );

            return Ok(assignments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error while fetching assignments");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message = "Unexpected error occurred",
                    details = ex.Message
                });
        }
    }

    /// <summary>
    /// Get Assignment by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Assignment), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

            if (User.IsInRole("MANAGER"))
            {
                return Ok(assignment);
            }

            var userId = long.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            var consultant = await _consultantService
                .GetByUserIdAsync(userId, ct);

            if (consultant == null)
            {
                return Forbid();
            }

            if (assignment.ConsultantId != consultant.Id)
            {
                return Forbid();
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
    [Authorize(Policy = "ManagerOnly")]
    [HttpGet("consultant/{consultantId}")]
    [ProducesResponseType(typeof(IEnumerable<Assignment>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Assignment>>> GetByConsultantId(
    long consultantId,
    [FromQuery] bool? published,
    CancellationToken ct)
    {
        try
        {
            var assignments = await _assignmentService
                .GetAssignmentsByConsultantAsync(consultantId, published, ct);

            return Ok(assignments);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex,
                "Validation error for consultant {ConsultantId}",
                consultantId);

            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex,
                "Service error for consultant {ConsultantId}",
                consultantId);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message = "Error getting assignments by consultant",
                    details = ex.Message
                });
        }
    }

    [HttpGet("visibility")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(IEnumerable<Assignment>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Assignment>>> GetByVisibility(
        [FromQuery] bool published,
        CancellationToken ct)
    {
        try
        {
            var assignments = await _assignmentService.GetAssignmentsByVisibilityAsync(published, ct);
            return Ok(assignments);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while getting assignments by visibility");
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error while getting assignments by visibility");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Error getting assignments by visibility", details = ex.Message });
        }
    }

    /// <summary>
    /// Create a new assignment
    /// </summary>
    [Authorize(Policy = "ManagerOnly")]
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
    [Authorize(Policy = "ManagerOnly")]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Assignment), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateAssignmentRequest request, CancellationToken ct)
    {
        try
        {
            var assignment = await _assignmentService.GetByIdAsync((int)id, ct);
            if (assignment == null)
            {
                return NotFound(new { message = $"Assignment with ID {id} not found" });
            }

            assignment.ConsultantId = request.ConsultantId;
            assignment.ManagerId = request.ManagerId;
            assignment.DateStart = request.DateStart;
            assignment.DateEnd = request.DateEnd;
            assignment.CourseId = assignment.Course.Id;

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

    /// <summary>
    /// Delete assignment by ID
    /// </summary>
    [Authorize(Policy = "ManagerOnly")]
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