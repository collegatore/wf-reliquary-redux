using Moq;
using NUnit.Framework;
using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Application.Ports;

namespace WF.Reliquary.UnitTests;

[TestFixture]
public sealed class DropTableLocalFileSourceWorkflowTests
{
    [Test]
    public async Task RegisterFromFileAsync_WhenDuplicateVersionExists_ReturnsDuplicateVersionWithoutSaving()
    {
        var store = new Mock<IDropTableSourceStore>();
        var metadataReader = new Mock<IDropTableMetadataReader>();
        var filePath = CreateTempHtmlFile("<b>Last Update:</b> 08 April, 2026");

        metadataReader
            .Setup(r => r.ReadLastUpdate(It.IsAny<string>()))
            .Returns("08 April, 2026");
        store
            .Setup(s => s.FindSummaryByLastUpdateAsync("08 April, 2026", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DropTableSourceSummary(42, "file:///old.html", DateTimeOffset.UnixEpoch, "08 April, 2026"));

        var workflow = new DropTableLocalFileSourceWorkflow(store.Object, metadataReader.Object);
        var result = await workflow.RegisterFromFileAsync(filePath);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsDuplicateVersion, Is.True);
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.ErrorMessage, Is.Null);
            Assert.That(result.Pending, Is.Not.Null);
            Assert.That(result.ExistingId, Is.EqualTo(42));
        });
        store.Verify(s => s.SaveAsync(It.IsAny<DropTableSourceContent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task RegisterFromFileAsync_WhenNewVersion_SavesAndReturnsSuccess()
    {
        var store = new Mock<IDropTableSourceStore>();
        var metadataReader = new Mock<IDropTableMetadataReader>();
        var filePath = CreateTempHtmlFile("<b>Last Update:</b> 08 April, 2026");

        metadataReader
            .Setup(r => r.ReadLastUpdate(It.IsAny<string>()))
            .Returns("08 April, 2026");
        store
            .Setup(s => s.FindSummaryByLastUpdateAsync("08 April, 2026", It.IsAny<CancellationToken>()))
            .ReturnsAsync((DropTableSourceSummary?)null);

        var workflow = new DropTableLocalFileSourceWorkflow(store.Object, metadataReader.Object);
        var result = await workflow.RegisterFromFileAsync(filePath);

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.LastUpdate, Is.EqualTo("08 April, 2026"));
            Assert.That(result.IsDuplicateVersion, Is.False);
        });
        store.Verify(s => s.SaveAsync(It.IsAny<DropTableSourceContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ConfirmSaveAsync_ReplacesAndReturnsSuccess()
    {
        var store = new Mock<IDropTableSourceStore>();
        var pending = new DropTableSourceContent(new Uri("file:///test.html"), "<html></html>", DateTimeOffset.UtcNow, "08 April, 2026");

        var workflow = new DropTableLocalFileSourceWorkflow(store.Object, Mock.Of<IDropTableMetadataReader>());
        var result = await workflow.ConfirmSaveAsync(pending, 42);

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.LastUpdate, Is.EqualTo("08 April, 2026"));
        });
        store.Verify(s => s.ReplaceAsync(42, pending, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ConfirmSaveAsync_WhenStoreThrows_ReturnsFailure()
    {
        var store = new Mock<IDropTableSourceStore>();
        var pending = new DropTableSourceContent(new Uri("file:///test.html"), "<html></html>", DateTimeOffset.UtcNow, "08 April, 2026");
        store
            .Setup(s => s.ReplaceAsync(999, pending, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("not found"));

        var workflow = new DropTableLocalFileSourceWorkflow(store.Object, Mock.Of<IDropTableMetadataReader>());
        var result = await workflow.ConfirmSaveAsync(pending, 999);

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("not found"));
        });
    }

    private static string CreateTempHtmlFile(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"wf-test-{Guid.NewGuid():N}.html");
        File.WriteAllText(path, content);
        return path;
    }
}
