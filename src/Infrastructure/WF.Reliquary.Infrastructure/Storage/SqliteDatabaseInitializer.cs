using Microsoft.EntityFrameworkCore;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Infrastructure.DropTables;

namespace WF.Reliquary.Infrastructure.Storage;

public sealed class SqliteDatabaseInitializer : IDatabaseInitializer
{
    private readonly ReliquaryDbExecutor executor;
    private readonly string databasePath;

    public SqliteDatabaseInitializer(string? databasePath = null)
    {
        this.databasePath = databasePath ?? LocalDropTableSourceStore.DefaultDatabasePath();
        executor = new ReliquaryDbExecutor(ReliquaryDbContext.BuildOptions(this.databasePath));
    }

    public Task InitializeAsync(CancellationToken cancellationToken = default)
        => executor.RunAsync(async (context, ct) =>
        {
            var directory = Path.GetDirectoryName(databasePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await context.Database.MigrateAsync(ct).ConfigureAwait(false);
        }, cancellationToken);
}
