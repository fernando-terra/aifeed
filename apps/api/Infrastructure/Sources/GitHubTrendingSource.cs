using AIFeed.Api.Infrastructure.Persistence;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace AIFeed.Api.Infrastructure.Sources;

/// <summary>
/// Fetches trending AI repositories from the GitHub Search API.
/// Uses the public unauthenticated endpoint (60 req/h rate limit).
/// Optionally set GITHUB_TOKEN env var to raise the limit to 5000 req/h.
/// </summary>
public sealed class GitHubTrendingSource(HttpClient http) : IFeedSource
{
    public string Id          => "github";
    public string DisplayName => "GitHub Trending";

    private const string Url =
        "https://api.github.com/search/repositories?q=topic:artificial-intelligence+pushed:>2024-01-01&sort=stars&order=desc&per_page=30";

    public async Task<Result<IReadOnlyList<FeedItem>>> FetchAsync(CancellationToken ct = default)
    {
        try
        {
            http.DefaultRequestHeaders.TryAddWithoutValidation(
                "User-Agent", "AIFeed/1.0 (github.com/fernando-terra)");
            http.DefaultRequestHeaders.TryAddWithoutValidation(
                "Accept", "application/vnd.github+json");

            var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
            if (!string.IsNullOrWhiteSpace(token))
                http.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {token}");

            var response = await http.GetFromJsonAsync<GitHubSearchResponse>(Url, ct);
            if (response is null)
                return Result.Failure<IReadOnlyList<FeedItem>>(
                    Error.Failure("GitHub.EmptyResponse", "GitHub API returned an empty response."));

            var items = response.Items
                .Where(r => !string.IsNullOrWhiteSpace(r.HtmlUrl))
                .Select(r => new FeedItem
                {
                    Title       = r.FullName,
                    Url         = r.HtmlUrl!,
                    Source      = Id,
                    Score       = r.StargazersCount,
                    Summary     = r.Description,
                    Author      = r.Owner?.Login,
                    PublishedAt = r.UpdatedAt ?? DateTimeOffset.UtcNow,
                    Tags        = r.Topics?.ToArray() ?? ["ai", "github"],
                })
                .ToList()
                .AsReadOnly();

            return Result.Success<IReadOnlyList<FeedItem>>(items);
        }
        catch (HttpRequestException ex)
        {
            return Result.Failure<IReadOnlyList<FeedItem>>(
                Error.Failure("GitHub.HttpError", $"HTTP error fetching GitHub: {ex.Message}"));
        }
        catch (Exception ex)
        {
            return Result.Failure<IReadOnlyList<FeedItem>>(
                Error.Failure("GitHub.Unexpected", ex.Message));
        }
    }

    // ── Response DTOs (GitHub Search API) ────────────────────────────────────

    private sealed record GitHubSearchResponse(
        [property: JsonPropertyName("items")] List<GitHubRepo> Items);

    private sealed record GitHubRepo(
        [property: JsonPropertyName("full_name")]        string         FullName,
        [property: JsonPropertyName("html_url")]         string?        HtmlUrl,
        [property: JsonPropertyName("description")]      string?        Description,
        [property: JsonPropertyName("stargazers_count")] int            StargazersCount,
        [property: JsonPropertyName("topics")]           List<string>?  Topics,
        [property: JsonPropertyName("updated_at")]       DateTimeOffset? UpdatedAt,
        [property: JsonPropertyName("owner")]            GitHubOwner?   Owner);

    private sealed record GitHubOwner(
        [property: JsonPropertyName("login")] string Login);
}
