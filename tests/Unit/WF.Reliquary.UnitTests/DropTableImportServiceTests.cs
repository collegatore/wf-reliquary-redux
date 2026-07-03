using Moq;
using NUnit.Framework;
using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Application.Ports;

namespace WF.Reliquary.UnitTests;

[TestFixture]
public sealed class DropTableImportServiceTests
{
    [Test]
    public async Task ImportAsync_WhenStoredSourceIsMissing_FailsWithoutParsingOrPersisting()
    {
        var sourceStore = new Mock<IDropTableSourceStore>();
        var parser = new Mock<IDropTableParser>();
        var importStore = new Mock<IDropTableImportStore>();
        var service = CreateService(sourceStore: sourceStore.Object, parser: parser.Object, importStore: importStore.Object);

        var result = await service.ImportAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("downloaded before import"));
        });
        parser.Verify(parser => parser.ParseAsync(It.IsAny<DropTableSourceContent>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
        importStore.Verify(store => store.ReplaceAsync(It.IsAny<ParsedDropTableImport>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ImportAsync_WhenParserFails_DoesNotReplacePreviousData()
    {
        var stored = StoredSource();
        var sourceStore = new Mock<IDropTableSourceStore>();
        var parser = new Mock<IDropTableParser>();
        var importStore = new Mock<IDropTableImportStore>();
        sourceStore
            .Setup(store => store.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);
        parser
            .Setup(parser => parser.ParseAsync(stored, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Relics section missing"));
        var service = CreateService(sourceStore: sourceStore.Object, parser: parser.Object, importStore: importStore.Object);

        var result = await service.ImportAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.SourceLastUpdate, Is.EqualTo("08 April, 2026"));
            Assert.That(result.ErrorMessage, Does.Contain("Relics section missing"));
        });
        importStore.Verify(store => store.ReplaceAsync(It.IsAny<ParsedDropTableImport>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ImportAsync_WhenParsingSucceeds_PersistsSnapshotAndReturnsCounts()
    {
        var stored = StoredSource();
        var parsed = SampleImport();
        var sourceStore = new Mock<IDropTableSourceStore>();
        var parser = new Mock<IDropTableParser>();
        var importStore = new Mock<IDropTableImportStore>();
        sourceStore
            .Setup(store => store.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);
        parser
            .Setup(parser => parser.ParseAsync(stored, parsed.ImportedAt, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parsed);
        var service = CreateService(
            sourceStore: sourceStore.Object,
            parser: parser.Object,
            importStore: importStore.Object,
            now: () => parsed.ImportedAt);

        var result = await service.ImportAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Counts.Relics, Is.EqualTo(1));
            Assert.That(result.Counts.MissionRewards, Is.EqualTo(1));
            Assert.That(result.Warnings, Has.Member("non-fatal warning"));
        });
        importStore.Verify(store => store.ReplaceAsync(parsed, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ImportAsync_WhenPersistenceFails_ReturnsFailureWithParserWarnings()
    {
        var stored = StoredSource();
        var parsed = SampleImport();
        var sourceStore = new Mock<IDropTableSourceStore>();
        var parser = new Mock<IDropTableParser>();
        var importStore = new Mock<IDropTableImportStore>();
        sourceStore
            .Setup(store => store.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);
        parser
            .Setup(parser => parser.ParseAsync(stored, parsed.ImportedAt, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parsed);
        importStore
            .Setup(store => store.ReplaceAsync(parsed, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("database locked"));
        var service = CreateService(
            sourceStore: sourceStore.Object,
            parser: parser.Object,
            importStore: importStore.Object,
            now: () => parsed.ImportedAt);

        var result = await service.ImportAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("database locked"));
            Assert.That(result.Warnings, Has.Member("non-fatal warning"));
        });
    }

    private static DropTableImportService CreateService(
        IDropTableSourceStore? sourceStore = null,
        IDropTableParser? parser = null,
        IDropTableImportStore? importStore = null,
        Func<DateTimeOffset>? now = null)
    {
        return new DropTableImportService(
            sourceStore ?? Mock.Of<IDropTableSourceStore>(),
            parser ?? Mock.Of<IDropTableParser>(),
            importStore ?? Mock.Of<IDropTableImportStore>(),
            now);
    }

    private static DropTableSourceContent StoredSource()
    {
        return new DropTableSourceContent(
            new Uri(DropTableSourceDefaults.SourceUrl),
            "<html></html>",
            DateTimeOffset.UnixEpoch,
            "08 April, 2026");
    }

    private static ParsedDropTableImport SampleImport()
    {
        return new ParsedDropTableImport(
            new Uri(DropTableSourceDefaults.SourceUrl),
            DateTimeOffset.UnixEpoch,
            "08 April, 2026",
            new DateTimeOffset(2026, 4, 21, 12, 0, 0, TimeSpan.Zero),
            [
                new ParsedRewardItem("Lith C14 Relic", "LITH C14 RELIC", "Relic"),
                new ParsedRewardItem("Cedo Prime Blueprint", "CEDO PRIME BLUEPRINT", "PrimePart")
            ],
            [new ParsedRelic("Lith", "C14", "Lith C14 Relic", "Lith C14 Relic")],
            [new ParsedRelicReward("Lith C14 Relic", "Cedo Prime Blueprint", "Rare", "Rare", 2.00m, 0)],
            [new ParsedMissionSource("Mercury/Apollodorus (Survival)", "Mercury", "Apollodorus", "Survival", null, 0)],
            [new ParsedRewardRotation("Mercury/Apollodorus (Survival)", 0, "B", "Rotation B", 0)],
            [new ParsedMissionReward("Mercury/Apollodorus (Survival)", 0, "B", "Lith C14 Relic", "Lith C14 Relic", "Rare", 7.69m, 0)],
            ["non-fatal warning"]);
    }
}
