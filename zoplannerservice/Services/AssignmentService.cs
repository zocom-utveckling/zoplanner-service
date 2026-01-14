using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using zoplannerservice.Models;

namespace zoplannerservice.Services;

/// <summary>
/// Assignment service - inherits all CRUD operations from BaseService
/// Add assignment-specific business logic here
/// </summary>

//TODO: Implement method that retrieves active assignments between week x to y


public class AssignmentService : BaseService<Assignment>, IAssignmentService
{
    protected override string EntityName => "Assignment";
    protected override string ApiEndpoint => "assignments";

    public AssignmentService(ISpringApiClient springClient, ILogger<AssignmentService> logger)
        : base(springClient, logger)
    {
    }

    public async Task<Assignment> CreateAsync(CreateAssignmentRequest request, CancellationToken ct = default)
    {
        if (request == null)
        {
            throw new ValidationException("Assignment data is required");
        }

        if (request.DateStart == null)
        {
            throw new ValidationException("DateStart is required");
        }
        if (request.DateEnd == null)
        {
            throw new ValidationException("DateEnd is required");
        }

        try
        {
            // Send only the fields without ID to Spring Boot
            var created = await _springClient.PostAsync<CreateAssignmentRequest, Assignment>(ApiEndpoint, request, ct);
            if (created == null)
            {
                throw new InvalidOperationException("Backend returned null when creating Assignment");
            }
            created.CourseId = (int)request.CourseId;
            return ApplyBusinessLogic(created);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Spring Boot error while creating {EntityName}", EntityName);
            throw new InvalidOperationException($"Spring Boot error while creating {EntityName}: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating {EntityName}", EntityName);
            throw new InvalidOperationException($"Failed to create {EntityName}", ex);
        }
    }




    public async Task<IEnumerable<Assignment>> GetByConsultantIdAsync(long consultantId, CancellationToken ct = default)
    {
        if (consultantId < 0)
        {
            throw new ValidationException("Consultant ID must be greater than 0");
        }

        try
        {
            var assignments = await _springClient.GetAsync<IEnumerable<Assignment>>($"{ApiEndpoint}/consultant/{consultantId}", ct);
            return assignments?.Select(a => ApplyBusinessLogic(a)) ?? Enumerable.Empty<Assignment>();
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            _logger.LogWarning("No assignments found for consultant {ConsultantId}", consultantId);
            return Enumerable.Empty<Assignment>();
        }
    }


    public async Task<Assignment?> PatchAsync(long id, PatchAssignmentRequest request, CancellationToken ct = default)
    {
        if (id <= 0)
            throw new ValidationException("Assignment ID must be greater than 0");

        if (request == null)
            throw new ValidationException("Patch data is required");

        // Ensure at least one field is provided
        if (request.ConsultantId is null && request.DateStart is null && request.DateEnd is null)
        {
            throw new ValidationException("At least one field must be provided for patch");
        }

        try
        {
            // Get existing assignment from Java API
            var existing = await GetByIdAsync((int)id, ct);
            if (existing == null)
            {
                return null;
            }

            bool hasChanges = false;



            if (request.ConsultantId.HasValue && existing.ConsultantId != request.ConsultantId.Value)
            {
                existing.ConsultantId = request.ConsultantId.Value;
                hasChanges = true;
            }

            if (request.DateStart.HasValue && existing.DateStart != request.DateStart.Value)
            {
                existing.DateStart = request.DateStart.Value;
                hasChanges = true;
            }

            if (request.DateEnd.HasValue && existing.DateEnd != request.DateEnd.Value)
            {
                existing.DateEnd = request.DateEnd.Value;
                hasChanges = true;
            }



            // Validate dates after merge
            if (existing.DateStart.HasValue && existing.DateEnd.HasValue && existing.DateEnd.Value < existing.DateStart.Value)
            {
                throw new ValidationException("End date must be after start date");
            }

            // If no actual changes, return existing
            if (!hasChanges)
            {
                _logger.LogInformation("No changes detected for {EntityName} {Id}", EntityName, id);
                return existing;
            }

            // Step 3: Use PUT to send complete updated entity to Java API
            var updated = await _springClient.PatchAsync<Assignment, Assignment>($"{ApiEndpoint}/{id}", existing, ct);

            if (updated == null)
            {
                throw new InvalidOperationException($"Backend returned null when updating {EntityName} {id}");
            }

            _logger.LogInformation("Successfully patched {EntityName} {Id}", EntityName, id);
            return ApplyBusinessLogic(updated);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Spring Boot error while patching {EntityName} with ID {Id}", EntityName, id);
            throw new InvalidOperationException($"Spring Boot error while patching {EntityName} with ID {id}: {ex.Message}", ex);
        }
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

    protected override void ValidateEntity(Assignment assignment)
    {
        base.ValidateEntity(assignment);
        if (assignment.DateStart == default)
            throw new ValidationException("Start date is required");
        if (assignment.DateEnd == default)
            throw new ValidationException("End date is required");
        if (assignment.DateEnd < assignment.DateStart)
            throw new ValidationException("End date must be after start date");
    }
}

