namespace AIFeed.Api.Infrastructure.Persistence;

/// <summary>Central aggregate — one item fetched from any AI news source.</summary>
public sealed class FeedItem
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    public required string Title       { get; init; }
    public required string Url         { get; init; }

    /// <summary>Source identifier: "hackernews" | "devto" | "arxiv" | "github" | "producthunt"</summary>
    public required string Source      { get; init; }

    public string[] Tags               { get; init; } = [];
    public int      Score              { get; init; }
    public string?  Summary            { get; init; }
    public string?  Author             { get; init; }
    public DateTimeOffset PublishedAt  { get; init; }
    public DateTimeOffset IngestedAt   { get; init; } = DateTimeOffset.UtcNow;
}
