using Moq;
using NUnit.Framework;
using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Application.Ports;

namespace WF.Reliquary.UnitTests;

[TestFixture]
public sealed class DropTableSourceRefreshServiceTests
{
    [Test]
    public async Task GetStatusAsync_ReturnsMissingStatusWhenNoFileExists()
    {
        var store = new Mock<IDropTableSourceStore>();
        var service = CreateService(sourceStore: store.Object);

        var status = await service.GetStatusAsync();

        Assert.Multiple(() =>
        {
            Assert.That(status.FileExists, Is.False);            
            Assert.That(status.LastUpdate, Is.Null);
        });
    }

    [Test]
    public async Task RefreshAsync_StoresDownloadedContentAndReturnsLastUpdate()
    {
        var downloadedAt = new DateTimeOffset(2026, 4, 19, 12, 0, 0, TimeSpan.Zero);
        var configuredSourceUrl = new Uri("https://example.test/custom.html");
        var download = new DropTableDownload(configuredSourceUrl, "<b>Last Update:</b> 08 April, 2026", downloadedAt);
        var stored = new StoredDropTableSource(
            download.Html,
            new DropTableSourceProvenance(configuredSourceUrl, downloadedAt, "08 April, 2026"));
        var client = new Mock<IDropTableSourceClient>();
        var store = new Mock<IDropTableSourceStore>();
        var metadataReader = new Mock<IDropTableMetadataReader>();
        var settingsStore = new Mock<IDropTableSourceSettingsStore>();

        settingsStore
            .Setup(store => store.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DropTableSourceSettings(configuredSourceUrl));
        client
            .Setup(client => client.DownloadAsync(configuredSourceUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(download);
        store
            .Setup(store => store.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);
        metadataReader
            .Setup(reader => reader.ReadLastUpdate(download.Html))
            .Returns("08 April, 2026");

        var service = CreateService(client.Object, store.Object, metadataReader.Object, settingsStore.Object);

        var result = await service.RefreshAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.ErrorMessage, Is.Null);
            Assert.That(result.Status.FileExists, Is.True);
            Assert.That(result.Status.DownloadedAt, Is.EqualTo(downloadedAt));
            Assert.That(result.Status.LastUpdate, Is.EqualTo("08 April, 2026"));
        });
        client.Verify(client => client.DownloadAsync(configuredSourceUrl, It.IsAny<CancellationToken>()), Times.Once);
        store.Verify(
            store => store.SaveAsync(
                It.Is<DropTableSourceContent>(saved => saved.LastUpdate == "08 April, 2026"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task RefreshAsync_WhenLastUpdateCannotBeParsed_FailsWithoutSaving()
    {
        var defaultUrl = new Uri(DropTableSourceDefaults.SourceUrl);
        var download = new DropTableDownload(defaultUrl, "<html>No Last Update here</html>", DateTimeOffset.UnixEpoch);
        var client = new Mock<IDropTableSourceClient>();
        var store = new Mock<IDropTableSourceStore>();
        var metadataReader = new Mock<IDropTableMetadataReader>();

        client
            .Setup(client => client.DownloadAsync(defaultUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(download);
        metadataReader
            .Setup(reader => reader.ReadLastUpdate(download.Html))
            .Returns((string?)null);

        var service = CreateService(client.Object, store.Object, metadataReader.Object);

        var result = await service.RefreshAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("Last Update"));
            Assert.That(result.Status.FileExists, Is.False);
        });
        store.Verify(
            store => store.SaveAsync(It.IsAny<DropTableSourceContent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task RefreshAsync_WhenDownloadFails_KeepsExistingStatus()
    {
        var defaultUrl = new Uri(DropTableSourceDefaults.SourceUrl);
        var stored = new StoredDropTableSource(
            "<b>Last Update:</b> 08 April, 2026",
            new DropTableSourceProvenance(defaultUrl, DateTimeOffset.UnixEpoch, "08 April, 2026"));
        var client = new Mock<IDropTableSourceClient>();
        var store = new Mock<IDropTableSourceStore>();
        var metadataReader = new Mock<IDropTableMetadataReader>();

        client
            .Setup(client => client.DownloadAsync(defaultUrl, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("network unavailable"));
        store
            .Setup(store => store.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);
        var service = CreateService(client.Object, store.Object, metadataReader.Object);

        var result = await service.RefreshAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("network unavailable"));
            Assert.That(result.Status.FileExists, Is.True);
            Assert.That(result.Status.LastUpdate, Is.EqualTo("08 April, 2026"));
        });
        metadataReader.Verify(reader => reader.ReadLastUpdate(stored.Html), Times.Never);
        store.Verify(
            store => store.SaveAsync(It.IsAny<DropTableSourceContent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task SaveSettingsAsync_PersistsUserEditableSourceUrl()
    {
        var settingsStore = new Mock<IDropTableSourceSettingsStore>();
        var customSettings = new DropTableSourceSettings(new Uri("https://example.test/drop-tables.html"));
        settingsStore
            .Setup(store => store.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(customSettings);
        var service = CreateService(settingsStore: settingsStore.Object);

        await service.SaveSettingsAsync(customSettings);

        var loaded = await service.GetSettingsAsync();

        Assert.That(loaded.SourceUrl, Is.EqualTo(customSettings.SourceUrl));
        settingsStore.Verify(store => store.SaveAsync(customSettings, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static DropTableSourceRefreshService CreateService(
        IDropTableSourceClient? sourceClient = null,
        IDropTableSourceStore? sourceStore = null,
        IDropTableMetadataReader? metadataReader = null,
        IDropTableSourceSettingsStore? settingsStore = null)
    {
        return new DropTableSourceRefreshService(
            sourceClient ?? Mock.Of<IDropTableSourceClient>(),
            sourceStore ?? Mock.Of<IDropTableSourceStore>(),
            metadataReader ?? Mock.Of<IDropTableMetadataReader>(),
            settingsStore ?? DefaultSettingsStore(new Uri(DropTableSourceDefaults.SourceUrl)));
    }

    private static IDropTableSourceSettingsStore DefaultSettingsStore(Uri sourceUrl)
    {
        var settingsStore = new Mock<IDropTableSourceSettingsStore>();
        settingsStore
            .Setup(store => store.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DropTableSourceSettings(sourceUrl));

        return settingsStore.Object;
    }
}
