using System.ComponentModel.DataAnnotations;
using zoplannerservice.Models;

namespace zoplannerservice.Services;

/// <summary>
/// Customer service with specific business logic
/// Inherits common CRUD operations from BaseService
/// </summary>
public class CustomerService : BaseService<Customer>, ICustomerService
{
    protected override string EntityName => "Customer";
    protected override string ApiEndpoint => "customers";

    public CustomerService(ISpringApiClient springClient, ILogger<CustomerService> logger)
        : base(springClient, logger)
    {
    }

    public async Task<IEnumerable<Customer>> GetAllAsync(CancellationToken ct)
    {
        return await _springClient.GetAsync<IEnumerable<Customer>>("customers", ct)
               ?? Enumerable.Empty<Customer>();
    }

    public async Task<Customer?> GetByIdAsync(long id, CancellationToken ct)
    {
        return await _springClient.GetAsync<Customer>($"customers/{id}", ct);
    }

    public async Task<Customer> CreateAsync(CreateCustomerRequest request, CancellationToken ct)
    {
        return await _springClient.PostAsync<CreateCustomerRequest, Customer>("customers", request, ct)
               ?? throw new Exception("Failed to create customer");
    }

    public async Task<Customer?> UpdateAsync(long id, UpdateCustomerRequest request, CancellationToken ct)
    {
        return await _springClient.PatchAsync<UpdateCustomerRequest, Customer>($"customers/{id}", request, ct);
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken ct)
    {
        return await _springClient.DeleteAsync($"customers/{id}", ct);
    }
}

