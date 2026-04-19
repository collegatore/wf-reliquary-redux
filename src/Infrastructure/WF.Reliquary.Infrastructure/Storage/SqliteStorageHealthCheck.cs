using Microsoft.Data.Sqlite;
using WF.Reliquary.Application.Ports;

namespace WF.Reliquary.Infrastructure.Storage;

public sealed class SqliteStorageHealthCheck : IStorageHealthCheck
{
    public string ProviderName => "SQLite";

    public bool CanCreateConnectionString()
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = ":memory:",
            Mode = SqliteOpenMode.Memory,
            Cache = SqliteCacheMode.Shared
        };

        return builder.ConnectionString.Contains("Data Source=:memory:", StringComparison.Ordinal);
    }
}
