using AIFeed.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AIFeed.Api.Features.Search;

/// <summary>GET /search?q=&amp;source=&amp;from=&amp;to= — full-text search over ingested items.</summary>
public static class SearchFeed
{
    public static void MapEndpoints(WebApplication app)
    {
        app.MapGet("/search", Handle)
           .WithName("SearchFeed")
           .WithSummary("Search AI news items by keyword, source, and date range")
           .RequireRateLimiting("per-ip");
    }

    private static async Task<IResult> Handle(
        AppDbContext      db,
        string?           q      = null,
        string?           source = null,
        DateTimeOffset?   from   = null,
        DateTimeOffset?   to     = null,
        int               page   = 1,
        int               size   = 20)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Results.BadRequest(new
            {
                code    = "Search.MissingQuery",
                message = "Query parameter 'q' is required.",
            });

        if (q.Length < 2)
            return Results.BadRequest(new
            {
                code    = "Search.QueryTooShort",
                message = "Query must be at least 2 characters.",
            });

        if (page < 1 || size is < 1 or > 100)
            return Results.BadRequest(new
            {
                code    = "Search.InvalidPagination",
                message = "page ≥ 1, size between 1 and 100.",
            });

        var query = db.FeedItems.AsQueryable();

        // SQLite LIKE search — case-insensitive by default on ASCII
        query = query.Where(x => EF.Functions.Like(x.Title, $"%{q}%")
                               || (x.Summary != null && EF.Functions.Like(x.Summary, $"%{q}%")));

        if (!string.IsNullOrWhiteSpace(source))
            query = query.Where(x => x.Source == source);

        if (from.HasValue)
            query = query.Where(x => x.PublishedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(x => x.PublishedAt <= to.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.PublishedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(); // materialise first — Tags value converter can't project inside SQL

        return Results.Ok(new
        {
            q,
            source,
            from,
            to,
            page,
            size,
            total,
            pages = (int)Math.Ceiling((double)total / size),
            items,
        });
    }
}
