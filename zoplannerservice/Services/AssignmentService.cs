using zoplannerservice.Models;

namespace zoplannerservice.Services;

/// <summary>
/// Assignment service - inherits all CRUD operations from BaseService
/// Add assignment-specific business logic here
/// </summary>
public class AssignmentService : BaseService<Assignment>
{
    protected override string EntityName => "Assignment";
    protected override string ApiEndpoint => "assignments";

    public AssignmentService(ISpringApiClient springClient, ILogger<AssignmentService> logger)
        : base(springClient, logger)
    {
    }

    /// <summary>
    /// Override to add assignment-specific business logic
    /// Example: validate dates, check conflicts, calculate status
    /// </summary>
    protected override Assignment ApplyBusinessLogic(Assignment assignment)
    {
        // Add assignment-specific transformations here
        // For example: calculate status, validate deadlines
        
        return assignment;
    }
}
