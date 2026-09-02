using Microsoft.AspNetCore.Identity;
using TmsApi.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TmsApi.Application.Interfaces;
using TmsApi.Api.Middlewares;
using TmsApi.Api.Filters;
using TmsApi.Api.Authentication;
using TmsApi.Api.BackgroundServices;
using TmsApi.Infrastructure.Persistence;
using Asp.Versioning;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TmsApi.Api.RateLimiting;
using MediatR;
using FluentValidation;
using TmsApi.Application.Behaviors;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Api.ExceptionHandlers;
using Microsoft.Extensions.Caching.Hybrid;
using TmsApi.Infrastructure.Services;
using TmsApi.Application.Features.Courses.Queries.GetCourses;
using TmsApi.Application.Features.Courses.Commands.CreateCourse;
using System.Threading.Channels;
using TmsApi.Application.Transcripts;
using TmsApi.Infrastructure.Transcripts;
using TmsApi.Infrastructure.Workers;
using TmsApi.Api.Hubs;
using TmsApi.Application.Notifications;
using TmsApi.Api.Notifications;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Tms.Api.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<TokenService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer =
                builder.Configuration["Jwt:Issuer"],

            ValidAudience =
                builder.Configuration["Jwt:Audience"],

            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        builder.Configuration["Jwt:Key"]!
                    )
                ),

            RoleClaimType = ClaimTypes.Role
        };
});
// builder.Services.AddAuthorization();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("CanEditCourse", policy =>
        policy.Requirements.Add(
            new CourseInstructorRequirement()));

builder.Services.AddSingleton< IAuthorizationHandler,  
 CourseInstructorHandler>();

// builder.Services.AddSingleton<CourseInstructorHandler>();


builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
});

builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("TmsDatabase"))
    .LogTo(Console.WriteLine, LogLevel.Information));

builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditLogFilter>();
});
builder.Services.AddSignalR();

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

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(
        typeof(GetCoursesQueryHandler).Assembly);
});


builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(
        typeof(EnrollStudentHandler).Assembly);
});


builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(
        typeof(CreateCourseHandler).Assembly);
});

builder.Services.AddValidatorsFromAssembly(
    typeof(EnrollStudentValidator).Assembly);

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(LoggingBehavior<,>));


builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>));

builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IInstructorService, InstructorService>();

builder.Services.AddScoped<ICachedCourseService, CachedCourseService>();
builder.Services.AddSingleton<ITranscriptStatusStore, InMemoryTranscriptStatusStore>();
builder.Services.AddSingleton<
    ITranscriptNotificationService,
    SignalRTranscriptNotificationService>();


builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions =
        new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(10),

            LocalCacheExpiration =
                TimeSpan.FromMinutes(2)
        };
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("AuthLimiter", opt =>
{
    opt.PermitLimit = 5;
    opt.Window = TimeSpan.FromMinutes(1);
    opt.QueueLimit = 0;
});

options.GlobalLimiter =
    PartitionedRateLimiter.Create<HttpContext, string>(
        httpContext =>
        {
            // Exclude login from the global 10-second limiter.
            // Login has its own AuthLimiter: 5 requests / minute.
            if (httpContext.Request.Path.StartsWithSegments(
                "/api/v1/auth/login"))
            {
                return RateLimitPartition.GetNoLimiter<string>(
                    "login-excluded");
            }

            var (partitionKey, tier) =
                ApiKeyResolver.Resolve(httpContext);

            return tier switch
            {
                ApiKeyTier.Paid =>
                    RateLimitPartition.GetTokenBucketLimiter(
                        $"paid:{partitionKey}",
                        _ => new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = 200,
                            TokensPerPeriod = 100,
                            ReplenishmentPeriod =
                                TimeSpan.FromSeconds(10),
                            QueueLimit = 0,
                            AutoReplenishment = true
                        }),

                ApiKeyTier.Free =>
                    RateLimitPartition.GetTokenBucketLimiter(
                        $"free:{partitionKey}",
                        _ => new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = 30,
                            TokensPerPeriod = 10,
                            ReplenishmentPeriod =
                                TimeSpan.FromSeconds(10),
                            QueueLimit = 0,
                            AutoReplenishment = true
                        }),

                _ =>
                    RateLimitPartition.GetTokenBucketLimiter(
                        $"anon:{partitionKey}",
                        _ => new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = 10,
                            TokensPerPeriod = 5,
                            ReplenishmentPeriod =
                                TimeSpan.FromSeconds(10),
                            QueueLimit = 0,
                            AutoReplenishment = true
                        })
            };
        });


    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;



    options.OnRejected =
        async (context, ct) =>
        {

            var retryAfter = "10";


            if(context.Lease.TryGetMetadata(
                MetadataName.RetryAfter,
                out var ts))
            {
                retryAfter =
                    ((int)ts.TotalSeconds).ToString();
            }


            context.HttpContext.Response.Headers.RetryAfter =
                retryAfter;


            context.HttpContext.Response.ContentType =
                "application/problem+json";


            await context.HttpContext.Response
                .WriteAsJsonAsync(

                new ProblemDetails
                {
                    Title = "Rate limit exceeded",

                    Detail =
                    $"Too many requests. Retry after {retryAfter} seconds.",

                    Status =
                    StatusCodes.Status429TooManyRequests
                },

                ct);

        };



    options.AddConcurrencyLimiter(
        "transcripts",
        opt =>
        {
            opt.PermitLimit = 5;
            opt.QueueLimit = 20;
            opt.QueueProcessingOrder =
                QueueProcessingOrder.OldestFirst;
        });



    options.AddTokenBucketLimiter(
        "search",
        opt =>
        {
            opt.TokenLimit = 10;
            opt.TokensPerPeriod = 5;
            opt.ReplenishmentPeriod =
                TimeSpan.FromSeconds(10);

            opt.QueueLimit = 2;

            opt.QueueProcessingOrder =
                QueueProcessingOrder.OldestFirst;

            opt.AutoReplenishment = true;
        });

});


builder.Services.AddExceptionHandler
    <GlobalExceptionHandler>();

builder.Services.AddProblemDetails();



builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddHostedService<TranscriptWorker>();
builder.Services.AddSingleton(
    Channel.CreateBounded<TranscriptRequest>(
        new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait
        }));

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion =
        new ApiVersion(1,0);


    options.AssumeDefaultVersionWhenUnspecified =
        true;


    options.ReportApiVersions = true;


    options.ApiVersionReader =
        ApiVersionReader.Combine(

            new UrlSegmentApiVersionReader(),

            new HeaderApiVersionReader(
                "X-Api-Version")
        );


})
.AddApiExplorer(options =>
{
    options.GroupNameFormat =
        "'v'VVV";


    options.SubstituteApiVersionInUrl =
        true;
});


var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>()
    ?? ["http://localhost:4200"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("TmsClient", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});



builder.Services
    .AddIdentityCore<TmsUser>(options =>
    {
        options.Password.RequiredLength = 12;

        options.Password.RequireUppercase = true;

        options.Password.RequireDigit = true;

        options.Password.RequireNonAlphanumeric = true;

        options.Lockout.MaxFailedAccessAttempts = 5;

        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);

        options.Lockout.AllowedForNewUsers = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<TmsDbContext>();
var app = builder.Build();

app.Use(async (context, next) =>
{
    context.Response.Headers.Append(
        "X-Content-Type-Options",
        "nosniff");

    context.Response.Headers.Append(
        "X-Frame-Options",
        "DENY");

    context.Response.Headers.Append(
        "Referrer-Policy",
        "strict-origin-when-cross-origin");

    if (app.Environment.IsDevelopment())
    {
        // Scalar requires inline JavaScript during development.
        context.Response.Headers.Append(
            "Content-Security-Policy",
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline'; " +
            "style-src 'self' 'unsafe-inline';");
    }
    else
    {
        context.Response.Headers.Append(
            "Content-Security-Policy",
            "default-src 'self'; " +
            "script-src 'self'; " +
            "style-src 'self';");
    }

    await next();
});

app.UseExceptionHandler();

app.UseStatusCodePages();



if(app.Environment.IsDevelopment())
{

    app.MapOpenApi(
        "/openapi/{documentName}.json");


   app.MapScalarApiReference(options =>
{
    options
        .WithTitle("TMS API Reference")

        .WithTheme(
            ScalarTheme.DeepSpace)

        .WithDefaultHttpClient(
            ScalarTarget.CSharp,
            ScalarClient.HttpClient)

        .AddDocument(
            "v1",
            "API Version 1.0")

        .AddDocument(
            "v2",
            "API Version 2.0")

        .AddPreferredSecuritySchemes("Bearer");
});
}

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("TmsClient");

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.Use(async (context, next) =>
{
if (context.User.Identity?.IsAuthenticated == true || context.Request.Cookies.ContainsKey("tms_auth"))
{
var antiforgery = context.RequestServices
.GetRequiredService<IAntiforgery>();
var tokens = antiforgery.GetAndStoreTokens(context);
context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!,
new CookieOptions
{
HttpOnly = false, // MUST be false so Angular JavaScript can read it!
Secure = !builder.Environment.IsDevelopment(),SameSite = SameSiteMode.Strict
});
}
await next(context);
});
app.UseMiddleware<V1DeprecationMiddleware>();
app.MapControllers();

// app.MapHub<TmsHub>("/hubs/tms");
app.MapHub<TmsHub>("/hubs/tms")
   .RequireCors("TmsClient");

if(app.Environment.IsDevelopment())
{
    using var scope =
        app.Services.CreateScope();
    var context =
        scope.ServiceProvider
        .GetRequiredService<TmsDbContext>();
}

app.Run();
public partial class Program
{
}

