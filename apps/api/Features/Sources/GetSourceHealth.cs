using AIFeed.Api.Infrastructure.Sources;

namespace AIFeed.Api.Features.Sources;

/// <summary>GET /sources/{id}/health — probes a single source and reports its status.</summary>
public static class GetSourceHealth
{
    public static void MapEndpoints(WebApplication app)
    {
        app.MapGet("/sources/{id}/health", Handle)
           .WithName("GetSourceHealth")
           .WithSummary("Probe a feed source and check its health")
           .RequireRateLimiting("per-ip");
    }

    private static async Task<IResult> Handle(string id, IEnumerable<IFeedSource> sources)
    {
        var source = sources.FirstOrDefault(s => s.Id == id);
        if (source is null)
            return Results.NotFound(new { code = "Source.NotFound", message = $"Source '{id}' not found." });

        using var cts    = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var result       = await source.FetchAsync(cts.Token);

        return result.Match(
            onSuccess: items => Results.Ok(new
            {
                source    = source.Id,
                status    = "healthy",
                itemCount = items.Count,
                probed    = DateTimeOffset.UtcNow,
            }),
            onFailure: error => Results.Ok(new
            {
                source  = source.Id,
                status  = "unhealthy",
                reason  = error.Message,
                probed  = DateTimeOffset.UtcNow,
            }));
    }
}
