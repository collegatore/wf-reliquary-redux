using Moq;
using NUnit.Framework;
using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Presentation;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class FirstStartSetupViewModelTests
{
    private static readonly Uri ConfiguredUrl = new("https://example.test/droptables");

    [Test]
    public async Task LoadEffectiveUrlAsync_DisplaysConfiguredUrl()
    {
        var viewModel = CreateViewModel();

        await viewModel.LoadEffectiveUrlAsync();

        Assert.That(viewModel.SourceUrlText, Is.EqualTo(ConfiguredUrl.ToString()));
    }

    [Test]
    public async Task ApproveCommand_ChainsDownloadThenImport_UsingReturnedId()
    {
        var refreshWorkflow = new Mock<IDropTableSourceRefreshWorkflow>();
        var importWorkflow = new Mock<IDropTableImportWorkflow>();

        refreshWorkflow
            .Setup(w => w.RefreshAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(DropTableRefreshResult.Success(new DropTableSourceStatus(true, DateTimeOffset.UtcNow, "08 April, 2026"), 42L));
        importWorkflow
            .Setup(w => w.ImportAsync(42L, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DropTableImportResult.Success(new ParsedDropTableImport(
                ConfiguredUrl, DateTimeOffset.UtcNow, "08 April, 2026", DateTimeOffset.UtcNow, [], [], [], [], [], [], [])));

        var viewModel = CreateViewModel(refreshWorkflow.Object, importWorkflow.Object);

        await viewModel.ApproveCommand.ExecuteAsync(null);

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.Stage, Is.EqualTo(FirstStartSetupStage.Done));
            Assert.That(viewModel.DownloadState, Is.EqualTo(FirstStartSetupStepState.Succeeded));
            Assert.That(viewModel.ImportState, Is.EqualTo(FirstStartSetupStepState.Succeeded));
        });
        importWorkflow.Verify(w => w.ImportAsync(42L, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ApproveCommand_InvokesReloadCallbacks_OnSuccess()
    {
        // Regression: the automated routine bypasses MainWindowViewModel entirely, so
        // without these callbacks other app pages (Prime Gear, Wishlist views, and the
        // Drop Tables tab's own acquisition/import display) would never refresh after
        // an automated download+import, unlike the manual Download/Import buttons.
        var refreshWorkflow = new Mock<IDropTableSourceRefreshWorkflow>();
        var importWorkflow = new Mock<IDropTableImportWorkflow>();

        refreshWorkflow
            .Setup(w => w.RefreshAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(DropTableRefreshResult.Success(new DropTableSourceStatus(true, DateTimeOffset.UtcNow, "08 April, 2026"), 42L));
        importWorkflow
            .Setup(w => w.ImportAsync(42L, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DropTableImportResult.Success(new ParsedDropTableImport(
                ConfiguredUrl, DateTimeOffset.UtcNow, "08 April, 2026", DateTimeOffset.UtcNow, [], [], [], [], [], [], [])));

        var viewModel = CreateViewModel(refreshWorkflow.Object, importWorkflow.Object);
        var downloadCallbackOrder = -1;
        var importCallbackOrder = -1;
        var callOrder = 0;
        viewModel.OnDownloadSucceeded = () => { downloadCallbackOrder = callOrder++; return Task.CompletedTask; };
        viewModel.OnImportSucceeded = () => { importCallbackOrder = callOrder++; return Task.CompletedTask; };

        await viewModel.ApproveCommand.ExecuteAsync(null);

        Assert.Multiple(() =>
        {
            Assert.That(downloadCallbackOrder, Is.EqualTo(0), "download callback should fire once, before the import callback");
            Assert.That(importCallbackOrder, Is.EqualTo(1), "import callback should fire once, after the download callback");
        });
    }

    [Test]
    public async Task OnImportSucceeded_WiredAsMainWindowDoes_RefreshesHasImportedData()
    {
        // Regression: the automated routine calls the raw IDropTableImportWorkflow
        // directly, bypassing MainWindowViewModel.ImportAsync — the only place that
        // normally refreshes HasImportedData (via RefreshImportStatusAsync). Without
        // also calling dropTables.LoadStatusAsync() from the OnImportSucceeded callback
        // (as MainWindow.axaml.cs now does), HasImportedData stays false forever after
        // an automated import, leaving "Clear imported data" permanently disabled even
        // though the import actually succeeded. This test exercises that exact wiring.
        var refreshWorkflow = new Mock<IDropTableSourceRefreshWorkflow>();
        var importWorkflow = new Mock<IDropTableImportWorkflow>();

        refreshWorkflow
            .Setup(w => w.RefreshAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(DropTableRefreshResult.Success(new DropTableSourceStatus(true, DateTimeOffset.UtcNow, "08 April, 2026"), 42L));
        importWorkflow
            .Setup(w => w.ImportAsync(42L, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DropTableImportResult.Success(new ParsedDropTableImport(
                ConfiguredUrl, DateTimeOffset.UtcNow, "08 April, 2026", DateTimeOffset.UtcNow, [], [], [], [], [], [], [])));

        var importMetadataReader = new Mock<IImportMetadataReader>();
        importMetadataReader
            .Setup(r => r.GetImportedVersionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("08 April, 2026");
        var sourceStore = new Mock<IDropTableSourceStore>();
        sourceStore
            .Setup(s => s.ListSummariesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DropTableSourceSummary> { new(42, ConfiguredUrl.ToString(), DateTimeOffset.UtcNow, "08 April, 2026") });

        var dropTablesRefreshWorkflow = new Mock<IDropTableSourceRefreshWorkflow>();
        dropTablesRefreshWorkflow
            .Setup(w => w.GetStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DropTableSourceStatus(true, DateTimeOffset.UtcNow, "08 April, 2026"));
        dropTablesRefreshWorkflow
            .Setup(w => w.HasStoredSourcesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var dropTables = new MainWindowViewModel(
            dropTablesRefreshWorkflow.Object,
            Mock.Of<IDropTableImportWorkflow>(),
            importMetadataReader.Object,
            Mock.Of<IDropTableLocalFileSourceWorkflow>(),
            Mock.Of<IDropTableSourceExporter>(),
            Mock.Of<IDropTableImportStore>(),
            sourceStore.Object);

        var setupViewModel = CreateViewModel(refreshWorkflow.Object, importWorkflow.Object);
        setupViewModel.OnImportSucceeded = async () =>
        {
            await dropTables.LoadStatusAsync();
            if (dropTables.OnImportSucceeded is not null)
            {
                await dropTables.OnImportSucceeded();
            }
        };

        await setupViewModel.ApproveCommand.ExecuteAsync(null);

        Assert.Multiple(() =>
        {
            Assert.That(dropTables.HasImportedData, Is.True);
            Assert.That(dropTables.ClearImportCommand.CanExecute(null), Is.True);
        });
    }

    [Test]
    public async Task ApproveCommand_DoesNotInvokeImportCallback_WhenDownloadFails()
    {
        var refreshWorkflow = new Mock<IDropTableSourceRefreshWorkflow>();
        var importWorkflow = new Mock<IDropTableImportWorkflow>();

        refreshWorkflow
            .Setup(w => w.RefreshAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(DropTableRefreshResult.Failure(DropTableSourceStatus.Missing(), "network unavailable"));

        var viewModel = CreateViewModel(refreshWorkflow.Object, importWorkflow.Object);
        var downloadCallbackInvoked = false;
        var importCallbackInvoked = false;
        viewModel.OnDownloadSucceeded = () => { downloadCallbackInvoked = true; return Task.CompletedTask; };
        viewModel.OnImportSucceeded = () => { importCallbackInvoked = true; return Task.CompletedTask; };

        await viewModel.ApproveCommand.ExecuteAsync(null);

        Assert.Multiple(() =>
        {
            Assert.That(downloadCallbackInvoked, Is.False);
            Assert.That(importCallbackInvoked, Is.False);
        });
    }

    [Test]
    public async Task ApproveCommand_DoesNotInvokeImportCallback_WhenImportFails()
    {
        var refreshWorkflow = new Mock<IDropTableSourceRefreshWorkflow>();
        var importWorkflow = new Mock<IDropTableImportWorkflow>();

        refreshWorkflow
            .Setup(w => w.RefreshAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(DropTableRefreshResult.Success(new DropTableSourceStatus(true, DateTimeOffset.UtcNow, "08 April, 2026"), 7L));
        importWorkflow
            .Setup(w => w.ImportAsync(7L, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DropTableImportResult.Failure("parse error"));

        var viewModel = CreateViewModel(refreshWorkflow.Object, importWorkflow.Object);
        var downloadCallbackInvoked = false;
        var importCallbackInvoked = false;
        viewModel.OnDownloadSucceeded = () => { downloadCallbackInvoked = true; return Task.CompletedTask; };
        viewModel.OnImportSucceeded = () => { importCallbackInvoked = true; return Task.CompletedTask; };

        await viewModel.ApproveCommand.ExecuteAsync(null);

        Assert.Multiple(() =>
        {
            Assert.That(downloadCallbackInvoked, Is.True, "download succeeded, so its callback should still fire");
            Assert.That(importCallbackInvoked, Is.False, "import failed, so its callback should not fire");
        });
    }

    [Test]
    public async Task DenyCommand_PerformsNoWorkflowCalls()
    {
        var refreshWorkflow = new Mock<IDropTableSourceRefreshWorkflow>();
        var importWorkflow = new Mock<IDropTableImportWorkflow>();
        var viewModel = CreateViewModel(refreshWorkflow.Object, importWorkflow.Object);
        var closed = false;
        viewModel.RequestClose = () => closed = true;

        viewModel.DenyCommand.Execute(null);

        Assert.That(closed, Is.True);
        refreshWorkflow.Verify(w => w.RefreshAsync(It.IsAny<CancellationToken>()), Times.Never);
        importWorkflow.Verify(w => w.ImportAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ApproveCommand_WhenDownloadFails_SkipsImportAndReportsError()
    {
        var refreshWorkflow = new Mock<IDropTableSourceRefreshWorkflow>();
        var importWorkflow = new Mock<IDropTableImportWorkflow>();
        var reporter = new Mock<IErrorReporter>();

        refreshWorkflow
            .Setup(w => w.RefreshAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(DropTableRefreshResult.Failure(DropTableSourceStatus.Missing(), "network unavailable"));

        var viewModel = CreateViewModel(refreshWorkflow.Object, importWorkflow.Object, reporter.Object);

        await viewModel.ApproveCommand.ExecuteAsync(null);

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.Stage, Is.EqualTo(FirstStartSetupStage.Done));
            Assert.That(viewModel.DownloadState, Is.EqualTo(FirstStartSetupStepState.Failed));
            Assert.That(viewModel.ImportState, Is.EqualTo(FirstStartSetupStepState.Skipped));
        });
        importWorkflow.Verify(w => w.ImportAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
        reporter.Verify(r => r.Report(It.Is<string>(s => s.Contains("Download failed")), It.IsAny<Exception>()), Times.Once);
    }

    [Test]
    public async Task ApproveCommand_WhenImportFailsAfterSuccessfulDownload_ReportsErrorAndLeavesSourceSaved()
    {
        var refreshWorkflow = new Mock<IDropTableSourceRefreshWorkflow>();
        var importWorkflow = new Mock<IDropTableImportWorkflow>();
        var reporter = new Mock<IErrorReporter>();

        refreshWorkflow
            .Setup(w => w.RefreshAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(DropTableRefreshResult.Success(new DropTableSourceStatus(true, DateTimeOffset.UtcNow, "08 April, 2026"), 7L));
        importWorkflow
            .Setup(w => w.ImportAsync(7L, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DropTableImportResult.Failure("parse error"));

        var viewModel = CreateViewModel(refreshWorkflow.Object, importWorkflow.Object, reporter.Object);

        await viewModel.ApproveCommand.ExecuteAsync(null);

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.Stage, Is.EqualTo(FirstStartSetupStage.Done));
            Assert.That(viewModel.DownloadState, Is.EqualTo(FirstStartSetupStepState.Succeeded));
            Assert.That(viewModel.ImportState, Is.EqualTo(FirstStartSetupStepState.Failed));
        });
        reporter.Verify(r => r.Report(It.Is<string>(s => s.Contains("Import failed")), It.IsAny<Exception>()), Times.Once);
    }

    [Test]
    public void ExecutionPhaseHeader_ShowsInProgress_BeforeApproval()
    {
        var viewModel = CreateViewModel();

        Assert.That(viewModel.ExecutionPhaseHeader, Is.EqualTo("Performing initial setup"));
    }

    [Test]
    public async Task ExecutionPhaseHeader_ShowsSuccess_WhenBothStepsSucceed()
    {
        var refreshWorkflow = new Mock<IDropTableSourceRefreshWorkflow>();
        var importWorkflow = new Mock<IDropTableImportWorkflow>();

        refreshWorkflow
            .Setup(w => w.RefreshAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(DropTableRefreshResult.Success(new DropTableSourceStatus(true, DateTimeOffset.UtcNow, "08 April, 2026"), 42L));
        importWorkflow
            .Setup(w => w.ImportAsync(42L, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DropTableImportResult.Success(new ParsedDropTableImport(
                ConfiguredUrl, DateTimeOffset.UtcNow, "08 April, 2026", DateTimeOffset.UtcNow, [], [], [], [], [], [], [])));

        var viewModel = CreateViewModel(refreshWorkflow.Object, importWorkflow.Object);

        await viewModel.ApproveCommand.ExecuteAsync(null);

        Assert.That(viewModel.ExecutionPhaseHeader, Is.EqualTo("Initial setup successfully completed"));
    }

    [Test]
    public async Task ExecutionPhaseHeader_ShowsFailure_WhenDownloadFailsAndImportIsSkipped()
    {
        // Regression: the original implementation checked that both DownloadState and
        // ImportState were individually "Succeeded or Failed" before considering the run
        // settled. It never accounted for Skipped (Import's state when Download fails),
        // so this exact scenario got stuck showing "Performing initial setup" forever
        // even though the run had already reached the Done stage.
        var refreshWorkflow = new Mock<IDropTableSourceRefreshWorkflow>();
        var importWorkflow = new Mock<IDropTableImportWorkflow>();

        refreshWorkflow
            .Setup(w => w.RefreshAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(DropTableRefreshResult.Failure(DropTableSourceStatus.Missing(), "network unavailable"));

        var viewModel = CreateViewModel(refreshWorkflow.Object, importWorkflow.Object);

        await viewModel.ApproveCommand.ExecuteAsync(null);

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.ImportState, Is.EqualTo(FirstStartSetupStepState.Skipped));
            Assert.That(viewModel.ExecutionPhaseHeader, Does.StartWith("Initial setup failed"));
        });
    }

    [Test]
    public async Task ExecutionPhaseHeader_ShowsFailure_WhenImportFailsAfterSuccessfulDownload()
    {
        var refreshWorkflow = new Mock<IDropTableSourceRefreshWorkflow>();
        var importWorkflow = new Mock<IDropTableImportWorkflow>();

        refreshWorkflow
            .Setup(w => w.RefreshAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(DropTableRefreshResult.Success(new DropTableSourceStatus(true, DateTimeOffset.UtcNow, "08 April, 2026"), 7L));
        importWorkflow
            .Setup(w => w.ImportAsync(7L, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DropTableImportResult.Failure("parse error"));

        var viewModel = CreateViewModel(refreshWorkflow.Object, importWorkflow.Object);

        await viewModel.ApproveCommand.ExecuteAsync(null);

        Assert.That(viewModel.ExecutionPhaseHeader, Does.StartWith("Initial setup failed"));
    }

    [Test]
    public async Task ExecutionPhaseHeader_RaisesPropertyChanged_AsStageAndStepsProgress()
    {
        // Regression: the property was bound in the AXAML but nothing raised
        // PropertyChanged for it, so the header text would compute correctly but never
        // actually refresh on screen as the run progressed.
        var refreshWorkflow = new Mock<IDropTableSourceRefreshWorkflow>();
        var importWorkflow = new Mock<IDropTableImportWorkflow>();

        refreshWorkflow
            .Setup(w => w.RefreshAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(DropTableRefreshResult.Success(new DropTableSourceStatus(true, DateTimeOffset.UtcNow, "08 April, 2026"), 42L));
        importWorkflow
            .Setup(w => w.ImportAsync(42L, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DropTableImportResult.Success(new ParsedDropTableImport(
                ConfiguredUrl, DateTimeOffset.UtcNow, "08 April, 2026", DateTimeOffset.UtcNow, [], [], [], [], [], [], [])));

        var viewModel = CreateViewModel(refreshWorkflow.Object, importWorkflow.Object);
        var raisedCount = 0;
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(FirstStartSetupViewModel.ExecutionPhaseHeader))
            {
                raisedCount++;
            }
        };

        await viewModel.ApproveCommand.ExecuteAsync(null);

        Assert.That(raisedCount, Is.GreaterThan(0));
    }

    [Test]
    public void CloseCommand_IsDisabled_UntilBothStepsAreTerminal()
    {
        var viewModel = CreateViewModel();

        Assert.That(viewModel.CloseCommand.CanExecute(null), Is.False);
    }

    [Test]
    public async Task CloseCommand_BecomesEnabled_OnceBothStepsSettle()
    {
        var refreshWorkflow = new Mock<IDropTableSourceRefreshWorkflow>();
        var importWorkflow = new Mock<IDropTableImportWorkflow>();

        refreshWorkflow
            .Setup(w => w.RefreshAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(DropTableRefreshResult.Failure(DropTableSourceStatus.Missing(), "boom"));

        var viewModel = CreateViewModel(refreshWorkflow.Object, importWorkflow.Object);

        await viewModel.ApproveCommand.ExecuteAsync(null);

        Assert.That(viewModel.CloseCommand.CanExecute(null), Is.True);
    }

    private static FirstStartSetupViewModel CreateViewModel(
        IDropTableSourceRefreshWorkflow? refreshWorkflow = null,
        IDropTableImportWorkflow? importWorkflow = null,
        IErrorReporter? errorReporter = null)
    {
        var settingsStore = new Mock<IDropTableSourceSettingsStore>();
        settingsStore
            .Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DropTableSourceSettings(ConfiguredUrl));

        return new FirstStartSetupViewModel(
            refreshWorkflow ?? Mock.Of<IDropTableSourceRefreshWorkflow>(),
            importWorkflow ?? Mock.Of<IDropTableImportWorkflow>(),
            settingsStore.Object,
            errorReporter);
    }
}
