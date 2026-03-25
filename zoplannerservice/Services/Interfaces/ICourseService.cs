using zoplannerservice.Models;

namespace zoplannerservice.Services;

public interface ICourseService : IBaseService<Course>
{
    Task<Course> CreateAsync(CreateCourseRequest request, CancellationToken ct = default);

    Task<IEnumerable<Course>> GetByClassIdAsync(long classId, CancellationToken ct = default);
    Task<Course?> PatchAsync(long id, PatchCourseRequest request, CancellationToken ct = default);
    
    /// <summary>
    /// Get courses filtered by userId and/or status
    /// </summary>
    Task<IEnumerable<Course>> GetByFilterAsync(long? userId, string? status, CancellationToken ct = default);
}