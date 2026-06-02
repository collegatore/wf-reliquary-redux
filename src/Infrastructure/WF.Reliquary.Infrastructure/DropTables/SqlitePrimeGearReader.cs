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
            SELECT
                gvs.gear_name,
                ri.name AS item_name,
                gvs.is_vaulted
            FROM gear_vault_status gvs
            JOIN reward_items ri
                ON ri.item_kind = 'PrimePart'
                AND TRIM(SUBSTR(ri.name, 1, INSTR(ri.name, ' Prime ') + 5)) = gvs.gear_name
            ORDER BY gvs.gear_name, ri.name
            """;

        if (!File.Exists(databasePath))
        {
            return [];
        }

        var entries = new List<PrimeGearEntry>();

        try
        {
            await using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            string? currentGearName = null;
            var currentIsVaulted = false;
            var currentParts = new List<string>();

            while (await reader.ReadAsync(cancellationToken))
            {
                var gearName = reader.GetString(0);
                var itemName = reader.GetString(1);
                var isVaulted = reader.GetInt32(2) == 1;

                if (currentGearName != null && gearName != currentGearName)
                {
                    entries.Add(new PrimeGearEntry(currentGearName, currentParts, currentIsVaulted));
                    currentParts = [];
                }

                currentGearName = gearName;
                currentIsVaulted = isVaulted;
                currentParts.Add(DeriveGearAndPart(itemName).PartName);
            }

            if (currentGearName != null)
            {
                entries.Add(new PrimeGearEntry(currentGearName, currentParts, currentIsVaulted));
            }
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 1) // SQLITE_ERROR: table may not exist yet
        {
            return [];
        }

        return entries;
    }

    public static (string GearName, string PartName) DeriveGearAndPart(string itemName)
    {
        var idx = itemName.IndexOf(" Prime ", StringComparison.OrdinalIgnoreCase);
        var gearName = itemName[..(idx + 6)].Trim();
        var partName = itemName[(idx + 7)..].Trim();
        return (gearName, partName);
    }
}
