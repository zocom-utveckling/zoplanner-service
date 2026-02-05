using zoplannerservice.Models;

namespace zoplannerservice.Services;

public interface ICourseService : IBaseService<Course>
{
    Task<Course> CreateAsync(CreateCourseRequest request, CancellationToken ct = default);

    Task<IEnumerable<Course>> GetByClassIdAsync(long classId, CancellationToken ct = default);
    Task<Course?> PatchAsync(long id, PatchCourseRequest request, CancellationToken ct = default);
}