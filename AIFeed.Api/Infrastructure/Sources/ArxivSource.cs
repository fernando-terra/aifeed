using AIFeed.Api.Infrastructure.Persistence;
using System.Xml.Linq;

namespace AIFeed.Api.Infrastructure.Sources;

/// <summary>
/// Fetches recent AI papers from arXiv via the Atom feed API.
/// Parses the XML/Atom response — no external library required.
/// </summary>
public sealed class ArxivSource(HttpClient http) : IFeedSource
{
    public string Id          => "arxiv";
    public string DisplayName => "arXiv";

    private const string Url =
        "https://export.arxiv.org/api/query?search_query=cat:cs.AI&max_results=50&sortBy=submittedDate&sortOrder=descending";

    private static readonly XNamespace Atom = "http://www.w3.org/2005/Atom";

    public async Task<Result<IReadOnlyList<FeedItem>>> FetchAsync(CancellationToken ct = default)
    {
        try
        {
            var xml = await http.GetStringAsync(Url, ct);
            var doc = XDocument.Parse(xml);

            var items = doc.Root!
                .Elements(Atom + "entry")
                .Select(e => new FeedItem
                {
                    Title       = e.Element(Atom + "title")?.Value.Trim() ?? "Untitled",
                    Url         = e.Element(Atom + "id")?.Value.Trim() ?? "",
                    Source      = Id,
                    Summary     = e.Element(Atom + "summary")?.Value.Trim(),
                    Author      = e.Elements(Atom + "author")
                                   .FirstOrDefault()
                                   ?.Element(Atom + "name")?.Value,
                    PublishedAt = DateTimeOffset.TryParse(
                                      e.Element(Atom + "published")?.Value, out var dt)
                                  ? dt : DateTimeOffset.UtcNow,
                    Tags        = ["ai", "research", "arxiv"],
                })
                .Where(i => !string.IsNullOrWhiteSpace(i.Url))
                .ToList()
                .AsReadOnly();

            return Result.Success<IReadOnlyList<FeedItem>>(items);
        }
        catch (HttpRequestException ex)
        {
            return Result.Failure<IReadOnlyList<FeedItem>>(
                Error.Failure("Arxiv.HttpError", $"HTTP error fetching arXiv: {ex.Message}"));
        }
        catch (Exception ex)
        {
            return Result.Failure<IReadOnlyList<FeedItem>>(
                Error.Failure("Arxiv.Unexpected", ex.Message));
        }
    }
}
