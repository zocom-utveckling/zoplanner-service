using zoplannerservice.Models;

namespace zoplannerservice.Services;

public interface ISessionService : IBaseService<Session>
{
    Task<Session?> CreateAsync(long assignmentId, CreateSessionRequest entity, CancellationToken ct = default);

    Task<Session> CancelAsync(long sessionId, CancelSessionRequest request, CancellationToken ct = default);
    Task<Session> UncancelAsync(long sessionId, CancellationToken ct = default);
}
