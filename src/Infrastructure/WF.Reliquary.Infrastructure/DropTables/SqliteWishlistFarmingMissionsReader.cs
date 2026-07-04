using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.Wishlist;
using WF.Reliquary.Infrastructure.Storage;

namespace WF.Reliquary.Infrastructure.DropTables;

public sealed class SqliteWishlistFarmingMissionsReader : IWishlistFarmingMissionsReader
{
    private readonly string databasePath;
    private readonly ReliquaryDbExecutor executor;
    private readonly string sql;

    public SqliteWishlistFarmingMissionsReader(string? databasePath = null)
    {
        this.databasePath = databasePath ?? SqliteDropTableImportStore.DefaultDatabasePath();
        executor = new ReliquaryDbExecutor(ReliquaryDbContext.BuildOptions(this.databasePath));
        sql = SqlQueryLoader.Load("GetWishlistFarmingMissions.sql");
    }

    public Task<IReadOnlyList<WishlistFarmingMissionGroupEntry>> GetMissionsForWishlistAsync(
        long wishlistId, CancellationToken cancellationToken = default)
        => executor.RunAsync<IReadOnlyList<WishlistFarmingMissionGroupEntry>>(async (context, ct) =>
        {
            try
            {
                var rows = await context.Database
                    .SqlQueryRaw<FarmingRow>(sql, new SqliteParameter("@wishlist_id", wishlistId))
                    .ToListAsync(ct)
                    .ConfigureAwait(false);

                return BuildGroups(rows);
            }
            catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 1)
            {
                return [];
            }
        }, cancellationToken);

    internal static IReadOnlyList<WishlistFarmingMissionGroupEntry> BuildGroups(List<FarmingRow> rows)
    {
        var entries = BuildEntries(rows);
        return GroupEntries(entries);
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
            var missionType = missionRows[0].MissionType;
            var node = missionRows[0].MissionNode;
            var region = missionRows[0].MissionRegion;
            var hasRotations = missionRows.Any(r => r.RotationId.HasValue);

            if (hasRotations)
            {
                entries.Add(BuildRotationMission(displayLabel, missionType, node, region, missionRows));
            }
            else
            {
                entries.Add(BuildNonRotationMission(displayLabel, missionType, node, region, missionRows));
            }
        }

        return entries.OrderByDescending(e => e.EffectiveChance).ToList();
    }

    internal static IReadOnlyList<WishlistFarmingMissionGroupEntry> GroupEntries(
        IReadOnlyList<WishlistFarmingMissionEntry> entries)
    {
        // Missions with null MissionType become singleton groups.
        // Typed missions are grouped by (MissionType, reward fingerprint).
        var typedGroups = new Dictionary<(string type, string fingerprint), List<WishlistFarmingMissionEntry>>();
        var typedGroupOrder = new List<(string type, string fingerprint)>();
        var singletons = new List<WishlistFarmingMissionEntry>();

        foreach (var entry in entries)
        {
            if (entry.MissionType is null)
            {
                singletons.Add(entry);
                continue;
            }

            var fp = ComputeFingerprint(entry);
            var key = (entry.MissionType, fp);
            if (!typedGroups.TryGetValue(key, out var group))
            {
                group = [];
                typedGroups[key] = group;
                typedGroupOrder.Add(key);
            }
            group.Add(entry);
        }

        var allGroups = new List<WishlistFarmingMissionGroupEntry>();

        // Emit typed groups in the order their first member appeared (already sorted by chance).
        foreach (var key in typedGroupOrder)
        {
            var group = typedGroups[key];
            var representative = group[0];
            var nodes = group
                .Select(NodeLabel)
                .ToList();
            allGroups.Add(new WishlistFarmingMissionGroupEntry(
                GroupTitle: representative.MissionType!,
                Nodes: nodes,
                EffectiveChance: representative.EffectiveChance,
                HasRotations: representative.HasRotations,
                Rotations: representative.Rotations,
                Relics: representative.Relics));
        }

        // Emit singleton groups.
        foreach (var entry in singletons)
        {
            allGroups.Add(new WishlistFarmingMissionGroupEntry(
                GroupTitle: entry.DisplayLabel,
                Nodes: [NodeLabel(entry)],
                EffectiveChance: entry.EffectiveChance,
                HasRotations: entry.HasRotations,
                Rotations: entry.Rotations,
                Relics: entry.Relics));
        }

        // Re-sort the combined list by effective chance descending.
        return allGroups.OrderByDescending(g => g.EffectiveChance).ToList();
    }

    private static string NodeLabel(WishlistFarmingMissionEntry entry)
    {
        if (entry.Node is null) return entry.DisplayLabel;
        if (entry.Region is null) return entry.Node;
        return $"{entry.Region}/{entry.Node}";
    }

    private static string ComputeFingerprint(WishlistFarmingMissionEntry entry)
    {
        if (entry.HasRotations)
        {
            var rotParts = entry.Rotations
                .OrderBy(r => r.RotationDisplayLabel)
                .Select(r =>
                {
                    var relicParts = r.Relics
                        .Select(rel => $"{rel.RelicName}:{rel.ChancePercent:R}")
                        .OrderBy(s => s);
                    return $"{r.RotationDisplayLabel}:[{string.Join("|", relicParts)}]";
                });
            return string.Join(";", rotParts);
        }
        else
        {
            var relicParts = entry.Relics
                .Select(r => $"{r.RelicName}:{r.ChancePercent:R}")
                .OrderBy(s => s);
            return string.Join("|", relicParts);
        }
    }

    private static WishlistFarmingMissionEntry BuildRotationMission(
        string displayLabel, string? missionType, string? node, string? region, List<FarmingRow> rows)
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

        return new WishlistFarmingMissionEntry(displayLabel, missionType, node, region, effectiveChance, true, rotations, []);
    }

    private static WishlistFarmingMissionEntry BuildNonRotationMission(
        string displayLabel, string? missionType, string? node, string? region, List<FarmingRow> rows)
    {
        var relics = rows
            .Where(r => r.RelicName != null)
            .Select(r => new WishlistFarmingRelicEntry(r.RelicName!, r.RelicChancePercent!.Value))
            .ToList();
        var effectiveChance = relics.Sum(r => r.ChancePercent);
        return new WishlistFarmingMissionEntry(displayLabel, missionType, node, region, effectiveChance, false, [], relics);
    }

    internal sealed class FarmingRow
    {
        public long MissionSourceId { get; set; }
        public string MissionDisplayLabel { get; set; } = string.Empty;
        public int MissionSourceOrder { get; set; }
        public string? MissionType { get; set; }
        public string? MissionNode { get; set; }
        public string? MissionRegion { get; set; }
        public long? RotationId { get; set; }
        public string? RotationDisplayLabel { get; set; }
        public int? RotationSourceOrder { get; set; }
        public string? RelicName { get; set; }
        public double? RelicChancePercent { get; set; }
        public int? RewardSourceOrder { get; set; }
    }
}
