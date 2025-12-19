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
    public virtual async Task<T?> GetByIdAsync(long id, CancellationToken ct = default)
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
    public virtual async Task<IEnumerable<T>> GetAllSync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching all {EntityName} entities", EntityName);

        try
        {
            var entities = await _springClient.GetAsync<List<T>>(ApiEndpoint, ct);
            if (entities == null || entities.Count == 0)
            {
                _logger.LogInformation("No {EntityName} entities returned from backend", EntityName);
                return Enumerable.Empty<T>();
            }

            // Apply business logic to each entity
            var processed = entities.Select(ApplyBusinessLogic).ToList();
            _logger.LogInformation("Successfully retrieved {Count} {EntityName} entities", processed.Count, EntityName);
            return processed;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Spring Boot error while fetching all {EntityName} entities", EntityName);
            throw new InvalidOperationException($"Spring Boot error for fetching all {EntityName} entities: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Request timeout while fetching all {EntityName} entities", EntityName);
            throw new TimeoutException($"Request to Spring Boot timed out while fetching all {EntityName} entities", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching all {EntityName} entities", EntityName);
            throw;
        }
    }

    /// <summary>
    /// Create new entity - STUB: to be implemented based on business requirements
    /// </summary>
    public virtual async Task<T> CreateAsync(T entity, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating new {EntityName} entity", EntityName);

        // Validate entity input
        ValidateEntity(entity);

        try
        {
            var created = await _springClient.PostAsync<T, T>(ApiEndpoint, entity, ct);
            if (created == null)
            {
                _logger.LogError("Spring Boot returned null when creating {EntityName}", EntityName);
                throw new InvalidOperationException($"Failed to create {EntityName} - backend returned null");
            }

            created = ApplyBusinessLogic(created);
            _logger.LogInformation("Successfully created {EntityName} entity", EntityName);
            return created;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Spring Boot error while creating {EntityName}", EntityName);
            throw new InvalidOperationException($"Spring Boot error while creating {EntityName}: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Request timeout while creating {EntityName}", EntityName);
            throw new TimeoutException($"Request to Spring Boot timed out while creating {EntityName}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating {EntityName}", EntityName);
            throw;
        }
    }

    /// <summary>
    /// Update existing entity - STUB: to be implemented based on business requirements
    /// </summary>
    public virtual async Task<T?> UpdateAsync(int id, T entity, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating {EntityName} with ID {Id}", EntityName, id);

        // Validate ID and entity
        if (id <= 0)
        {
            _logger.LogWarning("Invalid {EntityName} ID for update: {Id}", EntityName, id);
            throw new ValidationException($"{EntityName} ID must be greater than 0. Provided: {id}");
        }
        ValidateEntity(entity);

        try
        {
            // Verify entity exists before updating
            var existing = await _springClient.GetAsync<T>($"{ApiEndpoint}/{id}", ct);
            if (existing == null)
            {
                _logger.LogWarning("{EntityName} with ID {Id} not found - cannot update", EntityName, id);
                return null;
            }

            var updated = await _springClient.PutAsync<T, T>($"{ApiEndpoint}/{id}", entity, ct);
            if (updated == null)
            {
                _logger.LogError("Spring Boot returned null when updating {EntityName} {Id}", EntityName, id);
                throw new InvalidOperationException($"Failed to update {EntityName} {id} - backend returned null");
            }

            updated = ApplyBusinessLogic(updated);
            _logger.LogInformation("Successfully updated {EntityName} with ID {Id}", EntityName, id);
            return updated;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Spring Boot error while updating {EntityName} {Id}", EntityName, id);
            throw new InvalidOperationException($"Spring Boot error while updating {EntityName} {id}: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Request timeout while updating {EntityName} {Id}", EntityName, id);
            throw new TimeoutException($"Request to Spring Boot timed out while updating {EntityName} {id}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating {EntityName} {Id}", EntityName, id);
            throw;
        }
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
