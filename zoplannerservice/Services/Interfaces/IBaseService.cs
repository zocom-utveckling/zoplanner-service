namespace zoplannerservice.Services;

/// <summary>
/// Generic base interface for all entity services
/// Provides standard CRUD operations
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface IBaseService<T> where T : class
{
    /// <summary>
    /// Get entity by ID
    /// </summary>
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    
    /// <summary>
    /// Get all entities
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Create new entity
    /// </summary>
    Task<T> CreateAsync(T entity, CancellationToken ct = default);
    
    /// <summary>
    /// Update existing entity
    /// </summary>
    Task<T?> UpdateAsync(int id, T entity, CancellationToken ct = default);

    /// <summary>
    /// Delete entity by ID
    /// </summary>
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    
}
