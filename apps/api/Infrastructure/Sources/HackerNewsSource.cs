using AIFeed.Api.Infrastructure.Persistence;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace AIFeed.Api.Infrastructure.Sources;

/// <summary>
/// Fetches AI-related stories from Hacker News via the Algolia HN Search API.
/// Zero auth required. Returns up to 50 items per call.
/// </summary>
public sealed class HackerNewsSource(HttpClient http) : IFeedSource
{
    public string Id          => "hackernews";
    public string DisplayName => "Hacker News";

    private const string Url =
        "http://hn.algolia.com/api/v1/search?tags=story&query=AI&hitsPerPage=50&numericFilters=points>5";

    public async Task<Result<IReadOnlyList<FeedItem>>> FetchAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await http.GetFromJsonAsync<HnSearchResponse>(Url, ct);
            if (response is null)
                return Result.Failure<IReadOnlyList<FeedItem>>(
                    Error.Failure("HackerNews.EmptyResponse", "Algolia API returned an empty response."));

            var items = response.Hits
                .Where(h => !string.IsNullOrWhiteSpace(h.Url) && !string.IsNullOrWhiteSpace(h.Title))
                .Select(h => new FeedItem
                {
                    Title       = h.Title!,
                    Url         = h.Url!,
                    Source      = Id,
                    Score       = h.Points,
                    Author      = h.Author,
                    PublishedAt = DateTimeOffset.FromUnixTimeSeconds(h.CreatedAtI),
                    Tags        = ["ai", "hackernews"],
                })
                .ToList()
                .AsReadOnly();

            return Result.Success<IReadOnlyList<FeedItem>>(items);
        }
        catch (HttpRequestException ex)
        {
            return Result.Failure<IReadOnlyList<FeedItem>>(
                Error.Failure("HackerNews.HttpError", $"HTTP error fetching HN: {ex.Message}"));
        }
        catch (Exception ex)
        {
            return Result.Failure<IReadOnlyList<FeedItem>>(
                Error.Failure("HackerNews.Unexpected", ex.Message));
        }
    }

    // ── Response DTOs (Algolia HN Search API) ─────────────────────────────────

    private sealed record HnSearchResponse(
        [property: JsonPropertyName("hits")] List<HnHit> Hits);

    private sealed record HnHit(
        [property: JsonPropertyName("title")]         string? Title,
        [property: JsonPropertyName("url")]           string? Url,
        [property: JsonPropertyName("author")]        string? Author,
        [property: JsonPropertyName("points")]        int     Points,
        [property: JsonPropertyName("created_at_i")] long    CreatedAtI);
}
