using zoplannerservice.Models;

namespace zoplannerservice.Services;

public interface ISpringApiClient
{
    Task<T?> GetAsync<T>(string endpoint, CancellationToken ct = default) where T : class;
    
    Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data, CancellationToken ct = default) 
        where TRequest : class 
        where TResponse : class;
    
    Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data, CancellationToken ct = default) 
        where TRequest : class 
        where TResponse : class;
    
    Task<TResponse?> PatchAsync<TRequest, TResponse>(string endpoint, TRequest data, CancellationToken ct = default) 
        where TRequest : class 
        where TResponse : class;
    
    Task<bool> DeleteAsync(string endpoint, CancellationToken ct = default);
}