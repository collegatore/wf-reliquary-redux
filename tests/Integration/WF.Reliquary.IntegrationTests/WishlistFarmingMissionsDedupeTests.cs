using Microsoft.Data.Sqlite;
using NUnit.Framework;
using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Infrastructure.DropTables;

namespace WF.Reliquary.IntegrationTests;

/// <summary>
/// Covers the mission_rewards_merged view: rewards listed more than once within a single
/// pool or rotation are collapsed into one entry whose chance is the sum.
/// </summary>
[TestFixture]
public sealed class WishlistFarmingMissionsDedupeTests
{
    [Test]
    public async Task NonRotation_DuplicateRelic_IsMergedWithSummedChance()
    {
        var dbPath = TempDatabasePath();
        await SeedAsync(dbPath, DuplicateNonRotationImport());
        var wishlistId = await ActiveWishlistWithPartAsync(dbPath, "SARYN PRIME NEUROPTICS", "Saryn Prime", "Neuroptics");

        var missions = await new SqliteWishlistFarmingMissionsReader(dbPath).GetMissionsForWishlistAsync(wishlistId);

        var group = missions.Single();
        Assert.That(group.HasRotations, Is.False);
        Assert.That(group.Relics, Has.Count.EqualTo(1), "Duplicate relic rows should collapse to one entry.");
        Assert.That(group.Relics[0].ChancePercent, Is.EqualTo(3.0).Within(0.001), "2% + 1% should sum to 3%.");
        Assert.That(group.EffectiveChance, Is.EqualTo(3.0).Within(0.001));
    }

    [Test]
    public async Task Rotation_DuplicateRelic_IsMergedWithSummedChance()
    {
        var dbPath = TempDatabasePath();
        await SeedAsync(dbPath, DuplicateRotationImport());
        var wishlistId = await ActiveWishlistWithPartAsync(dbPath, "SARYN PRIME CHASSIS", "Saryn Prime", "Chassis");

        var missions = await new SqliteWishlistFarmingMissionsReader(dbPath).GetMissionsForWishlistAsync(wishlistId);

        var group = missions.Single();
        Assert.That(group.HasRotations, Is.True);
        var rotA = group.Rotations.Single(r => r.RotationDisplayLabel == "Rotation A");
        Assert.That(rotA.Relics, Has.Count.EqualTo(1), "Duplicate relic rows in one rotation should collapse to one entry.");
        Assert.That(rotA.Relics[0].ChancePercent, Is.EqualTo(3.0).Within(0.001));
        Assert.That(rotA.CumulativeChance, Is.EqualTo(3.0).Within(0.001));
    }

    [Test]
    public async Task SameTypeMissions_IdenticalOnlyAfterMerge_AreGroupedTogether()
    {
        var dbPath = TempDatabasePath();
        await SeedAsync(dbPath, TwoMissionsIdenticalAfterMergeImport());
        var wishlistId = await ActiveWishlistWithPartAsync(dbPath, "SARYN PRIME NEUROPTICS", "Saryn Prime", "Neuroptics");

        var missions = await new SqliteWishlistFarmingMissionsReader(dbPath).GetMissionsForWishlistAsync(wishlistId);

        // Mission 1 lists the relic as 2% + 1% (merges to 3%); mission 2 lists it once at 3%.
        // After merge both reward tables are identical → one Capture group with two nodes.
        var group = missions.Single();
        Assert.That(group.GroupTitle, Is.EqualTo("Capture"));
        Assert.That(group.Nodes, Has.Count.EqualTo(2));
        Assert.That(group.Relics[0].ChancePercent, Is.EqualTo(3.0).Within(0.001));
    }

    [Test]
    public async Task RewardItem_ToRelic_IsOneToOne_InRealSample()
    {
        var dbPath = TempDatabasePath();
        await SeedFullSampleAsync(dbPath);

        // The view carries relic_id as a passthrough on a group keyed by reward_item_id;
        // that is only well-defined if no reward item maps to more than one relic.
        var violations = ScalarLong(dbPath, """
            SELECT COUNT(*) FROM (
                SELECT reward_item_id
                FROM mission_rewards
                WHERE relic_id IS NOT NULL
                GROUP BY reward_item_id
                HAVING COUNT(DISTINCT relic_id) > 1
            );
            """);

        Assert.That(violations, Is.EqualTo(0),
            "A reward item mapped to multiple relics would make the merged view's relic_id passthrough ambiguous.");
    }

    // ── Fixtures ──────────────────────────────────────────────────────────────

    private static ParsedDropTableImport DuplicateNonRotationImport() => new(
        SourceUrl: new Uri(DropTableSourceDefaults.SourceUrl), DownloadedAt: DateTimeOffset.UnixEpoch, SourceLastUpdate: "08 April, 2026",
        ImportedAt: DateTimeOffset.UnixEpoch,
        RewardItems:
        [
            new("Lith B5 Relic", "LITH B5 RELIC", "Relic"),
            new("Saryn Prime Neuroptics", "SARYN PRIME NEUROPTICS", "PrimePart"),
        ],
        Relics: [new("Lith", "B5", "Lith B5 Relic", "Lith B5 Relic")],
        RelicRewards: [new("Lith B5 Relic", "Saryn Prime Neuroptics", "Common", "Common", 25.33m, 0)],
        MissionSources: [new("Hepit (Void) / Capture", "Void", "Hepit", "Capture", null, 0)],
        RewardRotations: [],
        MissionRewards:
        [
            // Same relic listed twice in the same non-rotation pool.
            new("Hepit (Void) / Capture", 0, null, "Lith B5 Relic", "Lith B5 Relic", "Uncommon", 2.00m, 0),
            new("Hepit (Void) / Capture", 0, null, "Lith B5 Relic", "Lith B5 Relic", "Uncommon", 1.00m, 1),
        ],
        Warnings: []);

    private static ParsedDropTableImport DuplicateRotationImport() => new(
        SourceUrl: new Uri(DropTableSourceDefaults.SourceUrl), DownloadedAt: DateTimeOffset.UnixEpoch, SourceLastUpdate: "08 April, 2026",
        ImportedAt: DateTimeOffset.UnixEpoch,
        RewardItems:
        [
            new("Meso N16 Relic", "MESO N16 RELIC", "Relic"),
            new("Saryn Prime Chassis", "SARYN PRIME CHASSIS", "PrimePart"),
        ],
        Relics: [new("Meso", "N16", "Meso N16 Relic", "Meso N16 Relic")],
        RelicRewards: [new("Meso N16 Relic", "Saryn Prime Chassis", "Common", "Common", 25.33m, 0)],
        MissionSources: [new("Mot (Void) / Survival", "Void", "Mot", "Survival", null, 0)],
        RewardRotations: [new("Mot (Void) / Survival", 0, "A", "Rotation A", 0)],
        MissionRewards:
        [
            // Same relic listed twice in one rotation.
            new("Mot (Void) / Survival", 0, "A", "Meso N16 Relic", "Meso N16 Relic", "Uncommon", 2.00m, 0),
            new("Mot (Void) / Survival", 0, "A", "Meso N16 Relic", "Meso N16 Relic", "Uncommon", 1.00m, 1),
        ],
        Warnings: []);

    private static ParsedDropTableImport TwoMissionsIdenticalAfterMergeImport() => new(
        SourceUrl: new Uri(DropTableSourceDefaults.SourceUrl), DownloadedAt: DateTimeOffset.UnixEpoch, SourceLastUpdate: "08 April, 2026",
        ImportedAt: DateTimeOffset.UnixEpoch,
        RewardItems:
        [
            new("Lith B5 Relic", "LITH B5 RELIC", "Relic"),
            new("Saryn Prime Neuroptics", "SARYN PRIME NEUROPTICS", "PrimePart"),
        ],
        Relics: [new("Lith", "B5", "Lith B5 Relic", "Lith B5 Relic")],
        RelicRewards: [new("Lith B5 Relic", "Saryn Prime Neuroptics", "Common", "Common", 25.33m, 0)],
        MissionSources:
        [
            new("Hepit (Void) / Capture", "Void", "Hepit", "Capture", null, 0),
            new("Ukko (Void) / Capture", "Void", "Ukko", "Capture", null, 1),
        ],
        RewardRotations: [],
        MissionRewards:
        [
            // Mission 0: relic listed twice (2% + 1% → 3%).
            new("Hepit (Void) / Capture", 0, null, "Lith B5 Relic", "Lith B5 Relic", "Uncommon", 2.00m, 0),
            new("Hepit (Void) / Capture", 0, null, "Lith B5 Relic", "Lith B5 Relic", "Uncommon", 1.00m, 1),
            // Mission 1: relic listed once at 3%.
            new("Ukko (Void) / Capture", 1, null, "Lith B5 Relic", "Lith B5 Relic", "Uncommon", 3.00m, 0),
        ],
        Warnings: []);

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static async Task SeedAsync(string dbPath, ParsedDropTableImport import)
        => await new SqliteDropTableImportStore(dbPath).ReplaceAsync(import);

    private static async Task SeedFullSampleAsync(string dbPath)
    {
        var samplePath = Path.Combine(AppContext.BaseDirectory, "TestData", "warframe-drop-tables-sample.html");
        var html = await File.ReadAllTextAsync(samplePath);
        var source = new DropTableSourceContent(
            new Uri(DropTableSourceDefaults.SourceUrl), html, DateTimeOffset.UnixEpoch, "08 April, 2026");
        var import = new HtmlDropTableParser().Parse(source, DateTimeOffset.UnixEpoch);
        await new SqliteDropTableImportStore(dbPath).ReplaceAsync(import);
    }

    private static async Task<long> ActiveWishlistWithPartAsync(
        string dbPath, string partNormalized, string gear, string partDisplay)
    {
        var store = new SqliteWishlistStore(dbPath);
        await store.SeedDefaultAsync();
        var wishlist = await store.GetActiveAsync();
        await store.AddItemAsync(wishlist!.Id, partNormalized, gear, partDisplay);
        return wishlist.Id;
    }

    private static long ScalarLong(string dbPath, string sql)
    {
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = dbPath }.ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        return Convert.ToInt64(command.ExecuteScalar());
    }

    private static string TempDatabasePath()
    {
        var dir = Path.Combine(Path.GetTempPath(), "wf-reliquary-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "import.sqlite");
    }
}
