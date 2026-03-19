using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using zoplannerservice.Models;
using zoplannerservice.Services.Interfaces;


namespace zoplannerservice.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActivitiesController : ControllerBase
{
    private readonly IActivityService _activityService;

    public ActivitiesController(IActivityService activityService)
    {
        _activityService = activityService;
    }

    // GET /api/activities

    [HttpGet]
    public async Task<IEnumerable<PlannerActivity>> GetAll(CancellationToken ct)
        => await _activityService.GetAllSync(ct);

    // POST /api/activities

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PlannerActivity activity)
    {
        var created = await _activityService.CreateAsync(activity);
        return Ok(created);
    }

    // Update /api/activities/{id}

    [HttpPatch("{id:long}")]
    public async Task<IActionResult> Patch(long id, [FromBody] PlannerActivity activity)
    {
        var updated = await _activityService.UpdateAsync(id, activity);
        return Ok(updated);
    }

    // DELETE /api/activities/{id}

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var success = await _activityService.DeleteAsync(id);
        if (!success) return NotFound();
        return Ok();
    }
}