using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.SQS;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using System.Text;
using System.Text.Json.Serialization;
using zoplannerservice.Models;
using zoplannerservice.Serialization;
using zoplannerservice.Services;
using zoplannerservice.Services.Interfaces;
using zoplannerservice.Swagger;

static void TryLoadEnvFile()
{
    var envFiles = new[]
    {
        Path.Combine(Directory.GetCurrentDirectory(), ".env.local"),
        Path.Combine(Directory.GetCurrentDirectory(), ".env"),
        Path.Combine(AppContext.BaseDirectory, ".env.local"),
        Path.Combine(AppContext.BaseDirectory, ".env"),
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".env.local"),
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".env"),
    };

    foreach (var file in envFiles)
    {
        var fullPath = Path.GetFullPath(file);
        if (File.Exists(fullPath))
        {
            Env.Load(fullPath);
            Console.WriteLine($"Loaded env file: {fullPath}");
            return;
        }
    }

    Console.WriteLine("No .env file found. Using system environment variables only.");
}

TryLoadEnvFile();

var builder = WebApplication.CreateBuilder(args);

// JWT
var jwtSecret = Environment.GetEnvironmentVariable("TOKENKEY")
               ?? builder.Configuration["TOKENKEY"];

if (string.IsNullOrWhiteSpace(jwtSecret))
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
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };
    });

// Authorization - match Java
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ManagerOnly", policy => policy.RequireRole("MANAGER"));
    options.AddPolicy("ConsultantOnly", policy => policy.RequireRole("CONSULTANT"));
    options.AddPolicy("ManagerOrConsultant", policy => policy.RequireRole("MANAGER", "CONSULTANT", "BOTH"));
    options.AddPolicy("AllUsers", policy => policy.RequireAuthenticatedUser());
});

// Controllers + JSON
builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    o.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    o.JsonSerializerOptions.Converters.Add(new FormatTimeSpan());
    o.JsonSerializerOptions.Converters.Add(new FormatDateTime());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "zoplannerservice (.NET)",
        Version = "v1",
        Description = ".NET API gateway applying business logic and calling Java CRUD Spring Boot"
    });

    c.OperationFilter<NotificationExamplesOperationFilter>();

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Skriv: Bearer {din token}",

        Reference = new OpenApiReference
        {
            Id = "Bearer",
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition("Bearer", jwtSecurityScheme);

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            jwtSecurityScheme,
            Array.Empty<string>()
        }
    });
});



// AWS SQS
var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
var regionName = Environment.GetEnvironmentVariable("AWS__Region")
                 ?? Environment.GetEnvironmentVariable("AWS_REGION")
                 ?? builder.Configuration["AWS:Region"];

var queueUrl = Environment.GetEnvironmentVariable("AWS__QueueUrl")
               ?? Environment.GetEnvironmentVariable("SQS_QUEUE_URL")
               ?? builder.Configuration["AWS:QueueUrl"];

if (string.IsNullOrEmpty(queueUrl))
{
    throw new InvalidOperationException("AWS QueueUrl is not configured. Check your .env or appsettings.json.");
}

var awsOptions = new AWSOptions
{
    Credentials = new BasicAWSCredentials(accessKey, secretKey),
    Region = RegionEndpoint.GetBySystemName(regionName)
};

var sqsClient = new AmazonSQSClient(awsOptions.Credentials, awsOptions.Region);
builder.Services.AddSingleton<NotificationService>(new NotificationService(sqsClient, queueUrl));

Console.WriteLine($"Config peek SpringApi:BaseUrl = {builder.Configuration["SpringApi:BaseUrl"]}");

builder.Services.Configure<SpringApiOptions>(builder.Configuration.GetSection("SpringApi"));

builder.Services.AddHttpClient<ISpringApiClient, SpringApiClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<SpringApiOptions>>().Value;

    if (string.IsNullOrWhiteSpace(options.BaseUrl))
    {
        throw new InvalidOperationException("SpringApi:BaseUrl is not configured in appsettings.json");
    }

    var normalized = options.BaseUrl.EndsWith("/") ? options.BaseUrl : options.BaseUrl + "/";
    client.BaseAddress = new Uri(normalized);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
})
.AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(
    3,
    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
))
.AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(
    5,
    TimeSpan.FromSeconds(200)
));

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
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<IFileService, FileService>();

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