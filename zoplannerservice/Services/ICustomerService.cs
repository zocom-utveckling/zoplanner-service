using zoplannerservice.Models;
using System.Threading;
using System.Threading.Tasks;

namespace zoplannerservice.Services;

/// <summary>
/// Specific customer service interface exposing extra methods not present in generic base.
/// </summary>
public interface ICustomerService : IBaseService<Customer>
{
    Task<Customer> CreateAsync(CreateCustomerRequest request, CancellationToken ct = default);
    Task<Customer?> PatchAsync(int id, PatchCustomerRequest request, CancellationToken ct = default);
}
