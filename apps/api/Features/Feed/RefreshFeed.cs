using AIFeed.Api.Infrastructure.Jobs;
using AIFeed.Api.Infrastructure.Persistence;
using AIFeed.Api.Infrastructure.Sources;
using Arkn.Jobs.Models;
using Arkn.Logging.Abstractions;

namespace AIFeed.Api.Features.Feed;

/// <summary>POST /feed/refresh — triggers an immediate feed refresh (outside the cron schedule).</summary>
public static class RefreshFeed
{
    public static void MapEndpoints(WebApplication app)
    {
        app.MapPost("/feed/refresh", Handle)
           .WithName("RefreshFeed")
           .WithSummary("Trigger an immediate feed refresh across all sources")
           .RequireRateLimiting("per-ip");
    }

    private static async Task<IResult> Handle(
        IEnumerable<IFeedSource> sources,
        AppDbContext              db,
        IArknLogger              logger)
    {
        var job = new RefreshFeedJob(sources, db, logger);

        var ctx    = new ArknJobContext(Guid.NewGuid(), "feed.refresh.manual",
                         DateTimeOffset.UtcNow, CancellationToken.None, logger);
        var result = await job.ExecuteAsync(ctx);

        return result.Match(
            onSuccess: () => Results.Ok(new
            {
                refreshed = DateTimeOffset.UtcNow,
                message   = "Feed refresh completed successfully.",
            }),
            onFailure: error => Results.Problem(error.Message));
    }
}
