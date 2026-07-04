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

        Assert.That(viewModel.SourceStatusText, Is.EqualTo("Source data not aquired."));
    }

    [Test]
    public async Task LoadStatusAsync_ShowsLastUpdateWhenAvailable()
    {
        var viewModel = CreateViewModel(CreateWorkflow(StatusWithLastUpdate("08 April, 2026")).Object);

        await viewModel.LoadStatusAsync();

        Assert.That(viewModel.SourceStatusText, Is.EqualTo("Source data ready. Drop Tables version: 08 April, 2026."));
    }

    [Test]
    public async Task RefreshAsync_ShowsFailureAndKeepsCommandAvailable()
    {
        var status = StatusWithLastUpdate("08 April, 2026");
        var workflow = CreateWorkflow(status);
        workflow
            .Setup(workflow => workflow.RefreshAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(DropTableRefreshResult.Failure(status, "network unavailable"));
        var reporter = new FakeErrorReporter();
        var viewModel = CreateViewModel(workflow.Object, errorReporter: reporter);

        await viewModel.RefreshAsync();

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.SourceStatusText, Is.EqualTo("Source data ready. Drop Tables version: 08 April, 2026."));
            Assert.That(reporter.LastMessage, Is.EqualTo("Download failed: network unavailable"));
            Assert.That(viewModel.IsBusy, Is.False);
            Assert.That(viewModel.DownloadCommand.CanExecute(null), Is.True);
        });
    }

    [Test]
    public async Task ImportAsync_InvokesImportWorkflowWithoutRefreshingSource()
    {
        var refreshWorkflow = CreateWorkflow(StatusWithLastUpdate("08 April, 2026"), hasStoredSources: true);
        var importWorkflow = CreateImportWorkflow(DropTableImportResult.Success(SampleParsedImport()));
        var viewModel = CreateViewModel(refreshWorkflow.Object, importWorkflow.Object, sourceStore: CreateSourceStoreWithSummary().Object);
        await viewModel.LoadStatusAsync();

        await viewModel.ImportAsync();

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.OperationStatusText, Does.Contain("Import completed."));
            Assert.That(viewModel.OperationStatusText, Does.Not.Contain("Relics:"));
            Assert.That(viewModel.ImportStatusText, Does.Contain("Relics: 1"));
            Assert.That(viewModel.IsBusy, Is.False);
            Assert.That(viewModel.ImportCommand.CanExecute(null), Is.True);
        });
        importWorkflow.Verify(workflow => workflow.ImportAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Once);
        refreshWorkflow.Verify(workflow => workflow.RefreshAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ImportAsync_ShowsFailureAndKeepsCommandAvailable()
    {
        var refreshWorkflow = CreateWorkflow(StatusWithLastUpdate("08 April, 2026"), hasStoredSources: true);
        var importWorkflow = CreateImportWorkflow(DropTableImportResult.Failure("source file missing"));
        var reporter = new FakeErrorReporter();
        var viewModel = CreateViewModel(refreshWorkflow.Object, importWorkflow.Object, sourceStore: CreateSourceStoreWithSummary().Object, errorReporter: reporter);
        await viewModel.LoadStatusAsync();

        await viewModel.ImportAsync();

        Assert.Multiple(() =>
        {
            Assert.That(reporter.LastMessage, Is.EqualTo("Import failed: source file missing"));
            Assert.That(viewModel.IsBusy, Is.False);
            Assert.That(viewModel.ImportCommand.CanExecute(null), Is.True);
        });
        refreshWorkflow.Verify(workflow => workflow.RefreshAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task RefreshAsync_WhileBusy_DisablesCommand_AndIgnoresReentrantCall()
    {
        var gate = new TaskCompletionSource();
        var status = StatusWithLastUpdate("08 April, 2026");
        var workflow = new Mock<IDropTableSourceRefreshWorkflow>();
        workflow.Setup(w => w.GetStatusAsync(It.IsAny<CancellationToken>())).ReturnsAsync(status);
        var calls = 0;
        workflow
            .Setup(w => w.RefreshAsync(It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                Interlocked.Increment(ref calls);
                await gate.Task;
                return DropTableRefreshResult.Success(status);
            });
        var viewModel = CreateViewModel(workflow.Object);

        var inFlight = viewModel.RefreshAsync();          // enters busy, parks on the gate

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.IsBusy, Is.True);
            Assert.That(viewModel.DownloadCommand.CanExecute(null), Is.False, "command disabled while busy");
        });

        await viewModel.RefreshAsync();                   // reentrant — must be a no-op

        gate.SetResult();
        await inFlight;

        Assert.Multiple(() =>
        {
            Assert.That(calls, Is.EqualTo(1), "the reentrant call did not start a second operation");
            Assert.That(viewModel.IsBusy, Is.False);
            Assert.That(viewModel.DownloadCommand.CanExecute(null), Is.True);
        });
    }

    private static MainWindowViewModel CreateViewModel(
        IDropTableSourceRefreshWorkflow? refreshWorkflow = null,
        IDropTableImportWorkflow? importWorkflow = null,
        IImportMetadataReader? importMetadataReader = null,
        IDropTableImportStore? importStore = null,
        IDropTableSourceStore? sourceStore = null,
        IErrorReporter? errorReporter = null)
    {
        return new MainWindowViewModel(
            refreshWorkflow ?? Mock.Of<IDropTableSourceRefreshWorkflow>(),
            importWorkflow ?? CreateImportWorkflow().Object,
            importMetadataReader ?? CreateImportMetadataReader().Object,
            Mock.Of<IDropTableLocalFileSourceWorkflow>(),
            Mock.Of<IDropTableSourceExporter>(),
            importStore ?? Mock.Of<IDropTableImportStore>(),
            sourceStore ?? CreateEmptySourceStore().Object,
            errorReporter);
    }

    /// <summary>Captures the most recent reported failure for assertions.</summary>
    private sealed class FakeErrorReporter : IErrorReporter
    {
        public string? LastMessage { get; private set; }
        public Exception? LastException { get; private set; }

        public void Report(string userMessage, Exception? exception = null)
        {
            LastMessage = userMessage;
            LastException = exception;
        }
    }

    private static Mock<IDropTableSourceRefreshWorkflow> CreateWorkflow(DropTableSourceStatus status, bool hasStoredSources = false)
    {
        var workflow = new Mock<IDropTableSourceRefreshWorkflow>();
        workflow
            .Setup(workflow => workflow.GetStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(status);
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
            .Setup(workflow => workflow.ImportAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
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

    [Test]
    public async Task ImportCommand_CanExecute_IsFalseWhenSelectedSourceIsNull()
    {
        var refreshWorkflow = CreateWorkflow(DropTableSourceStatus.Missing());
        var viewModel = CreateViewModel(refreshWorkflow.Object);
        await viewModel.LoadStatusAsync();

        Assert.That(viewModel.SelectedSource, Is.Null);
        Assert.That(viewModel.ImportCommand.CanExecute(null), Is.False);
    }

    [Test]
    public async Task ImportCommand_CanExecute_IsTrueWhenSourceSelected()
    {
        var refreshWorkflow = CreateWorkflow(StatusWithLastUpdate("08 April, 2026"), hasStoredSources: true);
        var viewModel = CreateViewModel(refreshWorkflow.Object, sourceStore: CreateSourceStoreWithSummary().Object);
        await viewModel.LoadStatusAsync();

        Assert.That(viewModel.SelectedSource, Is.Not.Null);
        Assert.That(viewModel.ImportCommand.CanExecute(null), Is.True);
    }

    [Test]
    public async Task RefreshAsync_RefreshesSourceListAfterSuccessfulDownload()
    {
        var status = StatusWithLastUpdate("08 April, 2026");
        var refreshWorkflow = CreateWorkflow(status);
        var sourceStore = CreateSourceStoreWithSummary();
        var viewModel = CreateViewModel(refreshWorkflow.Object, sourceStore: sourceStore.Object);

        await viewModel.RefreshAsync();

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.AvailableSources, Has.Count.EqualTo(1));
            Assert.That(viewModel.SelectedSource, Is.Not.Null);
            Assert.That(viewModel.SelectedSource!.LastUpdate, Is.EqualTo("08 April, 2026"));
        });
    }

    [Test]
    public async Task RefreshAsync_DuplicateVersion_CallsConfirmOverwrite()
    {
        var status = StatusWithLastUpdate("08 April, 2026");
        var pending = new DropTableSourceContent(new Uri(DropTableSourceDefaults.SourceUrl), "<html></html>", DateTimeOffset.UtcNow, "08 April, 2026");
        var refreshWorkflow = new Mock<IDropTableSourceRefreshWorkflow>();
        refreshWorkflow.Setup(w => w.GetStatusAsync(It.IsAny<CancellationToken>())).ReturnsAsync(status);
        refreshWorkflow.Setup(w => w.HasStoredSourcesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        refreshWorkflow.Setup(w => w.RefreshAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(DropTableRefreshResult.DuplicateVersion(pending, 1));
        refreshWorkflow.Setup(w => w.ConfirmSaveAsync(pending, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DropTableRefreshResult.Success(status));

        var sourceStore = CreateSourceStoreWithSummary();
        var reporter = new FakeErrorReporter();
        var viewModel = CreateViewModel(refreshWorkflow.Object, sourceStore: sourceStore.Object, errorReporter: reporter);
        await viewModel.LoadStatusAsync();

        var confirmCalled = false;
        viewModel.ConfirmOverwrite = _ => { confirmCalled = true; return Task.FromResult(true); };

        await viewModel.RefreshAsync();

        Assert.Multiple(() =>
        {
            Assert.That(confirmCalled, Is.True);
            Assert.That(reporter.LastMessage, Is.Null, "duplicate detection must not be reported as error");
        });
    }

    [Test]
    public async Task RefreshAsync_DuplicateVersion_CancelDoesNotRefreshList()
    {
        var status = StatusWithLastUpdate("08 April, 2026");
        var pending = new DropTableSourceContent(new Uri(DropTableSourceDefaults.SourceUrl), "<html></html>", DateTimeOffset.UtcNow, "08 April, 2026");
        var refreshWorkflow = new Mock<IDropTableSourceRefreshWorkflow>();
        refreshWorkflow.Setup(w => w.GetStatusAsync(It.IsAny<CancellationToken>())).ReturnsAsync(status);
        refreshWorkflow.Setup(w => w.HasStoredSourcesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        refreshWorkflow.Setup(w => w.RefreshAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(DropTableRefreshResult.DuplicateVersion(pending, 1));

        var sourceStore = CreateSourceStoreWithSummary();
        var viewModel = CreateViewModel(refreshWorkflow.Object, sourceStore: sourceStore.Object);
        await viewModel.LoadStatusAsync();
        var initialCount = sourceStore.Invocations.Count(i => i.Method.Name == "ListSummariesAsync");

        viewModel.ConfirmOverwrite = _ => Task.FromResult(false);

        await viewModel.RefreshAsync();

        var finalCount = sourceStore.Invocations.Count(i => i.Method.Name == "ListSummariesAsync");
        Assert.That(finalCount, Is.EqualTo(initialCount), "source list should not be refreshed when user cancels overwrite");
    }

    private static Mock<IDropTableSourceStore> CreateEmptySourceStore()
    {
        var store = new Mock<IDropTableSourceStore>();
        store
            .Setup(s => s.ListSummariesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DropTableSourceSummary>());
        return store;
    }

    private static Mock<IDropTableSourceStore> CreateSourceStoreWithSummary()
    {
        var store = new Mock<IDropTableSourceStore>();
        store
            .Setup(s => s.ListSummariesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DropTableSourceSummary>
            {
                new(1, DropTableSourceDefaults.SourceUrl, DateTimeOffset.UnixEpoch, "08 April, 2026")
            });
        return store;
    }

    private static DropTableSourceStatus StatusWithLastUpdate(string lastUpdate)
    {
        return new DropTableSourceStatus(
            true,
            DateTimeOffset.UnixEpoch,
            lastUpdate);
    }
}
