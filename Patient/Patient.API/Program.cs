using GenericToolKit.Application.DependencyInjection;
using GenericToolKit.Domain.Interfaces;
using GenericToolKit.Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Patient.API.Infrastructure.LoggedInUser;
using Patient.Application.Services;
using Patient.Infra.Data;
using Patient.Infra.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// DEPENDENCY INJECTION CONFIGURATION
// Demonstrates comprehensive setup of Generic Toolkit features
// ============================================================================

// 1. Add HTTP Context Accessor (required for HttpContextLoggedInUser)
builder.Services.AddHttpContextAccessor();

// 2. Register ILoggedInUser implementation
// This provides tenant and user context for multi-tenancy and audit tracking
builder.Services.AddScoped<ILoggedInUser, HttpContextLoggedInUser>();

// 3. Register DbContext with SQL Server
// BaseContext automatically applies tenant filtering and soft delete filtering
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PatientDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    });

    // Enable detailed errors in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Register PatientDbContext as DbContext (required by GenericRepository)
builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<PatientDbContext>());

// 4. Register Generic Repositories using toolkit extension methods
// This registers IGenericRepository<Patient> and GenericRepository<Patient>
builder.Services.AddGenericRepository<Patient.Domain.Entities.Patient>();
builder.Services.AddGenericRepository<Patient.Domain.Entities.Appointment>();

// 5. Register Custom Repositories
builder.Services.AddScoped<IPatientRepository, PatientRepository>();

// 6. Register Generic Services using toolkit extension methods
// This registers IGenericService<Patient> and GenericService<Patient>
builder.Services.AddGenericService<Patient.Domain.Entities.Patient>();
builder.Services.AddGenericService<Patient.Domain.Entities.Appointment>();

// 7. Register Custom Services
builder.Services.AddScoped<IPatientService, PatientService>();

// 8. Add Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure JSON serialization
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// 9. Add CORS (if needed)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 10. Add Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// ============================================================================
// MIDDLEWARE PIPELINE
// ============================================================================

// CORS
app.UseCors("AllowAll");

app.UseHttpsRedirection();

// Custom middleware to log tenant and user context
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

    var tenantId = context.Request.Headers["X-Tenant-Id"].FirstOrDefault() ?? "Not provided";
    var userId = context.Request.Headers["X-User-Id"].FirstOrDefault() ?? "Not provided";
    var roleId = context.Request.Headers["X-Role-Id"].FirstOrDefault() ?? "Not provided";

    logger.LogInformation(
        "Request: {Method} {Path} | Tenant: {TenantId} | User: {UserId} | Role: {RoleId}",
        context.Request.Method,
        context.Request.Path,
        tenantId,
        userId,
        roleId);

    await next();
});

app.UseAuthorization();

app.MapControllers();

// ============================================================================
// DATABASE MIGRATION (Auto-apply on startup in development)
// ============================================================================
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            var context = services.GetRequiredService<PatientDbContext>();

            logger.LogInformation("Checking for pending database migrations...");

            // Apply pending migrations
            if (context.Database.GetPendingMigrations().Any())
            {
                logger.LogInformation("Applying pending migrations...");
                context.Database.Migrate();
                logger.LogInformation("Migrations applied successfully");
            }
            else
            {
                logger.LogInformation("Database is up to date");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database");
            throw;
        }
    }
}

// Log startup information
var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
startupLogger.LogInformation("Patient Microservice started successfully");
startupLogger.LogInformation("API available at: https://localhost:7001");
startupLogger.LogInformation("Use Postman collection for API testing");
startupLogger.LogInformation("\n" +
    "===========================================\n" +
    "PATIENT MICROSERVICE API\n" +
    "===========================================\n" +
    "Generic Toolkit Features Demonstrated:\n" +
    "- Multi-tenancy (X-Tenant-Id header)\n" +
    "- User tracking (X-User-Id header)\n" +
    "- Role management (X-Role-Id header)\n" +
    "- Automatic audit fields\n" +
    "- Soft delete support\n" +
    "- Repository & Service patterns\n" +
    "- Specification pattern\n" +
    "- Transaction management\n" +
    "- Change tracking\n" +
    "===========================================\n");

app.Run();
