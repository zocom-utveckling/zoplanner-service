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

    [HttpGet("receive")]
    public async Task<IActionResult> Receive()
    {
        var messages = await _notificationService.ReceiveAssignmentEventsAsync();
        return Ok(messages);
    }

}