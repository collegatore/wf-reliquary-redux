using NUnit.Framework;
using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Infrastructure.DropTables;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class WishlistFarmingMissionsInfrastructureTests
{
    // ── GetMissionsForWishlistAsync ───────────────────────────────────────────

    [Test]
    public async Task NonRotationMission_EffectiveChance_IsPoolSum()
    {
        var dbPath = TempDatabasePath();
        await SeedImportAsync(dbPath);
        var wishlistStore = new SqliteWishlistStore(dbPath);
        await wishlistStore.SeedDefaultAsync();
        var wishlist = await wishlistStore.GetActiveAsync();

        // Saryn Prime Neuroptics is in Lith B5 (→ Hepit at 11.11%)
        // Saryn Prime Systems is in Lith S13 (→ Hepit at 7.41%)
        await wishlistStore.AddItemAsync(wishlist!.Id, "SARYN PRIME NEUROPTICS", "Saryn Prime", "Neuroptics");
        await wishlistStore.AddItemAsync(wishlist.Id, "SARYN PRIME SYSTEMS", "Saryn Prime", "Systems");

        var reader = new SqliteWishlistFarmingMissionsReader(dbPath);
        var missions = await reader.GetMissionsForWishlistAsync(wishlist.Id);

        var hepit = missions.Single(m => m.Nodes.Any(n => n.Contains("Hepit", StringComparison.OrdinalIgnoreCase)));
        Assert.That(hepit.HasRotations, Is.False);
        Assert.That(hepit.EffectiveChance, Is.EqualTo(11.11 + 7.41).Within(0.001));
    }

    [Test]
    public async Task RotationMission_EffectiveChance_IsAverageAcrossAllRotationsIncludingZero()
    {
        var dbPath = TempDatabasePath();
        await SeedImportAsync(dbPath);
        var wishlistStore = new SqliteWishlistStore(dbPath);
        await wishlistStore.SeedDefaultAsync();
        var wishlist = await wishlistStore.GetActiveAsync();

        // Saryn Prime Chassis is in Meso N16 (→ Mot Rotation A at 18.00%)
        // Rotations B and C have no wishlisted relic rewards → 0%
        await wishlistStore.AddItemAsync(wishlist!.Id, "SARYN PRIME CHASSIS", "Saryn Prime", "Chassis");

        var reader = new SqliteWishlistFarmingMissionsReader(dbPath);
        var missions = await reader.GetMissionsForWishlistAsync(wishlist.Id);

        var mot = missions.Single(m => m.Nodes.Any(n => n.Contains("Mot", StringComparison.OrdinalIgnoreCase)));
        Assert.That(mot.HasRotations, Is.True);

        // avg(18, 0, 0) = 6
        Assert.That(mot.EffectiveChance, Is.EqualTo(6.0).Within(0.001));
    }

    [Test]
    public async Task RotationMission_ZeroChanceRotations_AreIncluded()
    {
        var dbPath = TempDatabasePath();
        await SeedImportAsync(dbPath);
        var wishlistStore = new SqliteWishlistStore(dbPath);
        await wishlistStore.SeedDefaultAsync();
        var wishlist = await wishlistStore.GetActiveAsync();

        await wishlistStore.AddItemAsync(wishlist!.Id, "SARYN PRIME CHASSIS", "Saryn Prime", "Chassis");

        var reader = new SqliteWishlistFarmingMissionsReader(dbPath);
        var missions = await reader.GetMissionsForWishlistAsync(wishlist.Id);

        var mot = missions.Single(m => m.Nodes.Any(n => n.Contains("Mot", StringComparison.OrdinalIgnoreCase)));
        Assert.That(mot.Rotations, Has.Count.EqualTo(3));

        var rotA = mot.Rotations.Single(r => r.RotationDisplayLabel == "Rotation A");
        Assert.That(rotA.CumulativeChance, Is.EqualTo(18.0).Within(0.001));
        Assert.That(rotA.Relics, Has.Count.EqualTo(1));

        var rotB = mot.Rotations.Single(r => r.RotationDisplayLabel == "Rotation B");
        Assert.That(rotB.CumulativeChance, Is.EqualTo(0.0));
        Assert.That(rotB.Relics, Is.Empty);

        var rotC = mot.Rotations.Single(r => r.RotationDisplayLabel == "Rotation C");
        Assert.That(rotC.CumulativeChance, Is.EqualTo(0.0));
        Assert.That(rotC.Relics, Is.Empty);
    }

    [Test]
    public async Task Missions_AreOrderedByEffectiveChanceDescending()
    {
        var dbPath = TempDatabasePath();
        await SeedImportAsync(dbPath);
        var wishlistStore = new SqliteWishlistStore(dbPath);
        await wishlistStore.SeedDefaultAsync();
        var wishlist = await wishlistStore.GetActiveAsync();

        // Both missions have wishlisted relics:
        //   Hepit effective = 11.11 + 7.41 = 18.52%
        //   Mot effective   = 18.00 / 3    =  6.00%
        await wishlistStore.AddItemAsync(wishlist!.Id, "SARYN PRIME NEUROPTICS", "Saryn Prime", "Neuroptics");
        await wishlistStore.AddItemAsync(wishlist.Id, "SARYN PRIME SYSTEMS", "Saryn Prime", "Systems");
        await wishlistStore.AddItemAsync(wishlist.Id, "SARYN PRIME CHASSIS", "Saryn Prime", "Chassis");

        var reader = new SqliteWishlistFarmingMissionsReader(dbPath);
        var missions = await reader.GetMissionsForWishlistAsync(wishlist.Id);

        Assert.That(missions, Has.Count.EqualTo(2));
        Assert.That(missions[0].Nodes, Has.Some.Contains("Hepit"));
        Assert.That(missions[1].Nodes, Has.Some.Contains("Mot"));
        Assert.That(missions[0].EffectiveChance, Is.GreaterThan(missions[1].EffectiveChance));
    }

    [Test]
    public async Task EmptyWishlist_ReturnsNoMissions()
    {
        var dbPath = TempDatabasePath();
        await SeedImportAsync(dbPath);
        var wishlistStore = new SqliteWishlistStore(dbPath);
        await wishlistStore.SeedDefaultAsync();
        var wishlist = await wishlistStore.GetActiveAsync();

        var reader = new SqliteWishlistFarmingMissionsReader(dbPath);
        var missions = await reader.GetMissionsForWishlistAsync(wishlist!.Id);

        Assert.That(missions, Is.Empty);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string TempDatabasePath()
    {
        var dir = Path.Combine(Path.GetTempPath(), "wf-reliquary-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "import.sqlite");
    }

    private static async Task SeedImportAsync(string dbPath)
    {
        var store = new SqliteDropTableImportStore(dbPath);
        await store.ReplaceAsync(BuildImport());
    }

    private static ParsedDropTableImport BuildImport() => new(
        SourceUrl: null,
        SourcePath: "test.html",
        DownloadedAt: null,
        SourceLastUpdate: null,
        ImportedAt: DateTimeOffset.UnixEpoch,
        RewardItems:
        [
            new("Lith B5 Relic",          "LITH B5 RELIC",          "Relic"),
            new("Lith S13 Relic",         "LITH S13 RELIC",         "Relic"),
            new("Meso N16 Relic",         "MESO N16 RELIC",         "Relic"),
            new("Saryn Prime Neuroptics", "SARYN PRIME NEUROPTICS", "PrimePart"),
            new("Saryn Prime Systems",    "SARYN PRIME SYSTEMS",    "PrimePart"),
            new("Saryn Prime Chassis",    "SARYN PRIME CHASSIS",    "PrimePart"),
        ],
        Relics:
        [
            new("Lith", "B5",  "Lith B5 Relic",  "Lith B5 Relic"),
            new("Lith", "S13", "Lith S13 Relic", "Lith S13 Relic"),
            new("Meso", "N16", "Meso N16 Relic", "Meso N16 Relic"),
        ],
        RelicRewards:
        [
            new("Lith B5 Relic",  "Saryn Prime Neuroptics", "Common", "Common", 25.33m, 0),
            new("Lith S13 Relic", "Saryn Prime Systems",    "Common", "Common", 25.33m, 0),
            new("Meso N16 Relic", "Saryn Prime Chassis",    "Common", "Common", 25.33m, 0),
        ],
        MissionSources:
        [
            new("Hepit (Void) / Capture", "Void", "Hepit", "Capture", null, 0),
            new("Mot (Void) / Survival",  "Void", "Mot",   "Survival", null, 1),
        ],
        RewardRotations:
        [
            new("Mot (Void) / Survival", 1, "A", "Rotation A", 0),
            new("Mot (Void) / Survival", 1, "B", "Rotation B", 1),
            new("Mot (Void) / Survival", 1, "C", "Rotation C", 2),
        ],
        MissionRewards:
        [
            // Non-rotation mission: Hepit rewards Lith B5 and Lith S13
            new("Hepit (Void) / Capture", 0, null, "Lith B5 Relic",  "Lith B5 Relic",  "Uncommon", 11.11m, 0),
            new("Hepit (Void) / Capture", 0, null, "Lith S13 Relic", "Lith S13 Relic", "Uncommon",  7.41m, 1),
            // Rotation mission: Mot Rotation A rewards Meso N16; B and C have no wishlisted relics
            new("Mot (Void) / Survival",  1, "A", "Meso N16 Relic", "Meso N16 Relic", "Uncommon", 18.00m, 0),
        ],
        Warnings: []);
}
