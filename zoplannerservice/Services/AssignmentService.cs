using System.ComponentModel.DataAnnotations;
using zoplannerservice.Models;

namespace zoplannerservice.Services;

/// <summary>
/// Assignment service - inherits all CRUD operations from BaseService
/// Add assignment-specific business logic here
/// </summary>
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
        if (string.IsNullOrWhiteSpace(request.CourseName))
        {
            throw new ValidationException("CourseName is required");
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

    public async Task<IEnumerable<Assignment>> GetByClassIdAsync(long classId, CancellationToken ct = default)
    {
        if (classId <= 0)
        {
            throw new ValidationException("Class ID must be greater than 0");
        }

        try
        {
            var assignments = await _springClient.GetAsync<IEnumerable<Assignment>>($"{ApiEndpoint}/class/{classId}", ct);
            return assignments?.Select(a => ApplyBusinessLogic(a)) ?? Enumerable.Empty<Assignment>();
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            _logger.LogWarning("No assignments found for class {ClassId}", classId);
            return Enumerable.Empty<Assignment>();
        }
    }


    public async Task<IEnumerable<Assignment>> GetByConsultantIdAsync(long consultantId, CancellationToken ct = default)
    {
        if (consultantId <= 0)
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

    // Check if no useful fields were provided
    bool noFieldsProvided =
        string.IsNullOrWhiteSpace(request.CourseName) &&
        request.ConsultantId is null &&
        request.DateStart == default &&
        request.DateEnd == default &&
        request.ClassId is null;

    if (noFieldsProvided)
        throw new ValidationException("At least one field must be provided for patch");

    try
    {
        // Load existing assignment from backend
        var existing = await GetByIdAsync((int)id, ct);
        if (existing == null)
            return null;

        bool hasChanges = false;

        // --- Apply individual field changes ---
        if (!string.IsNullOrWhiteSpace(request.CourseName) &&
            existing.CourseName != request.CourseName)
        {
            existing.CourseName = request.CourseName;
            hasChanges = true;
        }

        if (request.ConsultantId.HasValue &&
            existing.ConsultantId != request.ConsultantId.Value)
        {
            existing.ConsultantId = request.ConsultantId.Value;
            hasChanges = true;
        }

        if (request.DateStart != default &&
            existing.DateStart != request.DateStart)
        {
            existing.DateStart = request.DateStart;
            hasChanges = true;
        }

        if (request.DateEnd != default &&
            existing.DateEnd != request.DateEnd)
        {
            existing.DateEnd = request.DateEnd;
            hasChanges = true;
        }

        if (request.ClassId.HasValue &&
            existing.ClassId != request.ClassId.Value)
        {
            existing.ClassId = request.ClassId.Value;
            hasChanges = true;
        }

        // Validate updated date range
        if (existing.DateEnd < existing.DateStart)
            throw new ValidationException("End date must be after start date");

        // If nothing changed → return existing
        if (!hasChanges)
        {
            _logger.LogInformation("No changes detected for Assignment {Id}", id);
            return existing;
        }

        // Send updated assignment via PUT to Spring API
        var updated = await _springClient.PutAsync<Assignment, Assignment>(
            $"{ApiEndpoint}/{id}", existing, ct);

        if (updated == null)
            throw new InvalidOperationException($"Backend returned null when updating Assignment {id}");

        _logger.LogInformation("Successfully patched Assignment {Id}", id);
        return ApplyBusinessLogic(updated);
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "Spring Boot error while patching Assignment {Id}", id);
        throw new InvalidOperationException(
            $"Spring Boot error while patching Assignment {id}: {ex.Message}", ex);
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
        if (string.IsNullOrWhiteSpace(assignment.CourseName))
        
            throw new ValidationException("Course name is required");
        if (assignment.DateStart == default)
            throw new ValidationException("Start date is required");
        if (assignment.DateEnd == default)
            throw new ValidationException("End date is required");
        if (assignment.DateEnd < assignment.DateStart)
            throw new ValidationException("End date must be after start date");
    }
}

