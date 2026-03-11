using zoplannerservice.Models;


namespace zoplannerservice.Services.Interfaces;

public interface IActivityService : IBaseService<PlannerActivity>
{
    Task<IEnumerable<PlannerActivity>> GetByDateAsync(DateTime date, CancellationToken ct = default);
    Task<PlannerActivity> CreateAsync(PlannerActivity activity);
    Task<PlannerActivity> UpdateAsync(long id, PlannerActivity activity, CancellationToken ct = default);
    Task<bool> DeleteAsync(long id);
}
