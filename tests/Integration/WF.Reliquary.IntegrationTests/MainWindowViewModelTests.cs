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
        IImportMetadataReader? importMetadataReader = null,
        IDropTableImportStore? importStore = null)
    {
        return new MainWindowViewModel(
            refreshWorkflow ?? Mock.Of<IDropTableSourceRefreshWorkflow>(),
            importWorkflow ?? CreateImportWorkflow().Object,
            importMetadataReader ?? CreateImportMetadataReader().Object,
            Mock.Of<IDropTableLocalFileSourceWorkflow>(),
            Mock.Of<IDropTableSourceExporter>(),
            importStore ?? Mock.Of<IDropTableImportStore>());
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

    [Test]
    public async Task ClearImportCommand_WhenConfirmed_CallsClearAsyncAndUpdatesState()
    {
        var store = new Mock<IDropTableImportStore>();
        var importMetadataReader = new Mock<IImportMetadataReader>();
        importMetadataReader
            .SetupSequence(r => r.GetImportedVersionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("08 April, 2026")
            .ReturnsAsync((string?)null);
        var viewModel = CreateViewModel(
            refreshWorkflow: CreateWorkflow(DropTableSourceStatus.Missing()).Object,
            importMetadataReader: importMetadataReader.Object,
            importStore: store.Object);
        await viewModel.LoadStatusAsync();

        var clearedCallbackInvoked = false;
        viewModel.OnImportCleared = () => { clearedCallbackInvoked = true; return Task.CompletedTask; };
        viewModel.ConfirmClear = () => Task.FromResult(true);

        await viewModel.ClearImportAsync();

        store.Verify(s => s.ClearAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Multiple(() =>
        {
            Assert.That(viewModel.HasImportedData, Is.False);
            Assert.That(viewModel.IsBusy, Is.False);
            Assert.That(clearedCallbackInvoked, Is.True);
        });
    }

    [Test]
    public async Task ClearImportCommand_WhenCancelled_DoesNotCallClearAsync()
    {
        var store = new Mock<IDropTableImportStore>();
        var importMetadataReader = CreateImportMetadataReader("08 April, 2026");
        var viewModel = CreateViewModel(
            refreshWorkflow: CreateWorkflow(DropTableSourceStatus.Missing()).Object,
            importMetadataReader: importMetadataReader.Object,
            importStore: store.Object);
        await viewModel.LoadStatusAsync();

        viewModel.ConfirmClear = () => Task.FromResult(false);

        await viewModel.ClearImportAsync();

        store.Verify(s => s.ClearAsync(It.IsAny<CancellationToken>()), Times.Never);
        Assert.That(viewModel.HasImportedData, Is.True);
    }

    [Test]
    public async Task ClearImportCommand_CanExecute_ReflectsHasImportedDataAndBusy()
    {
        var importMetadataReader = CreateImportMetadataReader(version: null);
        var refreshWorkflow = CreateWorkflow(DropTableSourceStatus.Missing());
        var viewModel = CreateViewModel(
            refreshWorkflow: refreshWorkflow.Object,
            importMetadataReader: importMetadataReader.Object);

        await viewModel.LoadStatusAsync();
        Assert.That(viewModel.ClearImportCommand.CanExecute(null), Is.False, "disabled when no import data");

        importMetadataReader.Setup(r => r.GetImportedVersionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("08 April, 2026");
        await viewModel.LoadStatusAsync();
        Assert.That(viewModel.ClearImportCommand.CanExecute(null), Is.True, "enabled when import data present");
    }

    private static DropTableSourceStatus StatusWithLastUpdate(string lastUpdate)
    {
        return new DropTableSourceStatus(
            true,
            DateTimeOffset.UnixEpoch,
            lastUpdate);
    }
}
