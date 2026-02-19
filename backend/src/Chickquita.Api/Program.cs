using Chickquita.Api.Endpoints;
using Chickquita.Api.Middleware;
using Chickquita.Application;
using MediatR;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Reflection;
using System.Text.Json;
using Chickquita.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Chickquita API",
        Version = "v1",
        Description = "API for Chickquita - A mobile-first PWA for tracking chicken farming profitability",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Chickquita Team"
        }
    });

    // Add JWT Bearer authentication to Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your Clerk JWT token. You can get this from your Clerk session."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Include XML documentation comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Application Insights telemetry
builder.Services.AddApplicationInsightsTelemetry();

// Register Application layer services (MediatR, AutoMapper, FluentValidation)
builder.Services.AddApplicationServices();

// Register Infrastructure layer services (DbContext, Repositories, Services)
builder.Services.AddInfrastructureServices(builder.Configuration);

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<Chickquita.Infrastructure.Data.ApplicationDbContext>(
        name: "database",
        tags: ["ready"]);

// Configure CORS for development only
if (builder.Environment.IsDevelopment())
{
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? new[] { "http://localhost:3100" };

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DevelopmentPolicy", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Chickquita API v1");
        options.RoutePrefix = "swagger"; // Access Swagger at /swagger
        options.DocumentTitle = "Chickquita API Documentation";
        options.DisplayRequestDuration();
        options.EnableTryItOutByDefault();
    });
    app.UseCors("DevelopmentPolicy");
}

app.UseHttpsRedirection();

// Serve default files (index.html) before static files
app.UseDefaultFiles();

// Serve static files with cache headers
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cache static assets for 30 days in production
        if (!app.Environment.IsDevelopment())
        {
            var path = ctx.File.PhysicalPath;
            // Cache JS, CSS, images, fonts for 30 days
            if (path != null && (path.EndsWith(".js") || path.EndsWith(".css") ||
                path.EndsWith(".png") || path.EndsWith(".jpg") || path.EndsWith(".jpeg") ||
                path.EndsWith(".svg") || path.EndsWith(".gif") || path.EndsWith(".webp") ||
                path.EndsWith(".woff") || path.EndsWith(".woff2") || path.EndsWith(".ttf") ||
                path.EndsWith(".eot")))
            {
                ctx.Context.Response.Headers.CacheControl = "public,max-age=2592000"; // 30 days
            }
            else if (path != null && path.EndsWith(".html"))
            {
                // Don't cache HTML files (especially index.html)
                ctx.Context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
                ctx.Context.Response.Headers.Pragma = "no-cache";
                ctx.Context.Response.Headers.Expires = "0";
            }
        }
    }
});

// Authentication & Authorization middleware
app.UseAuthentication();

// Tenant resolution middleware - must come after UseAuthentication
app.UseMiddleware<TenantResolutionMiddleware>();

app.UseAuthorization();

static async Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json; charset=utf-8";

    var entries = report.Entries.Select(e => new
    {
        key = e.Key,
        value = new
        {
            status = e.Value.Status.ToString(),
            duration = e.Value.Duration.ToString(),
            description = e.Value.Description,
            exception = e.Value.Exception?.Message
        }
    }).ToDictionary(e => e.key, e => e.value);

    var result = new
    {
        status = report.Status.ToString(),
        totalDuration = report.TotalDuration.ToString(),
        entries
    };

    await context.Response.WriteAsync(
        JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
}

// Health check endpoints — unauthenticated, publicly accessible
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = WriteHealthCheckResponse,
    AllowCachingResponses = false
}).AllowAnonymous();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    // Liveness: no dependency checks — just proves the app is running
    Predicate = _ => false,
    ResponseWriter = WriteHealthCheckResponse,
    AllowCachingResponses = false
}).AllowAnonymous();

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    // Readiness: only checks tagged "ready" (database)
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = WriteHealthCheckResponse,
    AllowCachingResponses = false
}).AllowAnonymous();

// MediatR test endpoint (can be removed after verification)
app.MapGet("/ping", async (IMediator mediator) =>
    {
        var result = await mediator.Send(new Chickquita.Application.Features.System.PingQuery());
        return Results.Ok(new { message = result });
    })
    .WithName("Ping")
    .WithOpenApi()
    .Produces<object>(200);

// Map webhook endpoints
app.MapWebhooksEndpoints();

// Map users endpoints
app.MapUsersEndpoints();

// Map coops endpoints
app.MapCoopsEndpoints();

// Map flocks endpoints
app.MapFlocksEndpoints();

// Map flock history endpoints
app.MapFlockHistoryEndpoints();

// Map daily records endpoints
app.MapDailyRecordsEndpoints();

// Map statistics endpoints
app.MapStatisticsEndpoints();

// Map purchases endpoints
app.MapPurchasesEndpoints();

// SPA fallback - serve index.html for all non-API routes
// This must come AFTER API endpoint mapping to ensure API routes take precedence
app.MapFallbackToFile("index.html");

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
