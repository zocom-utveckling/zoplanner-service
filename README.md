# Zoplanner Service - .NET to Spring Boot Connection

## 🚀 Quick Start

**Application URL:** http://localhost:5027  
**Swagger UI:** http://localhost:5027/swagger/index.html

## 🧪 Testing Your API

### 1. Start Spring Boot like usually or
```bash
./mvnw spring-boot:run
```

### 2. Start .NET Application from the directory zoplanner-service\zoplannerservice
C:\Users\buale\Documents\GitHub\zoplanner-service> cd zoplannerservice
PS C:\Users\buale\Documents\GitHub\zoplanner-service\zoplannerservice> dotnet run
```powershell
dotnet run
```
If you are not in the correct directory, type
```powershell
cd zoplannerservice
```
and then
```powershell
dotnet run
```
### 3. Open Swagger
http://localhost:5027/swagger/index.html

### Testing GET Endpoint

**Success (200):**
```json
{
  "id": 5,
  "name": "Acme Corporation",
  "email": "info@acme.com"
}
```

**Not Found (404):**
```json
{
  "message": "Customer with ID 999 not found"
}
```

**Spring Boot Down (503):**
```json
{
  "message": "Backend service error",
  "details": "No connection could be made..."
}
```

## 📋 Table of Contents

1. [What Is This Project?](#-what-is-this-project)
2. [Architecture Overview](#️-architecture-overview)
3. [Step-by-Step: Connecting .NET to Spring Boot](#-step-by-step-connecting-net-to-spring-boot)
4. [Understanding the Code Structure](#️-understanding-the-code-structure)
6. [Troubleshooting](#-troubleshooting)

## 📖 What Is This Project?

This is a **.NET 8.0 Web API** that acts as a **middle layer** (API Gateway) between:

- **React Frontend** (JavaScript app on port 3000)
- **Spring Boot Backend** (Java app on port 8080)

### Why Do We Need This Middle Layer?

1. React speaks JavaScript, Spring Boot speaks Java. .NET "translates" between them and adds business logic.
2. Validates data before it reaches the database (checks ID validity, email format, etc.).
3. Manages errors, retries, and timeouts so React doesn't need to worry about these details.

## 🏗️ Architecture Overview

### The Three-Tier Architecture

```
┌──────────────┐         ┌──────────────┐         ┌──────────────┐         ┌──────────┐
│    React     │────────▶│  .NET API    │────────▶│ Spring Boot  │───────▶│ Database 
│  (Frontend)  │◀────────│   Gateway    │◀────────│  (Backend)   │◀───────│         
└──────────────┘         └──────────────┘          └──────────────┘        └──────────┘
   Port 3000                Port 5027                Port 8080
```

### Project Structure

```
zoplannerservice/
├── Models/                    # Data structures
│   ├── SpringApiOptions.cs    # Configuration for Spring Boot connection
│   ├── Customer.cs            # Customer entity
│   ├── User.cs                # User entity
│   ├── Assignment.cs          # Assignment entity
│   ├── Schedule.cs            # Schedule entity
│   └── Class.cs               # Class entity
├── Services/                  # Business logic layer
│   ├── ISpringApiClient.cs    # Interface: HTTP methods contract
│   ├── SpringApiClient.cs     # Implementation: makes HTTP calls
│   ├── IBaseService.cs        # Interface: CRUD operations contract
│   ├── BaseService.cs         # Generic: reusable CRUD logic
│   ├── CustomerService.cs     # Customer-specific logic
│   ├── UserService.cs         # User-specific logic
│   ├── AssignmentService.cs   # Assignment-specific logic
│   ├── ScheduleService.cs     # Schedule-specific logic
│   └── ClassService.cs        # Class-specific logic
├── Controllers/               # HTTP endpoints (REST API)
│   ├── CustomerController.cs  # GET /api/customer/{id}, DELETE /api/customer/{id}
│   ├── UserController.cs      # GET /api/user/{id}, DELETE /api/user/{id}
│   ├── AssignmentController.cs
│   ├── ScheduleController.cs
│   └── ClassController.cs
├── Program.cs                 # Application entry point
└── appsettings.json           # Configuration (URLs, timeouts, etc.)
```

## 🔌 Step-by-Step: Connecting .NET to Spring Boot

### Step 1: Configure the Connection Settings

**File:** `appsettings.json`
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "SpringApi": {
    "BaseUrl": "http://localhost:8080/api",
    "TimeoutSeconds": 30
  }
}
```

**Explanation:**
- `"BaseUrl"`: Where your Spring Boot API is running
- `"TimeoutSeconds"`: How many seconds to wait before giving up (prevents hanging)

### Step 2: Create Settings Class

**File:** `Models/SpringApiOptions.cs`
```csharp
namespace zoplannerservice.Models;

/// <summary>
/// Configuration settings for Spring Boot API connection
/// .NET automatically fills this from appsettings.json
/// </summary>
public class SpringApiOptions
{
    /// <summary>
    /// Base URL where Spring Boot is running
    /// Example: http://localhost:8080/api
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Timeout in seconds (how long to wait for Spring Boot)
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}
```

### Step 3: Create HTTP Client Interface

**File:** `Services/ISpringApiClient.cs`
```csharp
namespace zoplannerservice.Services;

/// <summary>
/// Contract for making HTTP calls to Spring Boot
/// </summary>
public interface ISpringApiClient
{
    /// <summary>
    /// GET request - Fetch data from Spring Boot
    /// Example: GetAsync<Customer>("customers/5") → Customer object or null
    /// </summary>
    Task<T?> GetAsync<T>(string endpoint, CancellationToken ct = default) where T : class;

    /// <summary>
    /// POST request - Create new data in Spring Boot
    /// Example: PostAsync<Customer, Customer>("customers", newCustomer) → creates customer
    /// </summary>
    Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data, CancellationToken ct = default) 
        where TRequest : class where TResponse : class;

    /// <summary>
    /// PUT request - Update entire entity (replace all fields)
    /// Example: PutAsync("customers/5", updatedCustomer) → replaces all customer data
    /// </summary>
    Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data, CancellationToken ct = default) 
        where TRequest : class where TResponse : class;

    /// <summary>
    /// PATCH request - Update specific fields only
    /// Example: PatchAsync("customers/5", {name: "New Name"}) → changes only name
    /// </summary>
    Task<TResponse?> PatchAsync<TRequest, TResponse>(string endpoint, TRequest data, CancellationToken ct = default) 
        where TRequest : class where TResponse : class;

    /// <summary>
    /// DELETE request - Remove entity from Spring Boot
    /// Example: DeleteAsync("customers/5") → deletes customer with ID 5
    /// </summary>
    Task<bool> DeleteAsync(string endpoint, CancellationToken ct = default);
}
```

### Step 4: Implement HTTP Client

**File:** `Services/SpringApiClient.cs`
```csharp
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using zoplannerservice.Models;

namespace zoplannerservice.Services;

/// <summary>
/// Implementation of ISpringApiClient
/// This class handles all HTTP communication with Spring Boot
/// </summary>
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
            PropertyNameCaseInsensitive = true
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

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Spring API returned 404 for {Url}", uri);
                return null;
            }

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStreamAsync(ct);
            return await JsonSerializer.DeserializeAsync<T>(content, _jsonOptions, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Spring API GET {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string endpoint, TRequest data, CancellationToken ct = default)
        where TRequest : class where TResponse : class
    {
        try
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var uri = new Uri(_client.BaseAddress!, endpoint);

            _logger.LogInformation("Calling Spring API POST {Url}", uri);
            var response = await _client.PostAsync(endpoint, content, ct);
            _logger.LogInformation("Spring API POST {Url} responded {StatusCode}", uri, (int)response.StatusCode);

            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStreamAsync(ct);
            return await JsonSerializer.DeserializeAsync<TResponse>(responseContent, _jsonOptions, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Spring API POST {Endpoint}", endpoint);
            throw;
        }
    }

    // Similar implementations for PutAsync, PatchAsync, DeleteAsync...
}
```

### Step 5: Register Everything in Program.cs

**File:** `Program.cs`
```csharp
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using zoplannerservice.Models;
using zoplannerservice.Services;

var builder = WebApplication.CreateBuilder(args);

// Step 1: Load Spring API settings
builder.Services.Configure<SpringApiOptions>(
    builder.Configuration.GetSection("SpringApi"));

// Step 2: Configure HttpClient with Polly resilience policies
builder.Services.AddHttpClient<ISpringApiClient, SpringApiClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<SpringApiOptions>>().Value;
    var baseUrl = options.BaseUrl;
    if (!baseUrl.EndsWith("/"))
        baseUrl += "/";
    
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());

// Step 3: Register services
builder.Services.AddScoped<IBaseService<Customer>, CustomerService>();
builder.Services.AddScoped<IBaseService<User>, UserService>();
builder.Services.AddScoped<IBaseService<Assignment>, AssignmentService>();
builder.Services.AddScoped<IBaseService<Schedule>, ScheduleService>();
builder.Services.AddScoped<IBaseService<Class>, ClassService>();

// Step 4: Add Controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Step 5: Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.Run();

// Retry Policy: Try 3 times before giving up
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

// Circuit Breaker: Stop after 5 failures
static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
}
```

## 🏗️ Understanding the Code Structure

### Generic BaseService Pattern

**File:** `Services/IBaseService.cs`
```csharp
namespace zoplannerservice.Services;

/// <summary>
/// Generic interface for all entity services
/// T can be Customer, User, Assignment, Schedule, or Class
/// </summary>
public interface IBaseService<T> where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
    Task<T> CreateAsync(T entity, CancellationToken ct = default);
    Task<T> UpdateAsync(int id, T entity, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
```

**File:** `Services/BaseService.cs`
```csharp
namespace zoplannerservice.Services;

/// <summary>
/// Generic base service with common CRUD operations
/// Write once, use for ALL entities
/// </summary>
public abstract class BaseService<T> : IBaseService<T> where T : class
{
    protected readonly ISpringApiClient _springApiClient;
    protected readonly ILogger<BaseService<T>> _logger;
    
    protected abstract string EntityName { get; }  // "Customer"
    protected abstract string ApiEndpoint { get; } // "customers"

    protected BaseService(
        ISpringApiClient springApiClient,
        ILogger<BaseService<T>> logger)
    {
        _springApiClient = springApiClient;
        _logger = logger;
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        if (id <= 0)
        {
            _logger.LogWarning("Invalid {EntityName} ID: {Id}", EntityName, id);
            throw new ArgumentException($"{EntityName} ID must be greater than 0", nameof(id));
        }

        try
        {
            _logger.LogInformation("Fetching {EntityName} with ID {Id}", EntityName, id);
            var entity = await _springApiClient.GetAsync<T>($"{ApiEndpoint}/{id}", ct);
            
            if (entity == null)
            {
                _logger.LogWarning("{EntityName} with ID {Id} not found", EntityName, id);
                throw new KeyNotFoundException($"{EntityName} with ID {id} not found");
            }

            entity = ApplyBusinessLogic(entity);
            _logger.LogInformation("Successfully fetched {EntityName} with ID {Id}", EntityName, id);
            return entity;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Spring Boot error when fetching {EntityName} {Id}", EntityName, id);
            throw new InvalidOperationException($"Spring Boot error for {EntityName} {id}: {ex.Message}", ex);
        }
    }

    public virtual async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        if (id <= 0)
        {
            _logger.LogWarning("Invalid {EntityName} ID for deletion: {Id}", EntityName, id);
            throw new ArgumentException($"{EntityName} ID must be greater than 0", nameof(id));
        }

        try
        {
            _logger.LogInformation("Deleting {EntityName} with ID {Id}", EntityName, id);
            var result = await _springApiClient.DeleteAsync($"{ApiEndpoint}/{id}", ct);
            
            if (result)
            {
                _logger.LogInformation("Successfully deleted {EntityName} with ID {Id}", EntityName, id);
            }
            else
            {
                _logger.LogWarning("{EntityName} with ID {Id} not found for deletion", EntityName, id);
            }
            
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Spring Boot error when deleting {EntityName} {Id}", EntityName, id);
            throw new InvalidOperationException($"Spring Boot error for {EntityName} {id}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Hook for child classes to add custom business logic
    /// </summary>
    protected virtual T ApplyBusinessLogic(T entity)
    {
        return entity; // Default: no transformation
    }

    // Stubs for future implementation
    public virtual Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
        => throw new NotImplementedException("GetAllAsync not implemented yet");

    public virtual Task<T> CreateAsync(T entity, CancellationToken ct = default)
        => throw new NotImplementedException("CreateAsync not implemented yet");

    public virtual Task<T> UpdateAsync(int id, T entity, CancellationToken ct = default)
        => throw new NotImplementedException("UpdateAsync not implemented yet");
}
```

### Entity Services

**File:** `Services/CustomerService.cs`
```csharp
using zoplannerservice.Models;

namespace zoplannerservice.Services;

public class CustomerService : BaseService<Customer>
{
    protected override string EntityName => "Customer";
    protected override string ApiEndpoint => "customers";

    public CustomerService(
        ISpringApiClient springApiClient,
        ILogger<BaseService<Customer>> logger)
        : base(springApiClient, logger) { }

    protected override Customer ApplyBusinessLogic(Customer customer)
    {
        // Add custom logic here
        // Example: customer.DiscountRate = CalculateDiscount(customer);
        return customer;
    }
}
```

### Controllers

**File:** `Controllers/CustomerController.cs`
```csharp
using Microsoft.AspNetCore.Mvc;
using zoplannerservice.Models;
using zoplannerservice.Services;

namespace zoplannerservice.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly IBaseService<Customer> _customerService;
    private readonly ILogger<CustomerController> _logger;

    public CustomerController(
        IBaseService<Customer> customerService,
        ILogger<CustomerController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("GET request for Customer ID {Id}", id);
            var customer = await _customerService.GetByIdAsync(id, ct);
            return Ok(customer);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error for Customer ID {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Customer ID {Id} not found", id);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error fetching Customer ID {Id}", id);
            return StatusCode(503, new 
            { 
                message = "Backend service error",
                details = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching Customer ID {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("DELETE request for Customer ID {Id}", id);
            var result = await _customerService.DeleteAsync(id, ct);
            
            if (result)
                return NoContent();
            else
                return NotFound(new { message = $"Customer with ID {id} not found" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error for Customer ID {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Service error deleting Customer ID {Id}", id);
            return StatusCode(503, new 
            { 
                message = "Backend service error",
                details = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting Customer ID {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
```
## 🐛 Troubleshooting

### Problem 1: "Connection refused"
**Solution:** Start Spring Boot and check `appsettings.json` BaseUrl

### Problem 2: "404 Not Found" for everything
**Solution:** Check service endpoints match Spring Boot controller paths

### Problem 3: "Request timeout" (504)
**Solution:** Increase `TimeoutSeconds` in `appsettings.json`

### Problem 4: "Circuit breaker is open"
**Solution:** Wait 30 seconds or fix Spring Boot issues

### Problem 5: JSON deserialization errors
**Solution:** Make foreign keys and dates nullable in models


