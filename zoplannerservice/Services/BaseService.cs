using System.ComponentModel.DataAnnotations;

namespace zoplannerservice.Services;

/// <summary>
/// Base service implementation with common business logic
/// Handles validation, logging, and error handling for all entity types
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public abstract class BaseService<T> : IBaseService<T> where T : class
{
    protected readonly ISpringApiClient _springClient;
    protected readonly ILogger<BaseService<T>> _logger;
    protected abstract string EntityName { get; }
    protected abstract string ApiEndpoint { get; }

    protected BaseService(ISpringApiClient springClient, ILogger<BaseService<T>> logger)
    {
        _springClient = springClient;
        _logger = logger;
    }

    /// <summary>
    /// Get entity by ID with full validation and error handling
    /// </summary>
    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        // Validate input
        if (id <= 0)
        {
            _logger.LogWarning("Invalid {EntityName} ID: {Id}. ID must be greater than 0", EntityName, id);
            throw new ValidationException($"{EntityName} ID must be greater than 0. Provided: {id}");
        }

        _logger.LogInformation("Fetching {EntityName} with ID: {Id}", EntityName, id);

        try
        {
            // Call Spring Boot API
            var entity = await _springClient.GetAsync<T>($"{ApiEndpoint}/{id}", ct);

            if (entity == null)
            {
                _logger.LogInformation("{EntityName} with ID {Id} not found in database", EntityName, id);
                return null;
            }

            // Apply business logic (can be overridden in derived classes)
            entity = ApplyBusinessLogic(entity);

            _logger.LogInformation("Successfully retrieved {EntityName} with ID {Id}", EntityName, id);
            return entity;
        }
        catch (HttpRequestException ex)
        {
            // HttpRequestException now contains Spring Boot error details
            _logger.LogError(ex, "Spring Boot error while fetching {EntityName} {Id}", EntityName, id);
            throw new InvalidOperationException($"Spring Boot error for {EntityName} {id}: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Request timeout while fetching {EntityName} {Id}", EntityName, id);
            throw new TimeoutException($"Request to Spring Boot timed out for {EntityName} {id}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching {EntityName} {Id}", EntityName, id);
            throw;
        }
    }

    /// <summary>
    /// Delete entity by ID with validation
    /// </summary>
    public virtual async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        // Validate input
        if (id <= 0)
        {
            _logger.LogWarning("Invalid {EntityName} ID for deletion: {Id}", EntityName, id);
            throw new ValidationException($"{EntityName} ID must be greater than 0. Provided: {id}");
        }

        _logger.LogInformation("Attempting to delete {EntityName} with ID: {Id}", EntityName, id);

        try
        {
            // Check if entity exists before deletion
            var existingEntity = await _springClient.GetAsync<T>($"{ApiEndpoint}/{id}", ct);
            
            if (existingEntity == null)
            {
                _logger.LogWarning("{EntityName} with ID {Id} not found in database, cannot delete", EntityName, id);
                return false;
            }

            // Proceed with deletion
            var deleted = await _springClient.DeleteAsync($"{ApiEndpoint}/{id}", ct);
            
            if (deleted)
            {
                _logger.LogInformation("Successfully deleted {EntityName} with ID {Id}", EntityName, id);
            }
            else
            {
                _logger.LogWarning("Failed to delete {EntityName} with ID {Id}", EntityName, id);
            }

            return deleted;
        }
        catch (HttpRequestException ex)
        {
            // HttpRequestException now contains Spring Boot error details
            _logger.LogError(ex, "Spring Boot error while deleting {EntityName} {Id}", EntityName, id);
            throw new InvalidOperationException($"Spring Boot error for deleting {EntityName} {id}: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Request timeout while deleting {EntityName} {Id}", EntityName, id);
            throw new TimeoutException($"Request to Spring Boot timed out for {EntityName} {id}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting {EntityName} {Id}", EntityName, id);
            throw;
        }
    }

    /// <summary>
    /// Get all entities - STUB: to be implemented based on business requirements
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("GetAllAsync called for {EntityName} - STUB: Not yet implemented", EntityName);
        
        // TODO: Implement when business logic is defined
        // Example implementation:
        // var entities = await _springClient.GetAsync<List<T>>(ApiEndpoint, ct);
        // return entities?.Select(ApplyBusinessLogic) ?? Enumerable.Empty<T>();
        
        throw new NotImplementedException($"GetAllAsync for {EntityName} is not yet implemented. Define business logic first.");
    }

    /// <summary>
    /// Create new entity - STUB: to be implemented based on business requirements
    /// </summary>
    public virtual async Task<T> CreateAsync(T entity, CancellationToken ct = default)
    {
        _logger.LogInformation("CreateAsync called for {EntityName} - STUB: Not yet implemented", EntityName);
        
        // TODO: Implement when business logic is defined
        // 1. Validate entity data
        // 2. Apply business rules
        // 3. Call Spring Boot API
        // 4. Return created entity
        
        throw new NotImplementedException($"CreateAsync for {EntityName} is not yet implemented. Define validation and business logic first.");
    }

    /// <summary>
    /// Update existing entity - STUB: to be implemented based on business requirements
    /// </summary>
    public virtual async Task<T?> UpdateAsync(int id, T entity, CancellationToken ct = default)
    {
        _logger.LogInformation("UpdateAsync called for {EntityName} ID {Id} - STUB: Not yet implemented", EntityName, id);
        
        // TODO: Implement when business logic is defined
        // 1. Validate ID
        // 2. Validate entity data
        // 3. Check if entity exists
        // 4. Apply business rules
        // 5. Call Spring Boot API
        // 6. Return updated entity
        
        throw new NotImplementedException($"UpdateAsync for {EntityName} is not yet implemented. Define validation and business logic first.");
    }

    /// <summary>
    /// Apply business logic to entity
    /// Override in derived classes to add specific business rules
    /// </summary>
    protected virtual T ApplyBusinessLogic(T entity)
    {
        // Default implementation: no modifications
        // Override in derived classes for specific business logic
        return entity;
    }

    /// <summary>
    /// Validate entity data
    /// Override in derived classes to add specific validation rules
    /// </summary>
    protected virtual void ValidateEntity(T entity)
    {
        if (entity == null)
        {
            _logger.LogWarning("{EntityName} object is null", EntityName);
            throw new ValidationException($"{EntityName} data is required");
        }

        // Override in derived classes for specific validation
    }
}
