using Microsoft.AspNetCore.Mvc;
using zoplannerservice.Models;
using zoplannerservice.Services;
using zoplannerservice.Services.Interfaces;

namespace zoplannerservice.Controllers;

[ApiController]
[Route("api/calendar")]
// [Authorize(Policy = "StaffOnly")] // Uncomment to apply your existing security
public class CalendarController : ControllerBase
{
    private readonly ICalendarService _calendarService;
    private readonly ILogger<CalendarController> _logger;

    public CalendarController(ICalendarService calendarService, ILogger<CalendarController> logger)
    {
        _calendarService = calendarService;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/calendar/events?startDate=...&endDate=...&userId=...
    /// </summary>
    [HttpGet("events")]
    public async Task<ActionResult<IEnumerable<Session>>> GetCalendarEvents(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] long? userId = null,
        CancellationToken ct = default)
    {
        var events = await _calendarService.GetCalendarEventsAsync(startDate, endDate, userId, ct);
        return Ok(events);
    }

    /// <summary>
    /// POST /api/calendar/events
    /// Mapped to Session create
    /// </summary>
    [HttpPost("events")]
    public async Task<ActionResult<Session>> CreateEvent([FromBody] Session session, CancellationToken ct = default)
    {
        var created = await _calendarService.CreateAsync(session, ct);
        return CreatedAtAction(nameof(GetCalendarEvents), new { userId = session.AssignmentId }, created);
    }

    /// <summary>
    /// PATCH /api/calendar/events/{eventId}
    /// Useful for partial updates like dragging an event to a new time
    /// </summary>
    [HttpPatch("events/{eventId}")]
    public async Task<ActionResult<Session>> PatchEvent(long eventId, [FromBody] object patchData, CancellationToken ct = default)
    {
        var updated = await _calendarService.PatchEventAsync(eventId, patchData, ct);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    /// <summary>
    /// DELETE /api/calendar/events/{eventId}
    /// </summary>
    [HttpDelete("events/{eventId}")]
    public async Task<IActionResult> DeleteEvent(int eventId, CancellationToken ct = default)
    {
        var deleted = await _calendarService.DeleteAsync(eventId, ct);
        if (!deleted) return NotFound();
        return NoContent();
    }
}