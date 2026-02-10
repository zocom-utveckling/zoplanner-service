using zoplannerservice.Models;
using zoplannerservice.Services.Interfaces;

namespace zoplannerservice.Services;

public class CalendarService : ICalendarService
{
    private readonly ISpringApiClient _apiClient;
    private const string BasePath = "api/calendar/events";

    public CalendarService(ISpringApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IEnumerable<Session>> GetCalendarEventsAsync(DateTime startDate, DateTime endDate, long? userId = null, CancellationToken ct = default)
    {
        // Using ISO 8601 format (yyyy-MM-ddTHH:mm:ss) for Spring compatibility
        var endpoint = $"{BasePath}?startDate={startDate:s}&endDate={endDate:s}";
        if (userId.HasValue) endpoint += $"&userId={userId}";

        var results = await _apiClient.GetAsync<IEnumerable<Session>>(endpoint, ct);
        return results ?? Enumerable.Empty<Session>();
    }

    public async Task<Session?> PatchEventAsync(long eventId, object patchRequest, CancellationToken ct = default)
    {
        return await _apiClient.PatchAsync<object, Session>($"{BasePath}/{eventId}", patchRequest, ct);
    }

    public async Task<Session?> GetByIdAsync(long id, CancellationToken ct = default)
        => await _apiClient.GetAsync<Session>($"{BasePath}/{id}", ct);

    public async Task<IEnumerable<Session>> GetAllSync(CancellationToken ct = default)
        => await _apiClient.GetAsync<IEnumerable<Session>>(BasePath, ct) ?? Enumerable.Empty<Session>();

    public async Task<Session> CreateAsync(Session entity, CancellationToken ct = default)
        => await _apiClient.PostAsync<Session, Session>(BasePath, entity, ct)
           ?? throw new HttpRequestException("Failed to create session event.");

    public async Task<Session?> UpdateAsync(int id, Session entity, CancellationToken ct = default)
        => await _apiClient.PutAsync<Session, Session>($"{BasePath}/{id}", entity, ct);

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        => await _apiClient.DeleteAsync($"{BasePath}/{id}", ct);
}