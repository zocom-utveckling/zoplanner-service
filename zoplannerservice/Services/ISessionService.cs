


using zoplannerservice.Models;

namespace zoplannerservice.Services;

public interface ISessionService : IBaseService<Session>
{
    Task<Session> CreateAsync(long assignmentId, CreateSessionRequest entity, CancellationToken ct = default);
}