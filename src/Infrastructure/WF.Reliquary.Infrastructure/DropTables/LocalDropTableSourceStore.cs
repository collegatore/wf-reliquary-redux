using Microsoft.EntityFrameworkCore;
using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Infrastructure.Storage;
using WF.Reliquary.Infrastructure.Storage.Entities;

namespace WF.Reliquary.Infrastructure.DropTables;

public sealed class LocalDropTableSourceStore : IDropTableSourceStore
{
    private readonly string databasePath;
    private readonly DbContextOptions<ReliquaryDbContext> contextOptions;

    public LocalDropTableSourceStore(string? databasePath = null)
    {
        this.databasePath = databasePath ?? DefaultDatabasePath();
        contextOptions = ReliquaryDbContext.BuildOptions(this.databasePath);
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

    public async Task<StoredDropTableSource?> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(databasePath))
        {
            return null;
        }

        await using var context = new ReliquaryDbContext(contextOptions);
        var entity = await context.SourceFiles
            .AsNoTracking()
            .OrderByDescending(e => e.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
        {
            return null;
        }

        var provenance = new DropTableSourceProvenance(
            new Uri(entity.SourceUrl),
            DateTimeOffset.Parse(entity.DownloadedAt, null, System.Globalization.DateTimeStyles.RoundtripKind),
            entity.SourceLastUpdate);

        return new StoredDropTableSource(entity.Content, provenance);
    }

    public async Task SaveAsync(DropTableSourceContent content, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var context = new ReliquaryDbContext(contextOptions);
        await context.Database.MigrateAsync(cancellationToken);

        context.SourceFiles.Add(new SourceFileEntity
        {
            Content = content.Html,
            SourceUrl = content.SourceUrl?.ToString(),
            DownloadedAt = content.DownloadedAt.ToString("O"),
            SourceLastUpdate = content.LastUpdate
        });

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> HasAnyAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(databasePath))
        {
            return false;
        }

        await using var context = new ReliquaryDbContext(contextOptions);
        return await context.SourceFiles.AnyAsync(cancellationToken);
    }
}
