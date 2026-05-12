using AIFeed.Api.Infrastructure.Persistence;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace AIFeed.Api.Infrastructure.Sources;

/// <summary>
/// Fetches AI-tagged articles from dev.to via the public Articles API.
/// Zero auth required. Returns up to 50 items per call.
/// </summary>
public sealed class DevToSource(HttpClient http) : IFeedSource
{
    public string Id          => "devto";
    public string DisplayName => "dev.to";

    private const string Url = "https://dev.to/api/articles?tag=ai&per_page=50&state=fresh";

    public async Task<Result<IReadOnlyList<FeedItem>>> FetchAsync(CancellationToken ct = default)
    {
        try
        {
            http.DefaultRequestHeaders.TryAddWithoutValidation(
                "User-Agent", "AIFeed/1.0 (github.com/fernando-terra)");

            var articles = await http.GetFromJsonAsync<List<DevToArticle>>(Url, ct);
            if (articles is null)
                return Result.Failure<IReadOnlyList<FeedItem>>(
                    Error.Failure("DevTo.EmptyResponse", "dev.to API returned an empty response."));

            var items = articles
                .Where(a => !string.IsNullOrWhiteSpace(a.Url) && !string.IsNullOrWhiteSpace(a.Title))
                .Select(a => new FeedItem
                {
                    Title       = a.Title!,
                    Url         = a.Url!,
                    Source      = Id,
                    Score       = a.PublicReactionsCount,
                    Author      = a.User?.Name,
                    Summary     = a.Description,
                    PublishedAt = a.PublishedAt ?? DateTimeOffset.UtcNow,
                    Tags        = a.TagList?.ToArray() ?? ["ai"],
                })
                .ToList()
                .AsReadOnly();

            return Result.Success<IReadOnlyList<FeedItem>>(items);
        }
        catch (HttpRequestException ex)
        {
            return Result.Failure<IReadOnlyList<FeedItem>>(
                Error.Failure("DevTo.HttpError", $"HTTP error fetching dev.to: {ex.Message}"));
        }
        catch (Exception ex)
        {
            return Result.Failure<IReadOnlyList<FeedItem>>(
                Error.Failure("DevTo.Unexpected", ex.Message));
        }
    }

    // ── Response DTOs (dev.to API) ─────────────────────────────────────────────

    private sealed record DevToArticle(
        [property: JsonPropertyName("title")]                 string?          Title,
        [property: JsonPropertyName("url")]                   string?          Url,
        [property: JsonPropertyName("description")]           string?          Description,
        [property: JsonPropertyName("tag_list")]              List<string>?    TagList,
        [property: JsonPropertyName("public_reactions_count")] int             PublicReactionsCount,
        [property: JsonPropertyName("published_at")]          DateTimeOffset?  PublishedAt,
        [property: JsonPropertyName("user")]                  DevToUser?       User);

    private sealed record DevToUser(
        [property: JsonPropertyName("name")] string? Name);
}
