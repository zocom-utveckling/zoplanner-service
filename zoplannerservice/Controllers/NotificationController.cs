using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using zoplannerservice.Models;
using zoplannerservice.Services;

namespace zoplannerservice.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly NotificationService _notificationService;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(
        NotificationService notificationService,
        ILogger<NotificationController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    [HttpPost("send-new-assignment")]
    public async Task<IActionResult> Send([FromBody] NewAssignmentEvent @event)
    {
        var json = JsonSerializer.Serialize(
            @event,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

        _logger.LogInformation("NEW_ASSIGNMENT JSON payload: {Json}", json);

        _logger.LogInformation(
            "Received NEW_ASSIGNMENT notification request. TeacherId={TeacherId}, AssignmentId={AssignmentId}",
            @event.TeacherId,
            @event.AssignmentId);

        try
        {
            await _notificationService.SendEventAsync(@event);
            _logger.LogInformation("NEW_ASSIGNMENT event enqueued to SQS successfully.");
            return Ok(new { Message = "Event sent!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to enqueue NEW_ASSIGNMENT event to SQS. TeacherId={TeacherId}, AssignmentId={AssignmentId}",
                @event.TeacherId,
                @event.AssignmentId);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { Message = "Failed to send event." });
        }
    }

    [HttpPost("send-schedule-updated")]
    public async Task<IActionResult> SendScheduleUpdated([FromBody] ScheduleUpdatedEvent @event)
    {
        _logger.LogInformation(
            "Received SCHEDULE_UPDATED notification request. TeacherId={TeacherId}, TeacherEmail={TeacherEmail}, Preference={Preference}",
            @event.TeacherId,
            @event.TeacherEmail,
            @event.Preference);

        try
        {
            await _notificationService.SendEventAsync(@event);
            _logger.LogInformation("SCHEDULE_UPDATED event enqueued to SQS successfully.");
            return Ok(new { Message = "SCHEDULE_UPDATED event sent!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to enqueue SCHEDULE_UPDATED event to SQS. TeacherId={TeacherId}, TeacherEmail={TeacherEmail}",
                @event.TeacherId,
                @event.TeacherEmail);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { Message = "Failed to send event." });
        }
    }

    [HttpPost("send-direct-message")]
    public async Task<IActionResult> SendDirectMessage([FromBody] DirectMessageEvent @event)
    {
        _logger.LogInformation(
            "Received DIRECT_MESSAGE notification request. RecipientEmail={RecipientEmail}, Subject={Subject}",
            @event.RecipientEmail,
            @event.Subject);

        try
        {
            await _notificationService.SendEventAsync(@event);
            _logger.LogInformation("DIRECT_MESSAGE event enqueued to SQS successfully.");
            return Ok(new { Message = "DIRECT_MESSAGE event sent!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to enqueue DIRECT_MESSAGE event to SQS. RecipientEmail={RecipientEmail}, Subject={Subject}",
                @event.RecipientEmail,
                @event.Subject);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { Message = "Failed to send event." });
        }
    }

    [HttpPost("send-schedule-calendar")]
    public async Task<IActionResult> SendScheduleCalendar([FromBody] ScheduleCalendarEvent @event)
    {
        _logger.LogInformation(
            "Received SCHEDULE_CALENDAR notification request. TeacherEmail={TeacherEmail}, TeacherName={TeacherName}, MonthTitle={MonthTitle}, WeekRange={WeekRange}",
            @event.TeacherEmail,
            @event.TeacherName,
            @event.MonthTitle,
            @event.WeekRange);

        try
        {
            await _notificationService.SendEventAsync(@event);
            _logger.LogInformation("SCHEDULE_CALENDAR event enqueued to SQS successfully.");
            return Ok(new { Message = "SCHEDULE_CALENDAR event sent!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to enqueue SCHEDULE_CALENDAR event to SQS. TeacherEmail={TeacherEmail}, TeacherName={TeacherName}",
                @event.TeacherEmail,
                @event.TeacherName);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { Message = "Failed to send event." });
        }
    }

    [HttpGet("receive")]
    public async Task<IActionResult> Receive()
    {
        var messages = await _notificationService.ReceiveAssignmentEventsAsync();
        return Ok(messages);
    }

}