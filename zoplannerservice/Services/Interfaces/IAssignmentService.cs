using zoplannerservice.Models;

namespace zoplannerservice.Services;

public interface IAssignmentService : IBaseService<Assignment>
{
    Task<Assignment> CreateAsync(CreateAssignmentRequest request, CancellationToken ct = default);
     
    Task<IEnumerable<Assignment>> GetByClassIdAsync(long classId, CancellationToken ct = default);
    Task<IEnumerable<Assignment>> GetByConsultantIdAsync(long consultantId, CancellationToken ct = default);
    Task<Assignment?> PatchAsync(long id, PatchAssignmentRequest request, CancellationToken ct = default);
}