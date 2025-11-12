using zoplannerservice.Services;
using zoplannerservice.Models;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using System.Net.Http;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services 
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "zoplannerservice",
        Version = "v1",
        Description = "API gateway applying business logic and calling Spring Boot"
    });
});

// Quick debug: peek config value
Console.WriteLine($"Config peek SpringApi:BaseUrl = {builder.Configuration["SpringApi:BaseUrl"]}");

// Configure Spring API options (single registration)
builder.Services.Configure<SpringApiOptions>(builder.Configuration.GetSection("SpringApi"));

// Add HttpClient for Spring API with retry and circuit breaker (single registration)
builder.Services.AddHttpClient<ISpringApiClient, SpringApiClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<SpringApiOptions>>().Value;
    Console.WriteLine($"Debug: SpringApi:BaseUrl = {options.BaseUrl}"); // temp log
    if (string.IsNullOrWhiteSpace(options.BaseUrl))
    {
        throw new InvalidOperationException("SpringApi:BaseUrl is not configured in appsettings.json");
    }
    var normalized = options.BaseUrl.EndsWith("/") ? options.BaseUrl : options.BaseUrl + "/";
    client.BaseAddress = new Uri(normalized);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
})
.AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(3, retryAttempt =>
    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
.AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));

// Register services
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<IBaseService<Customer>, CustomerService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBaseService<User>, UserService>();
builder.Services.AddScoped<IBaseService<Assignment>, AssignmentService>();
builder.Services.AddScoped<IBaseService<Session>, SessionService>();
builder.Services.AddScoped<IBaseService<Class>, ClassService>();

// Add CORS (allow React and Java to connect)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


var app = builder.Build();

// Swagger for testing
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "zoplannerservice v1");
        c.RoutePrefix = "swagger";
    });


app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();


app.Run();


