using zoplannerservice.Models;

namespace zoplannerservice.Services;

/// <summary>
/// Session service - inherits all CRUD operations from BaseService
/// Add session-specific business logic here
/// </summary>
public class SessionService : BaseService<Session>
{
    protected override string EntityName => "Session";
    protected override string ApiEndpoint => "sessions";

    public SessionService(ISpringApiClient springClient, ILogger<SessionService> logger)
        : base(springClient, logger)
    {
    }

    /// <summary>
    /// Override to add session-specific business logic
    /// Example: validate time slots, check conflicts, optimize scheduling
    /// </summary>
    protected override Session ApplyBusinessLogic(Session session)
    {
        // Add session-specific transformations here
        // For example: validate time ranges, check overlaps
        
        return session;
    }
}
