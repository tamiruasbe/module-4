using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TmsApi.Application.Interfaces;
using TmsApi.Application.DTOs;
using TmsApi.Api.Middlewares;
using TmsApi.Api.Filters;
using TmsApi.Api.Authentication;
using TmsApi.Api.BackgroundServices;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Domain.Entities;

// using TmsApi.Filters.SomeFilter;

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
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("TmsDatabase"))
           .LogTo(Console.WriteLine, LogLevel.Information));
        //    .EnableSensitiveDataLogging());


// ═══════════════════════════════════════════════
// CORE SERVICES
// ═══════════════════════════════════════════════
builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditLogFilter>();
});

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();


// ═══════════════════════════════════════════════
// APPLICATION SERVICES
// ═══════════════════════════════════════════════
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<ICourseService, CourseService>();

builder.Services.AddSingleton<EnrollmentWorker>();


// ═══════════════════════════════════════════════
// STRICT LIFETIME VALIDATION
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
// ═══════════════════════════════════════════════

app.UseExceptionHandler();

app.UseStatusCodePages();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}


app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();


// Controllers
app.MapControllers();


// ═══════════════════════════════════════════════
// DATABASE SEED
// ═══════════════════════════════════════════════

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var context = scope.ServiceProvider
        .GetRequiredService<TmsDbContext>();

    await DataSeeder.SeedAsync(context);
}


app.Run();