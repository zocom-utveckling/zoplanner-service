using zoplannerservice.Services;
using zoplannerservice.Models;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using System.Net.Http;
using Microsoft.OpenApi.Models;
using Amazon.SQS;
using Microsoft.Extensions.DependencyInjection;
using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.SQS.Model;
using System.Text.Json.Serialization;
using System.Runtime.Serialization;
using zoplannerservice.Serialization;
using DotNetEnv;
using Amazon.Runtime;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


DotNetEnv.Env.Load(".env.local");
var builder = WebApplication.CreateBuilder(args);


// Auth Roles 
// Alla kanske inte behövs / kan ändras.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOnly", policy => policy.RequireRole("Manager"));
    options.AddPolicy("ConsultantOnly", policy => policy.RequireRole("Consultant"));
    options.AddPolicy("CustomerOnly", policy => policy.RequireRole("Customer"));

    options.AddPolicy("StaffOnly", policy => policy.RequireRole("Admin", "Manager", "Consultant"));

    // AllUsers = Ger tillgång till alla inloggade användare.
    options.AddPolicy("AllUsers", policy =>
        policy.RequireAuthenticatedUser());
});

// JWT -------------------------
var jwtSecret = Environment.GetEnvironmentVariable("TOKENKEY");
// Console.WriteLine($"JWT Key: {jwtSecret}"); // För att kolla så jwt blir läst från .env.
if (string.IsNullOrEmpty(jwtSecret))
{
    throw new Exception("JWT Key saknas.");
}

var key = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key)

        };
    });



// Add services and configure JSON (DateOnly -> yyyy-MM-dd), Parse string to enum
builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    o.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    o.JsonSerializerOptions.Converters.Add(new FormatDateTime());

});
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

// AWS SQS ---
if (builder.Environment.IsDevelopment())
{
    Env.Load(".env.local");
}

var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
var regionName = Environment.GetEnvironmentVariable("AWS__Region")
                 ?? builder.Configuration["AWS:Region"];
var queueUrl = Environment.GetEnvironmentVariable("AWS__QueueUrl")
               ?? builder.Configuration["AWS:QueueUrl"];

if (string.IsNullOrEmpty(queueUrl))
{
    throw new InvalidOperationException("AWS QueueUrl is not configured. Check your .env.local or appsettings.json.");
}

var awsOptions = new AWSOptions
{
    Credentials = new BasicAWSCredentials(accessKey, secretKey),
    Region = RegionEndpoint.GetBySystemName(regionName)
};

var sqsClient = new AmazonSQSClient(awsOptions.Credentials, awsOptions.Region);

builder.Services.AddSingleton<NotificationService>(new NotificationService(sqsClient, queueUrl));

// -----------------------------------------------------

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
builder.Services.AddScoped<IBaseService<Customer>, CustomerService>();

builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<IBaseService<Class>, ClassService>();

builder.Services.AddScoped<IAssignmentService, AssignmentService>();
builder.Services.AddScoped<IBaseService<Assignment>, AssignmentService>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBaseService<User>, UserService>();

builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IBaseService<Session>, SessionService>();

builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IBaseService<Course>, CourseService>();

builder.Services.AddScoped<IConsultantService, ConsultantService>();
builder.Services.AddScoped<IBaseService<Consultant>, ConsultantService>();

builder.Services.AddScoped<IManagerService, ManagerService>();
builder.Services.AddScoped<IBaseService<Manager>, ManagerService>();

builder.Services.AddScoped<IAuthService, AuthService>();

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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


app.Run();


