using System.Threading.RateLimiting;
using FluentValidation;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using TeensChurch.API.Data;
using TeensChurch.API.Endpoints;
using TeensChurch.API.Services;
using TeensChurch.API.Validators;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Configuration (PostgreSQL EF Core)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Port=5432;Database=TeensChurchDb;Username=postgres;Password=postgres";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
    });
});

// 2. Dependency Injection Services
builder.Services.AddScoped<ICsvImportService, CsvImportService>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateMemberValidator>();

// 3. CORS Configuration (Allows frontend dashboard to connect seamlessly)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 4. Rate Limiting for bulk upload endpoint (PRD Section 6)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("BulkUploadPolicy", limiterOptions =>
    {
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.PermitLimit = 15;
        limiterOptions.QueueLimit = 5;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

// 5. OpenAPI & Swagger Documentation (PRD Section 2)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 6. Pipeline configuration
app.UseCors();
app.UseRateLimiter();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Teens Church Management API v1");
    c.RoutePrefix = "swagger";
});

// Serve frontend static files if present in wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

// 7. Register Minimal API Endpoints
app.MapMemberEndpoints();
app.MapUnitEndpoints();

// Root redirect or health check
app.MapGet("/api/health", () => Results.Ok(new
{
    status = "Healthy",
    timestamp = DateTime.UtcNow,
    database = "PostgreSQL"
})).WithTags("Health");

// 8. Database auto-initialization & seeding on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = services.GetRequiredService<AppDbContext>();
        await DbInitializer.InitializeAsync(db, logger);
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Could not initialize PostgreSQL database at startup. Provide credentials in appsettings.json or ConnectionStrings:DefaultConnection.");
    }
}

app.Run();
