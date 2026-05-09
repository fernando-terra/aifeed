using AIFeed.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AIFeed.Api.Features.Digest;

/// <summary>GET /digest/daily — top items from the last 24 hours, grouped by source.</summary>
public static class GetDailyDigest
{
    public static void MapEndpoints(WebApplication app)
    {
        app.MapGet("/digest/daily", Handle)
           .WithName("GetDailyDigest")
           .WithSummary("Top AI news from the last 24 hours grouped by source")
           .RequireRateLimiting("per-ip");
    }

    private static async Task<IResult> Handle(AppDbContext db, int topPerSource = 5)
    {
        if (topPerSource is < 1 or > 20)
            return Results.BadRequest(new
            {
                code    = "Digest.InvalidParam",
                message = "topPerSource must be between 1 and 20.",
            });

        var since = DateTimeOffset.UtcNow.AddDays(-1);

        var groups = await db.FeedItems
            .Where(x => x.PublishedAt >= since)
            .OrderByDescending(x => x.Score)
            .ToListAsync();

        var digest = groups
            .GroupBy(x => x.Source)
            .ToDictionary(
                g => g.Key,
                g => g.Take(topPerSource).Select(x => new
                {
                    x.Id, x.Title, x.Url, x.Score,
                    x.Author, x.PublishedAt,
                }));

        return Results.Ok(new
        {
            date        = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd"),
            since,
            topPerSource,
            sources     = digest,
        });
    }
}
