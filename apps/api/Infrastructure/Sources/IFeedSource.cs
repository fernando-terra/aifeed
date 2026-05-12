using AIFeed.Api.Infrastructure.Persistence;

namespace AIFeed.Api.Infrastructure.Sources;

/// <summary>
/// Contract for all AI news feed adapters.
/// Each implementation fetches items from one external source.
/// Failures are surfaced via Result&lt;T&gt; — one broken source never breaks the feed.
/// </summary>
public interface IFeedSource
{
    /// <summary>Machine-readable identifier, e.g. "hackernews", "devto".</summary>
    string Id { get; }

    /// <summary>Human-readable label shown in API responses.</summary>
    string DisplayName { get; }

    /// <summary>Fetches the latest AI-related items from the external source.</summary>
    Task<Result<IReadOnlyList<FeedItem>>> FetchAsync(CancellationToken ct = default);
}
