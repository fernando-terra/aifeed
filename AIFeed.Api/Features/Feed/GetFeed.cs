using AIFeed.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AIFeed.Api.Features.Feed;

/// <summary>GET /feed?page=&amp;size=&amp;source= — paginated feed with optional source filter.</summary>
public static class GetFeed
{
    public static void MapEndpoints(WebApplication app)
    {
        app.MapGet("/feed", Handle)
           .WithName("GetFeed")
           .WithSummary("Get paginated AI news feed")
           .RequireRateLimiting("per-ip");
    }

    private static async Task<IResult> Handle(
        AppDbContext db,
        int          page   = 1,
        int          size   = 20,
        string?      source = null)
    {
        if (page < 1 || size is < 1 or > 100)
            return Results.BadRequest(new
            {
                code    = "Feed.InvalidPagination",
                message = "page ≥ 1, size between 1 and 100.",
            });

        var query = db.FeedItems.AsQueryable();

        if (!string.IsNullOrWhiteSpace(source))
            query = query.Where(x => x.Source == source);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.PublishedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(x => new
            {
                x.Id, x.Title, x.Url, x.Source,
                x.Tags, x.Score, x.Author, x.Summary,
                x.PublishedAt, x.IngestedAt,
            })
            .ToListAsync();

        return Results.Ok(new
        {
            page,
            size,
            total,
            pages = (int)Math.Ceiling((double)total / size),
            items,
        });
    }
}
