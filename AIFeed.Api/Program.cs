using AIFeed.Api.Features.Digest;
using AIFeed.Api.Features.Feed;
using AIFeed.Api.Features.Search;
using AIFeed.Api.Features.Sources;
using AIFeed.Api.Infrastructure.Jobs;
using AIFeed.Api.Infrastructure.Persistence;
using AIFeed.Api.Infrastructure.Sources;
using Arkn.Jobs.Extensions;
using Arkn.Jobs.Models;
using Arkn.Logging.Extensions;
using Microsoft.EntityFrameworkCore;

// ── Builder ───────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// Arkn structured logging
builder.Services.AddArknLogging(logging =>
{
    logging.AddConsoleSink();
    logging.AddInMemorySink();
});

// EF Core + SQLite
var dbPath = builder.Configuration.GetConnectionString("Default")
             ?? "Data Source=aifeed.db";

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(dbPath));

// Feed sources — registered as IFeedSource so IEnumerable<IFeedSource> resolves all
builder.Services.AddHttpClient<HackerNewsSource>();
builder.Services.AddHttpClient<DevToSource>();
builder.Services.AddHttpClient<ArxivSource>();
builder.Services.AddHttpClient<GitHubTrendingSource>();
builder.Services.AddHttpClient<ProductHuntSource>();

builder.Services.AddSingleton<IFeedSource, HackerNewsSource>();
builder.Services.AddSingleton<IFeedSource, DevToSource>();
builder.Services.AddSingleton<IFeedSource, ArxivSource>();
builder.Services.AddSingleton<IFeedSource, GitHubTrendingSource>();
builder.Services.AddSingleton<IFeedSource, ProductHuntSource>();

// Arkn background jobs
builder.Services.AddArknJobs(jobs =>
{
    jobs.Add<RefreshFeedJob>("*/30 * * * *")
        .WithName("feed.refresh")
        .WithDescription("Fetches AI news from all sources every 30 minutes")
        .WithRetry(maxAttempts: 2)
        .WithTimeout(TimeSpan.FromMinutes(5));

    jobs.Add<CleanupJob>("0 3 * * *")
        .WithName("feed.cleanup")
        .WithDescription("Purges feed items older than 7 days (runs at 03:00 UTC)");
});

builder.Services.AddScoped<RefreshFeedJob>();
builder.Services.AddScoped<CleanupJob>();

// ASP.NET Core Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    // Global spike arrest: max 20 req/s
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
        RateLimitPartition.GetSlidingWindowLimiter("global", _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit      = 20,
            Window           = TimeSpan.FromSeconds(1),
            SegmentsPerWindow = 4,
        }));

    // Per-IP: 100 req/min
    options.AddPolicy("per-ip", ctx =>
        RateLimitPartition.GetSlidingWindowLimiter(
            ctx.Connection.RemoteIpAddress?.ToString() ?? "anon",
            _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit      = 100,
                Window           = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 4,
            }));

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddEndpointsApiExplorer();

// ── App ───────────────────────────────────────────────────────────────────────

var app = builder.Build();

app.UseRateLimiter();

// Apply EF migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

// ── Endpoints ─────────────────────────────────────────────────────────────────

GetSources.MapEndpoints(app);
GetSourceHealth.MapEndpoints(app);
GetFeed.MapEndpoints(app);
RefreshFeed.MapEndpoints(app);
GetDailyDigest.MapEndpoints(app);
GetWeeklyDigest.MapEndpoints(app);
SearchFeed.MapEndpoints(app);

// Health check
app.MapGet("/health", () => Results.Ok(new { status = "ok", time = DateTimeOffset.UtcNow }))
   .WithName("HealthCheck")
   .RequireRateLimiting("per-ip");

app.Run();
