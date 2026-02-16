using System;
using System.ComponentModel.DataAnnotations;
using zoplannerservice.Models;

namespace zoplannerservice.Services;

/// <summary>
/// course service - inherits all CRUD operations from BaseService
/// Add course-specific business logic here
/// </summary>

//TODO: Implement method that retrieves active courses between week x to y

public class CourseService : BaseService<Course>, ICourseService
{
    protected override string EntityName => "Course";
    protected override string ApiEndpoint => "courses";

    public CourseService(ISpringApiClient springClient, ILogger<CourseService> logger)
        : base(springClient, logger)
    {
    }


    public async Task<Course> CreateAsync(CreateCourseRequest request, CancellationToken ct = default)
    {
        if (request == null)
        {
            throw new ValidationException("Course data is required");
        }

        if (request.DateStart == null)
        {
            throw new ValidationException("DateStart is required");
        }
        if (request.DateEnd == null)
        {
            throw new ValidationException("DateEnd is required");
        }
        if (request.ClassId < 0)
        {
            throw new ValidationException("ClassId is required");
        }

        if (request.Name == null)
        {
            throw new ValidationException("ClassId is required");
        }

        try
        {
            // Send only the fields without ID to Spring Boot
            var created = await _springClient.PostAsync<CreateCourseRequest, Course>($"{ApiEndpoint}/class/{request.ClassId}", request, ct);
            Console.WriteLine(created);
            if (created == null)
            {
                throw new InvalidOperationException("Backend returned null when creating course");
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

    public async Task<IEnumerable<Course>> GetByClassIdAsync(long classId, CancellationToken ct = default)
    {
        if (classId <= 0)
        {
            throw new ValidationException("Class ID must be greater than 0");
        }

        try
        {
            var courses = await _springClient.GetAsync<IEnumerable<Course>>($"{ApiEndpoint}/class/{classId}", ct);
            return courses?.Select(a => ApplyBusinessLogic(a)) ?? Enumerable.Empty<Course>();
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            _logger.LogWarning("No courses found for class {ClassId}", classId);
            return Enumerable.Empty<Course>();
        }
    }

    public async Task<IEnumerable<Course>> GetByFilterAsync(long? userId, string? status, CancellationToken ct = default)
    {
        // Validate userId if provided
        if (userId.HasValue && userId.Value <= 0)
        {
            throw new ValidationException("userId must be greater than 0");
        }

        // Build query string parameters
        var queryParams = new List<string>();

        if (userId.HasValue)
        {
            queryParams.Add($"userId={userId.Value}");
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            queryParams.Add($"status={Uri.EscapeDataString(status)}");
        }

        var endpoint = ApiEndpoint; // "courses"

        // Append query parameters if any exist
        if (queryParams.Any())
        {
            endpoint += "?" + string.Join("&", queryParams);
        }

        _logger.LogInformation("Fetching courses with filters: userId={UserId}, status={Status}", userId, status);
        _logger.LogInformation("Calling Spring Boot endpoint: {Endpoint}", endpoint);

        try
        {
            var courses = await _springClient.GetAsync<List<Course>>(endpoint, ct);

            if (courses == null || courses.Count == 0)
            {
                _logger.LogInformation("No courses returned for filters userId={UserId}, status={Status}", userId, status);
                return Enumerable.Empty<Course>();
            }

            var processed = courses.Select(ApplyBusinessLogic).ToList();
            _logger.LogInformation("Successfully retrieved {Count} courses with filters userId={UserId}, status={Status}", 
                processed.Count, userId, status);
            return processed;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Spring Boot error while fetching courses with filters userId={UserId}, status={Status}", userId, status);
            throw new InvalidOperationException($"Spring Boot error while fetching courses: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Request timeout while fetching courses with filters userId={UserId}, status={Status}", userId, status);
            throw new TimeoutException($"Request to Spring Boot timed out while fetching courses", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching courses with filters userId={UserId}, status={Status}", userId, status);
            throw;
        }
    }
    
    public async Task<Course?> PatchAsync(long id, PatchCourseRequest request, CancellationToken ct = default)
    {
        if (id <= 0)
            throw new ValidationException("Course ID must be greater than 0");
        
        if (request == null) 
            throw new ValidationException("Patch data is required");

        // Ensure at least one field is provided
        if (request.DateStart is null && request.DateEnd is null)
        {
            throw new ValidationException("At least one field must be provided for patch");
        }

        try
        {
            // Get existing course from Java API
            var existing = await GetByIdAsync((int)id, ct);
            if (existing == null)
            {
                return null;
            }

            bool hasChanges = false;


        

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
        var updated = await _springClient.PutAsync<Course, Course>($"{ApiEndpoint}/{id}", existing, ct);
        
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
    /// Override to add course-specific business logic
    /// Example: validate dates, check conflicts, calculate status
    /// </summary>
    protected override Course ApplyBusinessLogic(Course course)
    {
        // Add course-specific transformations here
        // For example: calculate status, validate deadlines

        return course;
    }
    
    protected override void ValidateEntity(Course course)
    {
        base.ValidateEntity(course);
        if (course.DateStart == default)
            throw new ValidationException("Start date is required");
        if (course.DateEnd == default)
            throw new ValidationException("End date is required");
        if (course.DateEnd < course.DateStart)
            throw new ValidationException("End date must be after start date");
    }
}

