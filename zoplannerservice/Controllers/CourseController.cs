using Microsoft.AspNetCore.Mvc;
using zoplannerservice.Services;
using zoplannerservice.Models;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace zoplannerservice.Controllers;

/// <summary>
/// Course controller - handles HTTP requests for courses
/// All CRUD operations are supported
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CourseController : ControllerBase
{
    private readonly ICourseService _courseService;
    private readonly ISpringApiClient _springClient;
    private readonly ILogger<CourseController> _logger;

    public CourseController(ICourseService courseService, ISpringApiClient springClient, ILogger<CourseController> logger)
    {
        _courseService = courseService;
        _springClient = springClient;
        _logger = logger;
    }


    /// <summary>
    /// Get all courses
    /// GET /api/course
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Course>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Course>>> GetAll(CancellationToken ct)
    {
        try
        {
        var courses = await _courseService.GetAllAsync(ct);
        return Ok(courses);
       }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching all courses");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Unexpected error occurred", details = ex.Message });
        }
    }

    /// <summary>
    /// Get course by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Course), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        try
        {
            var course = await _courseService.GetByIdAsync((int)id, ct);

            if (course == null)
            {
                return NotFound(new { message = $"Course with ID {id} not found in database" });
            }

            return Ok(course);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error for course {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error for course {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Error getting course", details = ex.Message });
        }
    }

    
    
 
    
    /// <summary>
    /// Get courses by Class ID
    /// </summary>

    [HttpGet("class/{classId}")]
    [ProducesResponseType(typeof(IEnumerable<Course>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Course>>> GetByClassId(long classId, CancellationToken ct)
    {
        try
        {
            var courses = await _courseService.GetByClassIdAsync(classId, ct);
            return Ok(courses);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error for class {ClassId}", classId);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error for class {ClassId}", classId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Error getting courses by class", details = ex.Message });
        }
    }

   

    /// <summary>
    /// Create a new course
    /// </summary>

    [HttpPost]
    [ProducesResponseType(typeof(Course), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Course>> Create([FromBody] CreateCourseRequest request, CancellationToken ct)
    {
        try
        {
            var createdCourse = await _courseService.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = createdCourse.Id }, createdCourse);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while creating course");
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error while creating course");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Failed to create course", details = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing course
    /// </summary>

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Course), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateCourseRequest request, CancellationToken ct)
    {
        try
        {
            var course = new Course
            {
                Id = id,
                DateStart = request.DateStart,
                DateEnd = request.DateEnd,
            };
            var updated = await _courseService.UpdateAsync((int)id, course, ct);
            if (updated == null)
            {
                return NotFound(new { message = $"Course with ID {id} not found in database" });
            }
            return Ok(updated);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while updating course {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error while updating course {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Failed to update course", details = ex.Message });
        }
    }

    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(Course), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Course>> Patch(long id, [FromBody] PatchCourseRequest request, CancellationToken ct)
    {
        try
        {
            var updated = await _courseService.PatchAsync(id, request, ct);
            if (updated == null)
            {
                return NotFound(new { message = $"Course with ID {id} not found" });
            }
            return Ok(updated);
        }
       
       catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error patching course {Id}", id);
            return StatusCode(500, new { message = "Error patching course", details = ex.Message });
        }
    }


    /// <summary>
    /// Delete course by ID
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
            var deleted = await _courseService.DeleteAsync(id, ct);
            
            if (!deleted)
            {
                return NotFound(new { message = $"Course with ID {id} not found in database" });
            }

            return NoContent();
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error while deleting course {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error while deleting course {Id}", id);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                new { message = "Backend service error", details = ex.Message });
        }
    }
}
