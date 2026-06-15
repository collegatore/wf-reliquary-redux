using NUnit.Framework;
using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Infrastructure.DropTables;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class PrimeGearInfrastructureTests
{
    [Test]
    public void DeriveGearAndPart_ReturnsCorrectGearNameAndPartName()
    {
        var (gearName, partName) = SqlitePrimeGearReader.DeriveGearAndPart("Afuris Prime Receiver");

        Assert.Multiple(() =>
        {
            Assert.That(gearName, Is.EqualTo("Afuris Prime"));
            Assert.That(partName, Is.EqualTo("Receiver"));
        });
    }

    [Test]
    public void DeriveGearAndPart_HandlesBlueprint()
    {
        var (gearName, partName) = SqlitePrimeGearReader.DeriveGearAndPart("Afuris Prime Blueprint");

        Assert.Multiple(() =>
        {
            Assert.That(gearName, Is.EqualTo("Afuris Prime"));
            Assert.That(partName, Is.EqualTo("Blueprint"));
        });
    }

    [Test]
    public async Task GetAllAsync_ReturnsGroupedEntriesFromSeededDatabase()
    {
        var databasePath = TempDatabasePath();
        var store = new SqliteDropTableImportStore(databasePath);
        var import = ImportWithPrimeParts([
            new ParsedRewardItem("Afuris Prime Receiver", "AFURIS PRIME RECEIVER", "PrimePart"),
            new ParsedRewardItem("Afuris Prime Barrel", "AFURIS PRIME BARREL", "PrimePart"),
            new ParsedRewardItem("Afuris Prime Link", "AFURIS PRIME LINK", "PrimePart"),
            new ParsedRewardItem("Afuris Prime Blueprint", "AFURIS PRIME BLUEPRINT", "PrimePart"),
            new ParsedRewardItem("Paris Prime Blueprint", "PARIS PRIME BLUEPRINT", "PrimePart"),
            new ParsedRewardItem("Paris Prime String", "PARIS PRIME STRING", "PrimePart"),
        ]);
        await store.ReplaceAsync(import);

        var reader = new SqlitePrimeGearReader(databasePath);
        var entries = await reader.GetAllAsync();

        Assert.That(entries, Has.Count.EqualTo(2));

        var afuris = entries.Single(e => e.GearName == "Afuris Prime");
        Assert.That(afuris.Parts, Is.EquivalentTo(new[] { "Barrel", "Blueprint", "Link", "Receiver" }));

        var paris = entries.Single(e => e.GearName == "Paris Prime");
        Assert.That(paris.Parts, Is.EquivalentTo(new[] { "Blueprint", "String" }));
    }

    [Test]
    public async Task GetAllAsync_ReturnsEmptyList_WhenNoItemsImported()
    {
        var databasePath = TempDatabasePath();
        var store = new SqliteDropTableImportStore(databasePath);
        await store.ReplaceAsync(ImportWithPrimeParts([]));

        var reader = new SqlitePrimeGearReader(databasePath);
        var entries = await reader.GetAllAsync();

        Assert.That(entries, Is.Empty);
    }

    [Test]
    public async Task GetAllAsync_ReturnsEmptyList_WhenDatabaseDoesNotExist()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), "wf-reliquary-tests", Guid.NewGuid().ToString("N"), "nonexistent.sqlite");

        var reader = new SqlitePrimeGearReader(databasePath);
        var entries = await reader.GetAllAsync();

        Assert.That(entries, Is.Empty);
    }

    private static ParsedDropTableImport ImportWithPrimeParts(IReadOnlyList<ParsedRewardItem> items)
    {
        return new ParsedDropTableImport(
            new Uri(DropTableSourceDefaults.SourceUrl),
            DateTimeOffset.UnixEpoch,
            "08 April, 2026",
            DateTimeOffset.UnixEpoch,
            items,
            [],
            [],
            [],
            [],
            [],
            []);
    }

    private static string TempDatabasePath()
    {
        var directory = Path.Combine(Path.GetTempPath(), "wf-reliquary-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, "import.sqlite");
    }
}
