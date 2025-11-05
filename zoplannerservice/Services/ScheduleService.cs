using zoplannerservice.Models;

namespace zoplannerservice.Services;

/// <summary>
/// Schedule service - inherits all CRUD operations from BaseService
/// Add schedule-specific business logic here
/// </summary>
public class ScheduleService : BaseService<Schedule>
{
    protected override string EntityName => "Schedule";
    protected override string ApiEndpoint => "schedules";

    public ScheduleService(ISpringApiClient springClient, ILogger<ScheduleService> logger)
        : base(springClient, logger)
    {
    }

    /// <summary>
    /// Override to add schedule-specific business logic
    /// Example: validate time slots, check conflicts, optimize scheduling
    /// </summary>
    protected override Schedule ApplyBusinessLogic(Schedule schedule)
    {
        // Add schedule-specific transformations here
        // For example: validate time ranges, check overlaps
        
        return schedule;
    }
}
