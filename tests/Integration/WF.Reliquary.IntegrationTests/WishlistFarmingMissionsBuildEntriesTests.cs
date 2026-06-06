using NUnit.Framework;
using WF.Reliquary.Infrastructure.DropTables;
using static WF.Reliquary.Infrastructure.DropTables.SqliteWishlistFarmingMissionsReader;

namespace WF.Reliquary.IntegrationTests;

/// <summary>
/// White-box tests for <see cref="SqliteWishlistFarmingMissionsReader.BuildEntries"/>.
/// Exercises grouping, chance accumulation, zero-chance rotations, and sort order
/// without hitting the database.
/// </summary>
[TestFixture]
public sealed class WishlistFarmingMissionsBuildEntriesTests
{
    // ── Non-rotation missions ─────────────────────────────────────────────────

    [Test]
    public void NonRotationMission_EffectiveChance_IsSumOfRelicChances()
    {
        var rows = new List<FarmingRow>
        {
            Row(missionId: 1, missionLabel: "Hepit", missionOrder: 0,
                rotationId: null, rotationLabel: null, rotationOrder: null,
                relicName: "Lith B5 Relic", chance: 11.11, rewardOrder: 0),
            Row(missionId: 1, missionLabel: "Hepit", missionOrder: 0,
                rotationId: null, rotationLabel: null, rotationOrder: null,
                relicName: "Lith S13 Relic", chance: 7.41, rewardOrder: 1),
        };

        var entries = BuildEntries(rows);

        Assert.That(entries, Has.Count.EqualTo(1));
        Assert.That(entries[0].HasRotations, Is.False);
        Assert.That(entries[0].EffectiveChance, Is.EqualTo(18.52).Within(0.001));
        Assert.That(entries[0].Relics, Has.Count.EqualTo(2));
        Assert.That(entries[0].Rotations, Is.Empty);
    }

    [Test]
    public void NonRotationMission_RelicsListedWithCorrectChances()
    {
        var rows = new List<FarmingRow>
        {
            Row(1, "Hepit", 0, null, null, null, "Lith B5 Relic", 11.11, 0),
            Row(1, "Hepit", 0, null, null, null, "Lith S13 Relic", 7.41, 1),
        };

        var entries = BuildEntries(rows);

        var relics = entries[0].Relics;
        Assert.That(relics.Any(r => r.RelicName == "Lith B5 Relic" && r.ChancePercent == 11.11), Is.True);
        Assert.That(relics.Any(r => r.RelicName == "Lith S13 Relic" && r.ChancePercent == 7.41), Is.True);
    }

    // ── Rotation missions ─────────────────────────────────────────────────────

    [Test]
    public void RotationMission_EffectiveChance_IsAverageAcrossAllRotations()
    {
        var rows = RotationRows(
            missionId: 10, missionLabel: "Mot", missionOrder: 1,
            rotations:
            [
                (rotId: 1L, rotLabel: "Rotation A", rotOrder: 0, relicName: "Meso N16 Relic", chance: 18.00),
                (rotId: 2L, rotLabel: "Rotation B", rotOrder: 1, relicName: null,             chance: null),
                (rotId: 3L, rotLabel: "Rotation C", rotOrder: 2, relicName: null,             chance: null),
            ]);

        var entries = BuildEntries(rows);

        Assert.That(entries[0].HasRotations, Is.True);
        Assert.That(entries[0].EffectiveChance, Is.EqualTo(6.0).Within(0.001)); // avg(18, 0, 0)
    }

    [Test]
    public void RotationMission_ZeroChanceRotations_AppearWithEmptyRelicsAndZeroChance()
    {
        var rows = RotationRows(
            missionId: 10, missionLabel: "Mot", missionOrder: 1,
            rotations:
            [
                (1L, "Rotation A", 0, "Meso N16 Relic", 18.00),
                (2L, "Rotation B", 1, null, null),
                (3L, "Rotation C", 2, null, null),
            ]);

        var entries = BuildEntries(rows);

        Assert.That(entries[0].Rotations, Has.Count.EqualTo(3));

        var rotA = entries[0].Rotations.Single(r => r.RotationDisplayLabel == "Rotation A");
        Assert.That(rotA.CumulativeChance, Is.EqualTo(18.0).Within(0.001));
        Assert.That(rotA.Relics, Has.Count.EqualTo(1));

        var rotB = entries[0].Rotations.Single(r => r.RotationDisplayLabel == "Rotation B");
        Assert.That(rotB.CumulativeChance, Is.EqualTo(0.0));
        Assert.That(rotB.Relics, Is.Empty);

        var rotC = entries[0].Rotations.Single(r => r.RotationDisplayLabel == "Rotation C");
        Assert.That(rotC.CumulativeChance, Is.EqualTo(0.0));
        Assert.That(rotC.Relics, Is.Empty);
    }

    [Test]
    public void RotationMission_MissionLevelRelics_AreEmpty()
    {
        var rows = RotationRows(
            missionId: 10, missionLabel: "Mot", missionOrder: 1,
            rotations: [(1L, "Rotation A", 0, "Meso N16 Relic", 18.00)]);

        var entries = BuildEntries(rows);

        Assert.That(entries[0].Relics, Is.Empty);
    }

    // ── Sort order ────────────────────────────────────────────────────────────

    [Test]
    public void Entries_AreSortedByEffectiveChanceDescending()
    {
        var nonRotation = new List<FarmingRow>
        {
            Row(1, "Hepit", 0, null, null, null, "Lith B5 Relic",  11.11, 0),
            Row(1, "Hepit", 0, null, null, null, "Lith S13 Relic",  7.41, 1),
        };
        // Feed rotation mission first in input to verify sort overrides insertion order.
        var rotation = RotationRows(
            missionId: 10, missionLabel: "Mot", missionOrder: 1,
            rotations:
            [
                (1L, "Rotation A", 0, "Meso N16 Relic", 18.00),
                (2L, "Rotation B", 1, null, null),
                (3L, "Rotation C", 2, null, null),
            ]);

        var entries = BuildEntries([.. rotation, .. nonRotation]);

        Assert.That(entries[0].DisplayLabel, Is.EqualTo("Hepit")); // 18.52%
        Assert.That(entries[1].DisplayLabel, Is.EqualTo("Mot"));   //  6.00%
    }

    [Test]
    public void MultipleNonRotationMissions_SortedByEffectiveChanceDescending()
    {
        var rows = new List<FarmingRow>
        {
            Row(1, "Low Mission",  0, null, null, null, "Lith X1 Relic",  5.00, 0),
            Row(2, "High Mission", 1, null, null, null, "Lith X2 Relic", 20.00, 0),
        };

        var entries = BuildEntries(rows);

        Assert.That(entries[0].DisplayLabel, Is.EqualTo("High Mission"));
        Assert.That(entries[1].DisplayLabel, Is.EqualTo("Low Mission"));
    }

    // ── Grouping ──────────────────────────────────────────────────────────────

    [Test]
    public void MultipleMissions_GroupedCorrectly()
    {
        var rows = new List<FarmingRow>
        {
            Row(1, "MissionA", 0, null, null, null, "Lith A1 Relic", 10.00, 0),
            Row(2, "MissionB", 1, null, null, null, "Lith B1 Relic", 15.00, 0),
        };

        var entries = BuildEntries(rows);

        Assert.That(entries, Has.Count.EqualTo(2));
    }

    [Test]
    public void EmptyInput_ReturnsEmptyList()
    {
        var entries = BuildEntries([]);
        Assert.That(entries, Is.Empty);
    }

    // ── Row factory helpers ───────────────────────────────────────────────────

    private static FarmingRow Row(
        long missionId, string missionLabel, int missionOrder,
        long? rotationId, string? rotationLabel, int? rotationOrder,
        string? relicName, double? chance, int? rewardOrder) => new()
    {
        MissionSourceId = missionId,
        MissionDisplayLabel = missionLabel,
        MissionSourceOrder = missionOrder,
        RotationId = rotationId,
        RotationDisplayLabel = rotationLabel,
        RotationSourceOrder = rotationOrder,
        RelicName = relicName,
        RelicChancePercent = chance,
        RewardSourceOrder = rewardOrder
    };

    private static List<FarmingRow> RotationRows(
        long missionId, string missionLabel, int missionOrder,
        (long rotId, string rotLabel, int rotOrder, string? relicName, double? chance)[] rotations)
    {
        return rotations
            .Select(r => Row(missionId, missionLabel, missionOrder,
                r.rotId, r.rotLabel, r.rotOrder,
                r.relicName, r.chance, r.relicName is null ? null : 0))
            .ToList();
    }
}
