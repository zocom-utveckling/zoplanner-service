using System.ComponentModel.DataAnnotations;
using zoplannerservice.Models;

namespace zoplannerservice.Services;

/// <summary>
/// Customer service with specific business logic
/// Inherits common CRUD operations from BaseService
/// </summary>
public class CustomerService : BaseService<Customer>
{
    protected override string EntityName => "Customer";
    protected override string ApiEndpoint => "customers";

    public CustomerService(ISpringApiClient springClient, ILogger<CustomerService> logger)
        : base(springClient, logger)
    {
    }

    /// <summary>
    /// Apply customer-specific business logic
    /// Override to add custom transformations or calculations
    /// </summary>
    protected override Customer ApplyBusinessLogic(Customer customer)
    {
        // Example: Normalize customer name
        if (!string.IsNullOrWhiteSpace(customer.Name))
        {
            customer.Name = customer.Name.Trim();
        }

        // Add more customer-specific logic here when requirements are defined
        // For example:
        // - Calculate customer tier/status
        // - Enrich with additional data
        // - Apply access control rules

        return customer;
    }

    /// <summary>
    /// Validate customer data
    /// Override when Create/Update methods are implemented
    /// </summary>
    protected override void ValidateEntity(Customer customer)
    {
        base.ValidateEntity(customer);

        // Add customer-specific validation here when needed
        // For example:
        // - Name length validation
        // - Email format validation
        // - Required fields validation
    }
}

