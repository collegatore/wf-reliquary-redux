using Microsoft.EntityFrameworkCore;
using WF.Reliquary.Application.PrimeGear;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Infrastructure.Storage;

namespace WF.Reliquary.Infrastructure.DropTables;

public sealed class SqlitePrimeGearReader : IPrimeGearReader
{
    private readonly string databasePath;
    private readonly DbContextOptions<ReliquaryDbContext> options;
    private readonly string primeGearSql;

    public SqlitePrimeGearReader(string? databasePath = null)
    {
        this.databasePath = databasePath ?? SqliteDropTableImportStore.DefaultDatabasePath();
        options = ReliquaryDbContext.BuildOptions(this.databasePath);
        primeGearSql = SqlQueryLoader.Load("GetPrimeGear.sql");
    }

    public async Task<IReadOnlyList<PrimeGearEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(databasePath))
        {
            return [];
        }

        try
        {
            await using var context = new ReliquaryDbContext(options);
            var rows = await context.Database
                .SqlQueryRaw<PrimeGearRow>(primeGearSql)
                .ToListAsync(cancellationToken);

            return BuildEntries(rows);
        }
        catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 1)
        {
            return [];
        }
    }

    private static IReadOnlyList<PrimeGearEntry> BuildEntries(List<PrimeGearRow> rows)
    {
        var entries = new List<PrimeGearEntry>();
        string? currentGearName = null;
        var currentIsVaulted = false;
        var currentParts = new List<string>();

        foreach (var row in rows)
        {
            if (currentGearName != null && row.GearName != currentGearName)
            {
                entries.Add(new PrimeGearEntry(currentGearName, currentParts, currentIsVaulted));
                currentParts = [];
            }

            currentGearName = row.GearName;
            currentIsVaulted = row.IsVaulted != 0;
            currentParts.Add(DerivePartName(row.ItemName));
        }

        if (currentGearName != null)
        {
            entries.Add(new PrimeGearEntry(currentGearName, currentParts, currentIsVaulted));
        }

        return entries;
    }

    private static string DerivePartName(string itemName)
    {
        var idx = itemName.IndexOf(" Prime ", StringComparison.OrdinalIgnoreCase);
        return itemName[(idx + 7)..].Trim();
    }

    public static (string GearName, string PartName) DeriveGearAndPart(string itemName)
    {
        var idx = itemName.IndexOf(" Prime ", StringComparison.OrdinalIgnoreCase);
        var gearName = itemName[..(idx + 6)].Trim();
        var partName = itemName[(idx + 7)..].Trim();
        return (gearName, partName);
    }

    private sealed class PrimeGearRow
    {
        public string GearName { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public int IsVaulted { get; set; }
    }
}
