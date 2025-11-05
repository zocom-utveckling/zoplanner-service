using zoplannerservice.Models;

namespace zoplannerservice.Services;

/// <summary>
/// Class service - inherits all CRUD operations from BaseService
/// Add class-specific business logic here
/// </summary>
public class ClassService : BaseService<Class>
{
    protected override string EntityName => "Class";
    protected override string ApiEndpoint => "classes";

    public ClassService(ISpringApiClient springClient, ILogger<ClassService> logger)
        : base(springClient, logger)
    {
    }

    /// <summary>
    /// Override to add class-specific business logic
    /// Example: validate capacity, check prerequisites, manage enrollment
    /// </summary>
    protected override Class ApplyBusinessLogic(Class classEntity)
    {
        // Add class-specific transformations here
        // For example: calculate available seats, check prerequisites
        
        return classEntity;
    }
}
