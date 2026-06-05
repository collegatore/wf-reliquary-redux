using Microsoft.EntityFrameworkCore;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Infrastructure.Storage;

namespace WF.Reliquary.Infrastructure.DropTables;

public sealed class SqliteImportMetadataReader : IImportMetadataReader
{
    private readonly string databasePath;
    private readonly DbContextOptions<ReliquaryDbContext> options;

    public SqliteImportMetadataReader(string? databasePath = null)
    {
        this.databasePath = databasePath ?? SqliteDropTableImportStore.DefaultDatabasePath();
        options = ReliquaryDbContext.BuildOptions(this.databasePath);
    }

    public async Task<string?> GetImportedVersionAsync(CancellationToken ct = default)
    {
        if (!File.Exists(databasePath))
        {
            return null;
        }

        await using var context = new ReliquaryDbContext(options);
        var metadata = await context.ImportMetadata
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == 1, ct);

        return metadata?.SourceLastUpdate;
    }
}
