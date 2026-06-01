using Microsoft.Data.Sqlite;
using WF.Reliquary.Application.PrimeGear;
using WF.Reliquary.Application.Ports;

namespace WF.Reliquary.Infrastructure.DropTables;

public sealed class SqlitePrimeGearReader : IPrimeGearReader
{
    private readonly string databasePath;
    private readonly string connectionString;

    public SqlitePrimeGearReader(string? databasePath = null)
    {
        this.databasePath = databasePath ?? SqliteDropTableImportStore.DefaultDatabasePath();
        connectionString = new SqliteConnectionStringBuilder { DataSource = this.databasePath }.ConnectionString;
    }

    public async Task<IReadOnlyList<PrimeGearEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT name
            FROM reward_items
            WHERE item_kind = 'PrimePart'
            ORDER BY name
            """;

        if (!File.Exists(databasePath))
        {
            return [];
        }

        var names = new List<string>();

        try
        {
            await using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                names.Add(reader.GetString(0));
            }
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 1) // SQLITE_ERROR: table may not exist yet
        {
            return [];
        }

        return names
            .Select(name => (Name: name, Derived: DeriveGearAndPart(name)))
            .GroupBy(x => x.Derived.GearName, StringComparer.Ordinal)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .Select(g => new PrimeGearEntry(
                g.Key,
                g.OrderBy(x => x.Derived.PartName, StringComparer.Ordinal)
                 .Select(x => x.Derived.PartName)
                 .ToList()))
            .ToList();
    }

    public static (string GearName, string PartName) DeriveGearAndPart(string itemName)
    {
        var idx = itemName.IndexOf(" Prime ", StringComparison.OrdinalIgnoreCase);
        var gearName = itemName[..(idx + 6)].Trim();
        var partName = itemName[(idx + 7)..].Trim();
        return (gearName, partName);
    }
}
