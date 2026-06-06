using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.Wishlist;
using WF.Reliquary.Infrastructure.Storage;

namespace WF.Reliquary.Infrastructure.DropTables;

public sealed class SqliteWishlistFarmingMissionsReader : IWishlistFarmingMissionsReader
{
    private readonly string databasePath;
    private readonly DbContextOptions<ReliquaryDbContext> options;
    private readonly string sql;

    public SqliteWishlistFarmingMissionsReader(string? databasePath = null)
    {
        this.databasePath = databasePath ?? SqliteDropTableImportStore.DefaultDatabasePath();
        options = ReliquaryDbContext.BuildOptions(this.databasePath);
        sql = SqlQueryLoader.Load("GetWishlistFarmingMissions.sql");
    }

    public async Task<IReadOnlyList<WishlistFarmingMissionEntry>> GetMissionsForWishlistAsync(
        long wishlistId, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(databasePath))
        {
            return [];
        }

        try
        {
            await using var context = new ReliquaryDbContext(options);
            var rows = await context.Database
                .SqlQueryRaw<FarmingRow>(sql, new SqliteParameter("@wishlist_id", wishlistId))
                .ToListAsync(cancellationToken);

            return BuildEntries(rows);
        }
        catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 1)
        {
            return [];
        }
    }

    internal static IReadOnlyList<WishlistFarmingMissionEntry> BuildEntries(List<FarmingRow> rows)
    {
        var seenMissions = new Dictionary<long, List<FarmingRow>>();
        var missionOrder = new List<long>();

        foreach (var row in rows)
        {
            if (!seenMissions.TryGetValue(row.MissionSourceId, out var missionRows))
            {
                missionRows = [];
                seenMissions[row.MissionSourceId] = missionRows;
                missionOrder.Add(row.MissionSourceId);
            }
            missionRows.Add(row);
        }

        var entries = new List<WishlistFarmingMissionEntry>(missionOrder.Count);

        foreach (var missionId in missionOrder)
        {
            var missionRows = seenMissions[missionId];
            var displayLabel = missionRows[0].MissionDisplayLabel;
            var hasRotations = missionRows.Any(r => r.RotationId.HasValue);

            if (hasRotations)
            {
                entries.Add(BuildRotationMission(displayLabel, missionRows));
            }
            else
            {
                entries.Add(BuildNonRotationMission(displayLabel, missionRows));
            }
        }

        return entries.OrderByDescending(e => e.EffectiveChance).ToList();
    }

    private static WishlistFarmingMissionEntry BuildRotationMission(string displayLabel, List<FarmingRow> rows)
    {
        var seenRotations = new Dictionary<long, List<FarmingRow>>();
        var rotationOrder = new List<long>();

        foreach (var row in rows)
        {
            var rotId = row.RotationId!.Value;
            if (!seenRotations.TryGetValue(rotId, out var rotRows))
            {
                rotRows = [];
                seenRotations[rotId] = rotRows;
                rotationOrder.Add(rotId);
            }
            if (row.RelicName != null)
            {
                rotRows.Add(row);
            }
        }

        var rotations = new List<WishlistFarmingRotationEntry>(rotationOrder.Count);
        foreach (var rotId in rotationOrder)
        {
            var rotRows = seenRotations[rotId];
            var rotLabel = rows.First(r => r.RotationId == rotId).RotationDisplayLabel!;
            var relics = rotRows
                .Select(r => new WishlistFarmingRelicEntry(r.RelicName!, r.RelicChancePercent!.Value))
                .ToList();
            var cumChance = relics.Sum(r => r.ChancePercent);
            rotations.Add(new WishlistFarmingRotationEntry(rotLabel, cumChance, relics));
        }

        var effectiveChance = rotations.Count > 0
            ? rotations.Average(r => r.CumulativeChance)
            : 0.0;

        return new WishlistFarmingMissionEntry(displayLabel, effectiveChance, true, rotations, []);
    }

    private static WishlistFarmingMissionEntry BuildNonRotationMission(string displayLabel, List<FarmingRow> rows)
    {
        var relics = rows
            .Where(r => r.RelicName != null)
            .Select(r => new WishlistFarmingRelicEntry(r.RelicName!, r.RelicChancePercent!.Value))
            .ToList();
        var effectiveChance = relics.Sum(r => r.ChancePercent);
        return new WishlistFarmingMissionEntry(displayLabel, effectiveChance, false, [], relics);
    }

    internal sealed class FarmingRow
    {
        public long MissionSourceId { get; set; }
        public string MissionDisplayLabel { get; set; } = string.Empty;
        public int MissionSourceOrder { get; set; }
        public long? RotationId { get; set; }
        public string? RotationDisplayLabel { get; set; }
        public int? RotationSourceOrder { get; set; }
        public string? RelicName { get; set; }
        public double? RelicChancePercent { get; set; }
        public int? RewardSourceOrder { get; set; }
    }
}
