using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

    public NotificationController(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] NewAssignmentEvent @event)
    {
        await _notificationService.SendEventAsync(@event);
        return Ok(new { Message = "Event sent!" });
    }

    [HttpPost("send-schedule-updated")]
    public async Task<IActionResult> SendScheduleUpdated([FromBody] ScheduleUpdatedEvent @event)
    {
        await _notificationService.SendEventAsync(@event);
        return Ok(new { Message = "SCHEDULE_UPDATED event sent!" });
    }

    [HttpPost("send-direct-message")]
    public async Task<IActionResult> SendDirectMessage([FromBody] DirectMessageEvent @event)
    {
        await _notificationService.SendEventAsync(@event);
        return Ok(new { Message = "DIRECT_MESSAGE event sent!" });
    }

    [HttpPost("send-schedule-calendar")]
    public async Task<IActionResult> SendScheduleCalendar([FromBody] ScheduleCalendarEvent @event)
    {
        await _notificationService.SendEventAsync(@event);
        return Ok(new { Message = "SCHEDULE_CALENDAR event sent!" });
    }

    [HttpGet("receive")]
    public async Task<IActionResult> Receive()
    {
        var messages = await _notificationService.ReceiveAssignmentEventsAsync();
        return Ok(messages);
    }

}