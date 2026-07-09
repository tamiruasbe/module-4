using Microsoft.AspNetCore.Authentication;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;
using TmsApi.Services;
using TmsApi.Filters;

var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════
// AUTHENTICATION & AUTHORIZATION
// ═══════════════════════════════════════════════
builder.Services.AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);

  

builder.Services.AddAuthorization();

// ═══════════════════════════════════════════════
// DATABASE
// ═══════════════════════════════════════════════
builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
           .LogTo(Console.WriteLine, LogLevel.Information)
           .EnableSensitiveDataLogging());

// ═══════════════════════════════════════════════
// CORE SERVICES (M4/M5 baseline — required by M6)
// ═══════════════════════════════════════════════
builder.Services.AddControllers();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditLogFilter>();
});
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

// ═══════════════════════════════════════════════
// APPLICATION SERVICES
// ✅ FIXED: EnrollmentService must be Scoped
// because it depends on DbContext (which is Scoped)
// Singleton holding a Scoped service crashes with
// ValidateScopes = true
// ═══════════════════════════════════════════════
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<ICourseService, CourseService>();
// builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddSingleton<EnrollmentWorker>();

// ═══════════════════════════════════════════════
// STRICT LIFETIME VALIDATION
// Catches scope mismatches at startup not at runtime
// ═══════════════════════════════════════════════
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

// ═══════════════════════════════════════════════
// BUILD
// ═══════════════════════════════════════════════
var app = builder.Build();

// ═══════════════════════════════════════════════
// MIDDLEWARE PIPELINE
// Order matters — ExceptionHandler must be FIRST
// so it can catch errors from everything below it
// ═══════════════════════════════════════════════

// Catches unhandled exceptions → clean 500 ProblemDetails
// (not raw HTML stack trace)
app.UseExceptionHandler();

// Fills empty error responses with ProblemDetails body
// e.g. 404 with no body becomes proper JSON
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    // API documentation — dev only
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Custom middleware — logs every request
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Terminal middleware — registers all controller routes
app.MapControllers();

// ═══════════════════════════════════════════════
// DATABASE SEED
// Runs once on startup if tables are empty
// ═══════════════════════════════════════════════


if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider
        .GetRequiredService<TmsDbContext>();
    await DataSeeder.SeedAsync(context);
}


app.Run();