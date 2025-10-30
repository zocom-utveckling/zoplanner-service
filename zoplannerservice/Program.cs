using zoplannerservice.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services 
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register UserService 
builder.Services.AddScoped<UserService>();

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
    app.UseSwaggerUI();


app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();


app.Run();


