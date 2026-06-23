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

    public Task<StoredDropTableSource?> LoadAsync(CancellationToken cancellationToken = default)
        => executor.RunAsync<StoredDropTableSource?>(async (context, ct) =>
        {
            if (!File.Exists(databasePath))
            {
                return null;
            }

            var entity = await context.SourceFiles
                .AsNoTracking()
                .OrderByDescending(e => e.Id)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);

            if (entity is null)
            {
                return null;
            }

            var provenance = new DropTableSourceProvenance(
                new Uri(entity.SourceUrl),
                DateTimeOffset.Parse(entity.DownloadedAt, null, System.Globalization.DateTimeStyles.RoundtripKind),
                entity.SourceLastUpdate);

            return new StoredDropTableSource(entity.Content, provenance);
        }, cancellationToken);

    public Task SaveAsync(DropTableSourceContent content, CancellationToken cancellationToken = default)
        => executor.RunAsync(async (context, ct) =>
        {
            var directory = Path.GetDirectoryName(databasePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await context.Database.MigrateAsync(ct).ConfigureAwait(false);

            context.SourceFiles.Add(new SourceFileEntity
            {
                Content = content.Html,
                SourceUrl = content.SourceUrl?.ToString(),
                DownloadedAt = content.DownloadedAt.ToString("O"),
                SourceLastUpdate = content.LastUpdate
            });

            await context.SaveChangesAsync(ct).ConfigureAwait(false);
        }, cancellationToken);

    public Task<bool> HasAnyAsync(CancellationToken cancellationToken = default)
        => executor.RunAsync(async (context, ct) =>
        {
            if (!File.Exists(databasePath))
            {
                return false;
            }

            return await context.SourceFiles.AnyAsync(ct).ConfigureAwait(false);
        }, cancellationToken);
}
