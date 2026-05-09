using AIFeed.Api.Infrastructure.Persistence;

namespace AIFeed.Api.Infrastructure.Sources;

/// <summary>
/// Fetches AI products from Product Hunt via the GraphQL API.
/// Requires PRODUCTHUNT_TOKEN env var (Developer Token from ph.com/v2/oauth/applications).
/// When the token is absent, the source returns an empty list so other sources are unaffected.
/// </summary>
public sealed class ProductHuntSource(HttpClient http) : IFeedSource
{
    public string Id          => "producthunt";
    public string DisplayName => "Product Hunt";

    public async Task<Result<IReadOnlyList<FeedItem>>> FetchAsync(CancellationToken ct = default)
    {
        var token = Environment.GetEnvironmentVariable("PRODUCTHUNT_TOKEN");
        if (string.IsNullOrWhiteSpace(token))
        {
            // No token configured — return empty instead of erroring so the feed keeps working
            return Result.Success<IReadOnlyList<FeedItem>>(Array.Empty<FeedItem>());
        }

        try
        {
            const string query = """
                {
                  "query": "{ posts(order: VOTES, topic: \"artificial-intelligence\", first: 30) { edges { node { name tagline url votesCount createdAt user { name } } } } }"
                }
                """;

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.producthunt.com/v2/api/graphql");
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {token}");
            request.Content = new StringContent(query, System.Text.Encoding.UTF8, "application/json");

            using var response = await http.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            var items = ParseProductHuntResponse(json);

            return Result.Success<IReadOnlyList<FeedItem>>(items);
        }
        catch (HttpRequestException ex)
        {
            return Result.Failure<IReadOnlyList<FeedItem>>(
                Error.Failure("ProductHunt.HttpError", $"HTTP error fetching Product Hunt: {ex.Message}"));
        }
        catch (Exception ex)
        {
            return Result.Failure<IReadOnlyList<FeedItem>>(
                Error.Failure("ProductHunt.Unexpected", ex.Message));
        }
    }

    private IReadOnlyList<FeedItem> ParseProductHuntResponse(string json)
    {
        // Lightweight manual parse to avoid System.Text.Json nesting complexity
        var items = new List<FeedItem>();

        using var doc = System.Text.Json.JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("data", out var data)) return items;
        if (!data.TryGetProperty("posts",        out var posts)) return items;
        if (!posts.TryGetProperty("edges",        out var edges)) return items;

        foreach (var edge in edges.EnumerateArray())
        {
            if (!edge.TryGetProperty("node", out var node)) continue;

            var name     = node.TryGetProperty("name",     out var n) ? n.GetString() : null;
            var url      = node.TryGetProperty("url",      out var u) ? u.GetString() : null;
            var tagline  = node.TryGetProperty("tagline",  out var t) ? t.GetString() : null;
            var votes    = node.TryGetProperty("votesCount", out var v) ? v.GetInt32() : 0;
            var author   = node.TryGetProperty("user", out var usr) && usr.TryGetProperty("name", out var un)
                           ? un.GetString() : null;
            DateTimeOffset published = DateTimeOffset.UtcNow;
            if (node.TryGetProperty("createdAt", out var ca) && DateTimeOffset.TryParse(ca.GetString(), out var dt))
                published = dt;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(url)) continue;

            items.Add(new FeedItem
            {
                Title       = name!,
                Url         = url!,
                Source      = Id,
                Score       = votes,
                Summary     = tagline,
                Author      = author,
                PublishedAt = published,
                Tags        = ["ai", "producthunt"],
            });
        }

        return items.AsReadOnly();
    }
}
