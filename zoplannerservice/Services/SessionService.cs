using System.ComponentModel.DataAnnotations;
using zoplannerservice.Exceptions;
using zoplannerservice.Models;

namespace zoplannerservice.Services;

/// <summary>
/// Session service - inherits all CRUD operations from BaseService
/// Add session-specific business logic here
/// </summary>
public class SessionService : BaseService<Session>, ISessionService
{
    protected override string EntityName => "Session";
    protected override string ApiEndpoint => "sessions";

    private readonly IAssignmentService _assignmentService;

    public SessionService(
        ISpringApiClient springClient,
        ILogger<SessionService> logger,
        IAssignmentService assignmentService)
        : base(springClient, logger)
    {
        _assignmentService = assignmentService;
    }

    public async Task<Session?> CreateAsync(long assignmentId, CreateSessionRequest request, CancellationToken ct = default)
    {
        var assignment = await _assignmentService.GetByIdAsync(assignmentId, ct);

        if (assignment == null)
        {
            throw new NotFoundException($"Assignment with ID {assignmentId} not found");
        }

        if (request == null)
        {
            throw new ValidationException("Assignment data is required");
        }

        var created = await _springClient.PostAsync<CreateSessionRequest, Session>(
            $"{ApiEndpoint}/{assignmentId}", request, ct);

        if (created == null)
        {
            throw new InvalidOperationException("Backend returned null when creating session");
        }

        return ApplyBusinessLogic(created);
    }

    public async Task<Session> CancelAsync(long sessionId, CancelSessionRequest request, CancellationToken ct = default)
    {
        if (request == null)
        {
            throw new ValidationException("Cancel data is required");
        }

        var updated = await _springClient.PostAsync<CancelSessionRequest, Session>(
            $"{ApiEndpoint}/{sessionId}/cancel", request, ct);

        if (updated == null)
        {
            throw new InvalidOperationException("Backend returned null when cancelling session");
        }

        return ApplyBusinessLogic(updated);
    }

    public async Task<Session> UncancelAsync(long sessionId, CancellationToken ct = default)
    {
        var updated = await _springClient.PostAsync<Session>(
            $"{ApiEndpoint}/{sessionId}/uncancel", ct);

        if (updated == null)
        {
            throw new InvalidOperationException("Backend returned null when uncancelling session");
        }

        return ApplyBusinessLogic(updated);
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
