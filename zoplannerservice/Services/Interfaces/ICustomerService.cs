using zoplannerservice.Models;
using System.Threading;
using System.Threading.Tasks;

namespace zoplannerservice.Services;

/// <summary>
/// Specific customer service interface exposing extra methods not present in generic base.
/// </summary>
public interface ICustomerService : IBaseService<Customer>
{
    Task<IEnumerable<Customer>> GetAllAsync(CancellationToken ct);
    Task<Customer?> GetByIdAsync(long id, CancellationToken ct);
    Task<Customer> CreateAsync(CreateCustomerRequest request, CancellationToken ct);
    Task<Customer?> UpdateAsync(long id, UpdateCustomerRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(long id, CancellationToken ct);
}
