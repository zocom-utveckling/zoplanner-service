using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using zoplannerservice.Models;
using zoplannerservice.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace zoplannerservice.Services;

public class SpringApiClient : ISpringApiClient
{
    private readonly HttpClient _client;
    private readonly ILogger<SpringApiClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public SpringApiClient(
        HttpClient client,
        ILogger<SpringApiClient> logger,
        IOptions<SpringApiOptions> options)
    {
        _client = client;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,  // Use camelCase for Spring Boot compatibility
            Converters = { new JsonStringEnumConverter(), new FormatDateTime() }
        };
    }

    public async Task<T?> GetAsync<T>(string endpoint, CancellationToken ct = default) where T : class
    {
        try
        {
            var uri = new Uri(_client.BaseAddress!, endpoint);
            _logger.LogInformation("Calling Spring API GET {Url}", uri);
            var response = await _client.GetAsync(endpoint, ct);
            _logger.LogInformation("Spring API GET {Url} responded {StatusCode}", uri, (int)response.StatusCode);
            
            // Handle 404 - entity not found
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Spring API returned 404 for {Url}", uri);
                return null;
            }

            // Handle error responses from Spring Boot
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("Spring Boot error {Status} for GET {Url}: {ErrorBody}", 
                    (int)response.StatusCode, uri, errorBody);
                
                throw new HttpRequestException(
                    $"Spring Boot returned {(int)response.StatusCode} ({response.StatusCode}) for GET {endpoint}: {errorBody}");
            }

            // Log raw JSON for debugging
            var rawJson = await response.Content.ReadAsStringAsync(ct);
            _logger.LogInformation("Raw JSON from Spring Boot: {Json}", rawJson);
            
            return JsonSerializer.Deserialize<T>(rawJson, _jsonOptions);
        }
        catch (HttpRequestException)
        {
            // Re-throw HttpRequestException with Spring Boot error details
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Spring API GET {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string endpoint,
        TRequest data,
        CancellationToken ct = default)
        where TRequest : class
        where TResponse : class
    {
        try
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var uri = new Uri(_client.BaseAddress!, endpoint);
            _logger.LogInformation("Calling Spring API POST {Url} with body: {RequestBody}", uri, json);
            
            var response = await _client.PostAsync(endpoint, content, ct);
            _logger.LogInformation("Spring API POST {Url} responded {StatusCode}", uri, (int)response.StatusCode);
            
            // Handle error responses from Spring Boot
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("Spring Boot error {Status} for POST {Url}: {ErrorBody}", 
                    (int)response.StatusCode, uri, errorBody);
                
                throw new HttpRequestException(
                    $"Spring Boot returned {(int)response.StatusCode} ({response.StatusCode}) for POST {endpoint}: {errorBody}");
            }
            
            var responseContent = await response.Content.ReadAsStreamAsync(ct);
            return await JsonSerializer.DeserializeAsync<TResponse>(responseContent, _jsonOptions, ct);
        }
        catch (HttpRequestException)
        {
            // Re-throw HttpRequestException with Spring Boot error details
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Spring API POST {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<TResponse?> PostAsync<TResponse>(string endpoint, CancellationToken ct = default) where TResponse : class
    {
        try
        {
            var uri = new Uri(_client.BaseAddress!, endpoint);
            _logger.LogInformation("Calling Spring API POST {Url} with body: {RequestBody}", uri, "{}");

            var response = await _client.PostAsync(endpoint, null);
            _logger.LogInformation("Spring API POST {Url} responded {StatusCode}", uri, (int)response.StatusCode);

            // Handle error responses from Spring Boot
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("Spring Boot error {Status} for POST {Url}: {ErrorBody}",
                    (int)response.StatusCode, uri, errorBody);

                throw new HttpRequestException(
                    $"Spring Boot returned {(int)response.StatusCode} ({response.StatusCode}) for POST {endpoint}: {errorBody}");
            }

            var responseContent = await response.Content.ReadAsStreamAsync(ct);
            return await JsonSerializer.DeserializeAsync<TResponse>(responseContent, _jsonOptions, ct);
        }
        catch (HttpRequestException)
        {
            // Re-throw HttpRequestException with Spring Boot error details
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Spring API POST {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<TResponse?> PostFormAsync<TResponse>(
        string endpoint,
        Dictionary<string, string> formData,
        CancellationToken ct = default)
        where TResponse : class
    {
        try
        {
            var content = new FormUrlEncodedContent(formData);
            var uri = new Uri(_client.BaseAddress!, endpoint);
            _logger.LogInformation("Calling Spring API POST {Url} with form data: {FormData}", 
                uri, string.Join(", ", formData.Select(kvp => $"{kvp.Key}={kvp.Value}")));
            
            var response = await _client.PostAsync(endpoint, content, ct);
            _logger.LogInformation("Spring API POST {Url} responded {StatusCode}", uri, (int)response.StatusCode);
            
            // Handle error responses from Spring Boot
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("Spring Boot error {Status} for POST {Url}: {ErrorBody}", 
                    (int)response.StatusCode, uri, errorBody);
                
                throw new HttpRequestException(
                    $"Spring Boot returned {(int)response.StatusCode} ({response.StatusCode}) for POST {endpoint}: {errorBody}");
            }
            
            var responseContent = await response.Content.ReadAsStreamAsync(ct);
            return await JsonSerializer.DeserializeAsync<TResponse>(responseContent, _jsonOptions, ct);
        }
        catch (HttpRequestException)
        {
            // Re-throw HttpRequestException with Spring Boot error details
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Spring API POST form {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(
    string endpoint,
    TRequest data,
    CancellationToken ct = default)
    where TRequest : class
    where TResponse : class
    {
        try
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var uri = new Uri(_client.BaseAddress!, endpoint);

            _logger.LogInformation("Calling Spring API PUT {Url} with body: {Body}", uri, json);  // ← Log the body

            var response = await _client.PutAsync(endpoint, content, ct);
            _logger.LogInformation("Spring API PUT {Url} responded {StatusCode}", uri, (int)response.StatusCode);

            // Check for errors BEFORE EnsureSuccessStatusCode
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("Spring Boot error {Status} for PUT {Url}: {ErrorBody}",
                    (int)response.StatusCode, uri, errorBody);

                throw new HttpRequestException(
                    $"Spring Boot returned {(int)response.StatusCode} ({response.StatusCode}) for PUT {endpoint}: {errorBody}");
            }

            var responseContent = await response.Content.ReadAsStreamAsync(ct);
            return await JsonSerializer.DeserializeAsync<TResponse>(responseContent, _jsonOptions, ct);
        }
        catch (HttpRequestException)
        {
            // Re-throw HttpRequestException with Spring Boot error details
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Spring API PUT {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<TResponse?> PatchAsync<TRequest, TResponse>(
        string endpoint,
        TRequest data,
        CancellationToken ct = default)
        where TRequest : class
        where TResponse : class
    {
        try
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var uri = new Uri(_client.BaseAddress!, endpoint);
            _logger.LogInformation("Calling Spring API PATCH {Url} with body: {RequestBody}", uri, json);
            
            var response = await _client.PatchAsync(endpoint, content, ct);
            _logger.LogInformation("Spring API PATCH {Url} responded {StatusCode}", uri, (int)response.StatusCode);
            
            // Handle error responses from Spring Boot
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("Spring Boot error {Status} for PATCH {Url}: {ErrorBody}", 
                    (int)response.StatusCode, uri, errorBody);
                
                throw new HttpRequestException(
                    $"Spring Boot returned {(int)response.StatusCode} ({response.StatusCode}) for PATCH {endpoint}: {errorBody}");
            }
            
            var responseContent = await response.Content.ReadAsStreamAsync(ct);
            return await JsonSerializer.DeserializeAsync<TResponse>(responseContent, _jsonOptions, ct);
        }
        catch (HttpRequestException)
        {
            // Re-throw HttpRequestException with Spring Boot error details
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Spring API PATCH {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string endpoint, CancellationToken ct = default)
    {
        try
        {
            var uri = new Uri(_client.BaseAddress!, endpoint);
            _logger.LogInformation("Calling Spring API DELETE {Url}", uri);
            var response = await _client.DeleteAsync(endpoint, ct);
            _logger.LogInformation("Spring API DELETE {Url} responded {StatusCode}", uri, (int)response.StatusCode);
            
            // Handle 404 - entity not found
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Spring API returned 404 for DELETE {Url}", uri);
                return false;
            }
            
            // Handle error responses from Spring Boot
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("Spring Boot error {Status} for DELETE {Url}: {ErrorBody}", 
                    (int)response.StatusCode, uri, errorBody);
                
                throw new HttpRequestException(
                    $"Spring Boot returned {(int)response.StatusCode} ({response.StatusCode}) for DELETE {endpoint}: {errorBody}");
            }
            
            return true;
        }
        catch (HttpRequestException)
        {
            // Re-throw HttpRequestException with Spring Boot error details
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Spring API DELETE {Endpoint}", endpoint);
            throw;
        }
    }


}