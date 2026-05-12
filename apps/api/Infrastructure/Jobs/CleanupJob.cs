using AIFeed.Api.Infrastructure.Persistence;
using Arkn.Jobs.Abstractions;
using Arkn.Jobs.Models;
using Arkn.Logging.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AIFeed.Api.Infrastructure.Jobs;

/// <summary>
/// Runs daily at 03:00 UTC. Purges feed items older than 7 days to keep the
/// SQLite file size bounded.
/// </summary>
public sealed class CleanupJob(AppDbContext db, IArknLogger logger) : IArknJob
{
    public async Task<Result> ExecuteAsync(ArknJobContext ctx)
    {
        var cutoff  = DateTimeOffset.UtcNow.AddDays(-7);
        var deleted = await db.FeedItems
            .Where(x => x.IngestedAt < cutoff)
            .ExecuteDeleteAsync(ctx.CancellationToken);

        logger.Info($"[CleanupJob] Purged {deleted} item(s) older than {cutoff:yyyy-MM-dd}.");
        return Result.Success();
    }
}
