using AIFeed.Api.Infrastructure.Persistence;
using AIFeed.Api.Infrastructure.Sources;
using Arkn.Jobs.Abstractions;
using Arkn.Jobs.Models;
using Arkn.Logging.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AIFeed.Api.Infrastructure.Jobs;

/// <summary>
/// Runs every 30 minutes. Fetches items from all registered <see cref="IFeedSource"/>s
/// and upserts them into the database. A failed source is logged and skipped —
/// it never takes down the other sources.
/// </summary>
public sealed class RefreshFeedJob(
    IEnumerable<IFeedSource> sources,
    AppDbContext              db,
    IArknLogger              logger) : IArknJob
{
    public async Task<Result> ExecuteAsync(ArknJobContext ctx)
    {
        var ingested = 0;
        var failed   = 0;

        foreach (var source in sources)
        {
            var result = await source.FetchAsync(ctx.CancellationToken);

            if (result.IsFailure)
            {
                logger.Warning(
                    $"[RefreshFeedJob] Source '{source.Id}' failed: {result.Error.Message}");
                failed++;
                continue;
            }

            var count = await UpsertAsync(result.Value, ctx.CancellationToken);
            ingested += count;

            logger.Info(
                $"[RefreshFeedJob] Source '{source.Id}' ingested {count} new item(s).");
        }

        logger.Info(
            $"[RefreshFeedJob] Complete. Ingested={ingested} Failed={failed}");

        return Result.Success();
    }

    private async Task<int> UpsertAsync(
        IReadOnlyList<FeedItem> items,
        CancellationToken       ct)
    {
        var count = 0;
        foreach (var item in items)
        {
            // Skip if Url already exists for this source (unique index)
            var exists = await db.FeedItems
                .AnyAsync(x => x.Source == item.Source && x.Url == item.Url, ct);

            if (!exists)
            {
                db.FeedItems.Add(item);
                count++;
            }
        }

        if (count > 0) await db.SaveChangesAsync(ct);
        return count;
    }
}
