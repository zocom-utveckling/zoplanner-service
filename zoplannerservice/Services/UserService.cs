using zoplannerservice.Models;

namespace zoplannerservice.Services;

/// <summary>
/// User service - handles user business logic
/// Only GET by ID and DELETE are implemented
/// </summary>
public class UserService : BaseService<User>
{
    protected override string EntityName => "User";
    protected override string ApiEndpoint => "users";

    public UserService(ISpringApiClient springClient, ILogger<UserService> logger)
        : base(springClient, logger)
    {
    }

    /// <summary>
    /// Apply user-specific business logic
    /// Add transformations when business requirements are defined
    /// </summary>
    protected override User ApplyBusinessLogic(User user)
    {
        // Add user-specific logic here when needed
        return user;
    }
}
