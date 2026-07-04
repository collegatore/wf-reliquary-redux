using Microsoft.EntityFrameworkCore;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Infrastructure.Storage;

namespace WF.Reliquary.Infrastructure.DropTables;

public sealed class SqliteImportMetadataReader : IImportMetadataReader
{
    private readonly string databasePath;
    private readonly ReliquaryDbExecutor executor;

    public SqliteImportMetadataReader(string? databasePath = null)
    {
        this.databasePath = databasePath ?? SqliteDropTableImportStore.DefaultDatabasePath();
        executor = new ReliquaryDbExecutor(ReliquaryDbContext.BuildOptions(this.databasePath));
    }

    public Task<string?> GetImportedVersionAsync(CancellationToken cancellationToken = default)
        => executor.RunAsync<string?>(async (context, ct) =>
        {
            var metadata = await context.ImportMetadata
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == 1, ct)
                .ConfigureAwait(false);

            return metadata?.SourceLastUpdate;
        }, cancellationToken);
}
