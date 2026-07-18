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
using Asp.Versioning;

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
// builder.Services.AddOpenApi();
builder.Services.AddOpenApi("v1", options =>
{
    options.ShouldInclude = description =>
        description.GroupName == "v1";
});


builder.Services.AddOpenApi("v2", options =>
{
    options.ShouldInclude = description =>
        description.GroupName == "v2";
});


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



builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);

    options.AssumeDefaultVersionWhenUnspecified = true;

    options.ReportApiVersions = true;

    // options.ApiVersionReader = new UrlSegmentApiVersionReader();
    options.ApiVersionReader = ApiVersionReader.Combine(
    new UrlSegmentApiVersionReader(),
    new HeaderApiVersionReader("X-Api-Version")
);

})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";

    options.SubstituteApiVersionInUrl = true;
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
   app.MapOpenApi("/openapi/{documentName}.json");
   app.MapScalarApiReference(options =>
{
    options
        .WithTitle("TMS API Reference")
        .WithTheme(ScalarTheme.DeepSpace)
        .WithDefaultHttpClient(
            ScalarTarget.CSharp,
            ScalarClient.HttpClient
        )
        .AddDocument("v1", "API Version 1.0")
        .AddDocument("v2", "API Version 2.0");
});
}


app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseMiddleware<V1DeprecationMiddleware>();
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