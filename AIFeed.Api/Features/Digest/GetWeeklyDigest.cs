using AIFeed.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AIFeed.Api.Features.Digest;

/// <summary>GET /digest/weekly — top items from the last 7 days, grouped by source.</summary>
public static class GetWeeklyDigest
{
    public static void MapEndpoints(WebApplication app)
    {
        app.MapGet("/digest/weekly", Handle)
           .WithName("GetWeeklyDigest")
           .WithSummary("Top AI news from the last 7 days grouped by source")
           .RequireRateLimiting("per-ip");
    }

    private static async Task<IResult> Handle(AppDbContext db, int topPerSource = 10)
    {
        if (topPerSource is < 1 or > 50)
            return Results.BadRequest(new
            {
                code    = "Digest.InvalidParam",
                message = "topPerSource must be between 1 and 50.",
            });

        var since = DateTimeOffset.UtcNow.AddDays(-7);

        var all = await db.FeedItems
            .Where(x => x.PublishedAt >= since)
            .OrderByDescending(x => x.Score)
            .ToListAsync();

        var digest = all
            .GroupBy(x => x.Source)
            .ToDictionary(
                g => g.Key,
                g => g.Take(topPerSource).Select(x => new
                {
                    x.Id, x.Title, x.Url, x.Score,
                    x.Author, x.PublishedAt,
                }));

        var topOverall = all
            .Take(topPerSource)
            .Select(x => new { x.Id, x.Title, x.Url, x.Source, x.Score, x.Author, x.PublishedAt });

        return Results.Ok(new
        {
            weekStarting = since.ToString("yyyy-MM-dd"),
            weekEnding   = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd"),
            topPerSource,
            topOverall,
            bySource     = digest,
        });
    }
}
