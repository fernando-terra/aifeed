using Microsoft.EntityFrameworkCore;

namespace AIFeed.Api.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<FeedItem> FeedItems => Set<FeedItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FeedItem>(e =>
        {
            e.HasKey(x => x.Id);

            // SQLite doesn't support array columns — store tags as CSV
            e.Property(x => x.Tags)
             .HasConversion(
                 v => string.Join(',', v),
                 v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));

            // SQLite doesn't support DateTimeOffset natively — store as Unix seconds (long)
            e.Property(x => x.PublishedAt)
             .HasConversion(
                 v => v.ToUnixTimeSeconds(),
                 v => DateTimeOffset.FromUnixTimeSeconds(v));

            e.Property(x => x.IngestedAt)
             .HasConversion(
                 v => v.ToUnixTimeSeconds(),
                 v => DateTimeOffset.FromUnixTimeSeconds(v));

            e.HasIndex(x => x.Source);
            e.HasIndex(x => x.PublishedAt);
            e.HasIndex(x => x.IngestedAt);

            // Url uniqueness per source prevents duplicate ingestion
            e.HasIndex(x => new { x.Source, x.Url }).IsUnique();
        });
    }
}
