using Microsoft.EntityFrameworkCore;
using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Infrastructure.Storage;
using WF.Reliquary.Infrastructure.Storage.Entities;

namespace WF.Reliquary.Infrastructure.DropTables;

public sealed class LocalDropTableSourceStore : IDropTableSourceStore
{
    private readonly string databasePath;
    private readonly ReliquaryDbExecutor executor;

    public LocalDropTableSourceStore(string? databasePath = null)
    {
        this.databasePath = databasePath ?? DefaultDatabasePath();
        executor = new ReliquaryDbExecutor(ReliquaryDbContext.BuildOptions(this.databasePath));
    }

    public static string DefaultRootPath()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        if (string.IsNullOrWhiteSpace(localAppData))
        {
            localAppData = AppContext.BaseDirectory;
        }

        return Path.Combine(localAppData, "WF Reliquary Redux", "DropTables");
    }

    public static string DefaultDatabasePath()
        => Path.Combine(DefaultRootPath(), "drop-table-import.sqlite");

    public Task<DropTableSourceContent?> LoadAsync(CancellationToken cancellationToken = default)
        => executor.RunAsync<DropTableSourceContent?>(async (context, ct) =>
        {
            var entity = await context.SourceFiles
                .AsNoTracking()
                .OrderByDescending(e => e.Id)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);

            if (entity is null)
            {
                return null;
            }

            return new DropTableSourceContent(
                new Uri(entity.SourceUrl),
                entity.Content,
                DateTimeOffset.Parse(entity.DownloadedAt, null, System.Globalization.DateTimeStyles.RoundtripKind),
                entity.SourceLastUpdate);
        }, cancellationToken);

    public Task<long> SaveAsync(DropTableSourceContent content, CancellationToken cancellationToken = default)
        => executor.RunAsync(async (context, ct) =>
        {
            var entity = new SourceFileEntity
            {
                Content = content.Html,
                SourceUrl = content.SourceUrl?.ToString() ?? string.Empty,
                DownloadedAt = content.DownloadedAt.ToString("O"),
                SourceLastUpdate = content.LastUpdate
            };
            context.SourceFiles.Add(entity);

            await context.SaveChangesAsync(ct).ConfigureAwait(false);

            return entity.Id;
        }, cancellationToken);

    public Task<bool> HasAnyAsync(CancellationToken cancellationToken = default)
        => executor.RunAsync(async (context, ct) =>
        {
            return await context.SourceFiles.AnyAsync(ct).ConfigureAwait(false);
        }, cancellationToken);

    public Task<IReadOnlyList<DropTableSourceSummary>> ListSummariesAsync(CancellationToken cancellationToken = default)
        => executor.RunAsync<IReadOnlyList<DropTableSourceSummary>>(async (context, ct) =>
        {
            return await context.SourceFiles
                .AsNoTracking()
                .OrderByDescending(e => e.Id)
                .Select(e => new DropTableSourceSummary(
                    e.Id,
                    e.SourceUrl,
                    DateTimeOffset.Parse(e.DownloadedAt, null, System.Globalization.DateTimeStyles.RoundtripKind),
                    e.SourceLastUpdate))
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }, cancellationToken);

    public Task<DropTableSourceSummary?> FindSummaryByLastUpdateAsync(string lastUpdate, CancellationToken cancellationToken = default)
        => executor.RunAsync<DropTableSourceSummary?>(async (context, ct) =>
        {
            var entity = await context.SourceFiles
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.SourceLastUpdate == lastUpdate, ct)
                .ConfigureAwait(false);

            if (entity is null)
            {
                return null;
            }

            return new DropTableSourceSummary(
                entity.Id,
                entity.SourceUrl,
                DateTimeOffset.Parse(entity.DownloadedAt, null, System.Globalization.DateTimeStyles.RoundtripKind),
                entity.SourceLastUpdate);
        }, cancellationToken);

    public Task<DropTableSourceContent?> LoadByIdAsync(long id, CancellationToken cancellationToken = default)
        => executor.RunAsync<DropTableSourceContent?>(async (context, ct) =>
        {
            var entity = await context.SourceFiles
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id, ct)
                .ConfigureAwait(false);

            if (entity is null)
            {
                return null;
            }

            return new DropTableSourceContent(
                new Uri(entity.SourceUrl),
                entity.Content,
                DateTimeOffset.Parse(entity.DownloadedAt, null, System.Globalization.DateTimeStyles.RoundtripKind),
                entity.SourceLastUpdate);
        }, cancellationToken);

    public Task ReplaceAsync(long existingId, DropTableSourceContent newContent, CancellationToken cancellationToken = default)
        => executor.RunAsync(async (context, ct) =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct).ConfigureAwait(false);

            var deleted = await context.SourceFiles
                .Where(e => e.Id == existingId)
                .ExecuteDeleteAsync(ct)
                .ConfigureAwait(false);

            if (deleted == 0)
            {
                throw new InvalidOperationException($"Source file with ID {existingId} not found.");
            }

            context.SourceFiles.Add(new SourceFileEntity
            {
                Content = newContent.Html,
                SourceUrl = newContent.SourceUrl?.ToString() ?? string.Empty,
                DownloadedAt = newContent.DownloadedAt.ToString("O"),
                SourceLastUpdate = newContent.LastUpdate
            });

            await context.SaveChangesAsync(ct).ConfigureAwait(false);
            await transaction.CommitAsync(ct).ConfigureAwait(false);
        }, cancellationToken);
}
