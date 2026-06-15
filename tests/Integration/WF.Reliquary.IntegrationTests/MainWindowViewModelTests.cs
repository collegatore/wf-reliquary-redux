using Moq;
using NUnit.Framework;
using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Presentation;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class MainWindowViewModelTests
{
    [Test]
    public async Task LoadStatusAsync_ShowsEmptyStateWhenNoFileExists()
    {
        var workflow = CreateWorkflow(DropTableSourceStatus.Missing());
        var viewModel = CreateViewModel(workflow.Object);

        await viewModel.LoadStatusAsync();

        Assert.That(viewModel.StatusText, Is.EqualTo("Source data not aquired."));
    }

    [Test]
    public async Task LoadStatusAsync_ShowsLastUpdateWhenAvailable()
    {
        var viewModel = CreateViewModel(CreateWorkflow(StatusWithLastUpdate("08 April, 2026")).Object);

        await viewModel.LoadStatusAsync();

        Assert.That(viewModel.StatusText, Is.EqualTo("Source data ready. Last Update: 08 April, 2026."));
    }

    [Test]
    public async Task RefreshAsync_ShowsFailureAndKeepsCommandAvailable()
    {
        var status = StatusWithLastUpdate("08 April, 2026");
        var workflow = CreateWorkflow(status);
        workflow
            .Setup(workflow => workflow.RefreshAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(DropTableRefreshResult.Failure(status, "network unavailable"));
        var viewModel = CreateViewModel(workflow.Object);

        await viewModel.RefreshAsync();

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.StatusText, Is.EqualTo("Source data ready. Last Update: 08 April, 2026."));
            Assert.That(viewModel.OperationStatusText, Is.EqualTo("Download failed: network unavailable"));
            Assert.That(viewModel.IsBusy, Is.False);
            Assert.That(viewModel.DownloadCommand.CanExecute(null), Is.True);
        });
    }

    [Test]
    public async Task ImportAsync_InvokesImportWorkflowWithoutRefreshingSource()
    {
        var refreshWorkflow = CreateWorkflow(StatusWithLastUpdate("08 April, 2026"), hasStoredSources: true);
        var importWorkflow = CreateImportWorkflow(DropTableImportResult.Success(SampleParsedImport()));
        var viewModel = CreateViewModel(refreshWorkflow.Object, importWorkflow.Object);
        await viewModel.LoadStatusAsync();

        await viewModel.ImportAsync();

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.OperationStatusText, Does.Contain("Import completed."));
            Assert.That(viewModel.OperationStatusText, Does.Contain("Relics: 1"));
            Assert.That(viewModel.IsBusy, Is.False);
            Assert.That(viewModel.ImportCommand.CanExecute(null), Is.True);
        });
        importWorkflow.Verify(workflow => workflow.ImportAsync(It.IsAny<CancellationToken>()), Times.Once);
        refreshWorkflow.Verify(workflow => workflow.RefreshAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ImportAsync_ShowsFailureAndKeepsCommandAvailable()
    {
        var refreshWorkflow = CreateWorkflow(StatusWithLastUpdate("08 April, 2026"), hasStoredSources: true);
        var importWorkflow = CreateImportWorkflow(DropTableImportResult.Failure("source file missing"));
        var viewModel = CreateViewModel(refreshWorkflow.Object, importWorkflow.Object);
        await viewModel.LoadStatusAsync();

        await viewModel.ImportAsync();

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.OperationStatusText, Is.EqualTo("Import failed: source file missing"));
            Assert.That(viewModel.IsBusy, Is.False);
            Assert.That(viewModel.ImportCommand.CanExecute(null), Is.True);
        });
        refreshWorkflow.Verify(workflow => workflow.RefreshAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static MainWindowViewModel CreateViewModel(
        IDropTableSourceRefreshWorkflow? refreshWorkflow = null,
        IDropTableImportWorkflow? importWorkflow = null,
        IImportMetadataReader? importMetadataReader = null)
    {
        return new MainWindowViewModel(
            refreshWorkflow ?? Mock.Of<IDropTableSourceRefreshWorkflow>(),
            importWorkflow ?? CreateImportWorkflow().Object,
            importMetadataReader ?? CreateImportMetadataReader().Object,
            Mock.Of<IDropTableLocalFileSourceWorkflow>(),
            Mock.Of<IDropTableSourceExporter>());
    }

    private static Mock<IDropTableSourceRefreshWorkflow> CreateWorkflow(DropTableSourceStatus status, bool hasStoredSources = false)
    {
        var workflow = new Mock<IDropTableSourceRefreshWorkflow>();
        workflow
            .Setup(workflow => workflow.GetStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(status);
        workflow
            .Setup(workflow => workflow.GetSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DropTableSourceSettings(new Uri(DropTableSourceDefaults.SourceUrl)));
        workflow
            .Setup(workflow => workflow.RefreshAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(DropTableRefreshResult.Success(status));
        workflow
            .Setup(workflow => workflow.HasStoredSourcesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(hasStoredSources);

        return workflow;
    }

    private static Mock<IDropTableImportWorkflow> CreateImportWorkflow(DropTableImportResult? result = null)
    {
        var workflow = new Mock<IDropTableImportWorkflow>();
        workflow
            .Setup(workflow => workflow.ImportAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(result ?? DropTableImportResult.Failure("not configured"));

        return workflow;
    }

    private static Mock<IImportMetadataReader> CreateImportMetadataReader(string? version = null)
    {
        var reader = new Mock<IImportMetadataReader>();
        reader
            .Setup(r => r.GetImportedVersionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(version);

        return reader;
    }

    private static ParsedDropTableImport SampleParsedImport()
    {
        return new ParsedDropTableImport(
            new Uri(DropTableSourceDefaults.SourceUrl),
            DateTimeOffset.UnixEpoch,
            "08 April, 2026",
            DateTimeOffset.UnixEpoch,
            [new ParsedRewardItem("Lith C14 Relic", "LITH C14 RELIC", "Relic")],
            [new ParsedRelic("Lith", "C14", "Lith C14 Relic", "Lith C14 Relic")],
            [],
            [],
            [],
            [],
            []);
    }

    private static DropTableSourceStatus StatusWithLastUpdate(string lastUpdate)
    {
        return new DropTableSourceStatus(
            true,            
            DateTimeOffset.UnixEpoch,
            lastUpdate);
    }
}
