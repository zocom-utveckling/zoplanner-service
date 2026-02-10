using zoplannerservice.Models;

namespace zoplannerservice.Services.Interfaces;

public interface ICalendarService : IBaseService<Session>
{
    Task<IEnumerable<Session>> GetCalendarEventsAsync(DateTime startDate, DateTime endDate, long? userId = null, CancellationToken ct = default);
    Task<Session?> PatchEventAsync(long eventId, object patchRequest, CancellationToken ct = default);
}