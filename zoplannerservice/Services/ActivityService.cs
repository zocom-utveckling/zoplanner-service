using System.ComponentModel.DataAnnotations;
using zoplannerservice.Models;
using zoplannerservice.Services.Interfaces;


namespace zoplannerservice.Services;

public class ActivityService : BaseService<PlannerActivity>, IActivityService
{
    protected override string EntityName => "Activity";
    protected override string ApiEndpoint => "activities";

    public ActivityService(ISpringApiClient springClient, ILogger<ActivityService> logger)
        : base(springClient, logger)
    {
    }

    public async Task<IEnumerable<PlannerActivity>> GetByDateAsync(DateTime date, CancellationToken ct = default)
    {
        var activities = await _springClient.GetAsync<IEnumerable<PlannerActivity>>(
            $"{ApiEndpoint}/date/{date:yyyy-MM-dd}", ct);

        return activities ?? Enumerable.Empty<PlannerActivity>();
    }

    // POST create activity
    public async Task<PlannerActivity> CreateAsync(PlannerActivity activity)
    {
        ValidateEntity(activity);
        var created = await _springClient.PostAsync<PlannerActivity, PlannerActivity>(ApiEndpoint, activity);
        return created!;
    }
    public async Task<PlannerActivity> UpdateAsync(long id, PlannerActivity activity, CancellationToken ct = default)
    {
        var endpoint = $"activities/{id}";
        return await _springClient.PatchAsync<PlannerActivity, PlannerActivity>(endpoint, activity, ct)
               ?? throw new InvalidOperationException("Failed to update activity");
    }

    // DELETE activity
    public async Task<bool> DeleteAsync(long id)
    {
        return await _springClient.DeleteAsync($"{ApiEndpoint}/{id}");
    }

    protected override void ValidateEntity(PlannerActivity activity)
    {
        base.ValidateEntity(activity);

        if (activity.EndTime <= activity.StartTime)
            throw new ValidationException("EndTime must be after StartTime");

        if (string.IsNullOrWhiteSpace(activity.Title))
            throw new ValidationException("Title is required");
    }
}

    





    