using AIFeed.Api.Infrastructure.Sources;

namespace AIFeed.Api.Features.Sources;

/// <summary>GET /sources — lists all registered feed sources and their IDs.</summary>
public static class GetSources
{
    public static void MapEndpoints(WebApplication app)
    {
        app.MapGet("/sources", Handle)
           .WithName("GetSources")
           .WithSummary("List all registered AI news sources")
           .RequireRateLimiting("per-ip");
    }

    private static IResult Handle(IEnumerable<IFeedSource> sources)
    {
        var result = sources.Select(s => new { s.Id, s.DisplayName }).ToList();
        return Results.Ok(result);
    }
}
