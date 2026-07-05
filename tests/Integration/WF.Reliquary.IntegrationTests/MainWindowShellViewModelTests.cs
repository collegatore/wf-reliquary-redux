using Moq;
using NUnit.Framework;
using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.PrimeGear;
using WF.Reliquary.Application.Profiles;
using WF.Reliquary.Application.RecruitingMessage;
using WF.Reliquary.Application.Settings;
using WF.Reliquary.Application.Wishlist;
using WF.Reliquary.Presentation;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class MainWindowShellViewModelTests
{
    // Tab indices mirror the TabControl declaration order in MainWindow.axaml:
    // Prime Gear=0, Relics and Missions=1, Drop Tables=2, Settings=3.
    private const int PrimeGearTabIndex = 0;
    private const int DropTablesTabIndex = 2;
    private const int SettingsTabIndex = 3;

    [Test]
    public void SelectedTabIndex_DefaultsToPrimeGear_OnConstruction()
    {
        var shell = CreateShell();

        Assert.That(shell.SelectedTabIndex, Is.EqualTo(PrimeGearTabIndex));
    }

    [Test]
    public void NavigateToDropTablesCommand_SelectsDropTablesTab()
    {
        var shell = CreateShell();

        shell.NavigateToDropTablesCommand.Execute(null);

        Assert.That(shell.SelectedTabIndex, Is.EqualTo(DropTablesTabIndex));
    }

    [Test]
    public void NavigateToSettingsCommand_SelectsSettingsTab()
    {
        var shell = CreateShell();

        shell.NavigateToSettingsCommand.Execute(null);

        Assert.That(shell.SelectedTabIndex, Is.EqualTo(SettingsTabIndex));
    }

    [Test]
    public async Task InitializeAsync_SelectsDropTablesTab_WhenNoImportDataExists()
    {
        var shell = CreateShell(importedVersion: null);

        await shell.InitializeAsync();

        Assert.That(shell.SelectedTabIndex, Is.EqualTo(DropTablesTabIndex));
    }

    [Test]
    public async Task InitializeAsync_SelectsPrimeGearTab_WhenImportDataExists()
    {
        var shell = CreateShell(importedVersion: "08 April, 2026");

        await shell.InitializeAsync();

        Assert.That(shell.SelectedTabIndex, Is.EqualTo(PrimeGearTabIndex));
    }

    [Test]
    public async Task InitializeAsync_OffersAutomatedSetup_WhenNoSourceExists()
    {
        var shell = CreateShell(importedVersion: null, hasStoredSources: false);
        var offered = false;
        shell.OfferAutomatedSetup = () =>
        {
            offered = true;
            return Task.CompletedTask;
        };

        await shell.InitializeAsync();

        Assert.That(offered, Is.True);
    }

    [Test]
    public async Task InitializeAsync_DoesNotOfferAutomatedSetup_WhenASourceAlreadyExists()
    {
        // A source can exist even when nothing has been imported yet (e.g. a prior
        // manual download or read-from-file without ever importing) — the gate is
        // stricter than the "no imported data" tab-redirect and must not re-offer.
        var shell = CreateShell(importedVersion: null, hasStoredSources: true);
        var offered = false;
        shell.OfferAutomatedSetup = () =>
        {
            offered = true;
            return Task.CompletedTask;
        };

        await shell.InitializeAsync();

        Assert.That(offered, Is.False);
    }

    [Test]
    public async Task InitializeAsync_DoesNotOfferAutomatedSetup_WhenImportedDataExists()
    {
        var shell = CreateShell(importedVersion: "08 April, 2026", hasStoredSources: true);
        var offered = false;
        shell.OfferAutomatedSetup = () =>
        {
            offered = true;
            return Task.CompletedTask;
        };

        await shell.InitializeAsync();

        Assert.That(offered, Is.False);
    }

    [Test]
    public async Task SettingsSave_ClearsRecruitingMessageEditorDirtyState()
    {
        // Regression: the shell wires settings.OnSaved = OnSettingsSaved, which
        // overwrites any OnSaved set elsewhere. That handler must clear the
        // Recruiting Chat message editor's dirty state, otherwise the leave-guard
        // keeps flagging unsaved changes after a successful save.
        var store = new Mock<IUserSettingsStore>();
        store.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(DefaultSettings);
        store.Setup(s => s.SaveAsync(It.IsAny<UserSettings>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var settings = new SettingsViewModel(store.Object, DefaultSettings);
        var recruitingMessageEditor = new RecruitingMessageTemplateEditorViewModel(store.Object, RecruitingMessageTemplate.Default);

        // Replicate MainWindow's RecruitingMessage↔Settings wiring (the dirty-state bridge).
        settings.GetCurrentRecruitingMessageTemplate = () => recruitingMessageEditor.CurrentDraftTemplate();
        settings.IsRecruitingMessageTemplateDirty = () => recruitingMessageEditor.HasUnsavedChanges;
        recruitingMessageEditor.OnDirtyChanged = settings.NotifyRecruitingMessageChanged;

        // The shell wires settings.OnSaved (the path under test).
        _ = CreateShell(settings: settings, recruitingMessageEditor: recruitingMessageEditor);

        // Dirty the editor.
        recruitingMessageEditor.PatternText = "{participation} [{relic_name}]";
        recruitingMessageEditor.ParseCommand.Execute(null);
        Assert.That(recruitingMessageEditor.HasUnsavedChanges, Is.True);
        Assert.That(settings.HasUnsavedChanges, Is.True);

        // Save through the main Settings Save button.
        await settings.SaveAsync();

        Assert.That(recruitingMessageEditor.HasUnsavedChanges, Is.False, "Recruiting Chat message editor dirty state should clear after save");
        Assert.That(settings.HasUnsavedChanges, Is.False, "Settings should report no unsaved changes after save");
    }

    [Test]
    public void RelicCardWidth_DefaultsTo300_OnConstruction()
    {
        var shell = CreateShell();

        Assert.That(shell.RelicCardWidth, Is.EqualTo(300.0));
    }

    [Test]
    public void AppearanceRelicCardWidthChange_PropagatesToShell()
    {
        // The shell wires appearance.ApplyRelicCardWidth so the side-by-side layout's
        // minimum column width stays in sync as the setting changes live.
        var appearance = new AppearanceViewModel(Mock.Of<IUserSettingsStore>(), DefaultSettings);
        var shell = CreateShell(appearance: appearance);

        appearance.RelicCardWidthPx = 450;

        Assert.That(shell.RelicCardWidth, Is.EqualTo(450.0));
    }

    [Test]
    public void AppearanceViewModeChange_PropagatesToWishlistRelics()
    {
        // The shell wires appearance.ApplyViewMode so the Appearance window's relic
        // view mode selector takes effect immediately on the Relics/Missions tab.
        var appearance = new AppearanceViewModel(Mock.Of<IUserSettingsStore>(), DefaultSettings);
        var shell = CreateShell(appearance: appearance);

        appearance.SelectedViewMode = RelicViewMode.Table;

        Assert.That(shell.WishlistRelics.ViewMode, Is.EqualTo(RelicViewMode.Table));
        Assert.That(shell.WishlistRelics.IsViewModeTable, Is.True);
    }

    [Test]
    public async Task InitializeAsync_RunsInitializerBeforeDataLoads()
    {
        var callOrder = new List<string>();
        var initializer = new Mock<IDatabaseInitializer>();
        initializer
            .Setup(i => i.InitializeAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("initializer"))
            .Returns(Task.CompletedTask);

        var importMetadataReader = new Mock<IImportMetadataReader>();
        importMetadataReader
            .Setup(r => r.GetImportedVersionAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("loadStatus"))
            .ReturnsAsync("08 April, 2026");

        var shell = CreateShell(
            importedVersion: "08 April, 2026",
            databaseInitializer: initializer.Object);

        await shell.InitializeAsync();

        Assert.That(callOrder, Has.Count.GreaterThanOrEqualTo(1));
        Assert.That(callOrder[0], Is.EqualTo("initializer"));
        initializer.Verify(i => i.InitializeAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task InitializeAsync_ReportsFailure_WhenInitializerThrows()
    {
        var initializer = new Mock<IDatabaseInitializer>();
        initializer
            .Setup(i => i.InitializeAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Migration failed"));

        var reporter = new Mock<IErrorReporter>();
        var previousReporter = AppErrors.Reporter;
        AppErrors.Reporter = reporter.Object;

        try
        {
            var shell = CreateShell(databaseInitializer: initializer.Object);
            await shell.InitializeAsync();

            reporter.Verify(
                r => r.Report(It.Is<string>(s => s.Contains("Database initialization failed")), It.IsAny<Exception>()),
                Times.Once);
        }
        finally
        {
            AppErrors.Reporter = previousReporter;
        }
    }

    [Test]
    public async Task InitializeAsync_ContinuesStartup_WhenInitializerThrows()
    {
        var initializer = new Mock<IDatabaseInitializer>();
        initializer
            .Setup(i => i.InitializeAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Migration failed"));

        var previousReporter = AppErrors.Reporter;
        AppErrors.Reporter = NoOpErrorReporter.Instance;

        try
        {
            var shell = CreateShell(importedVersion: null, databaseInitializer: initializer.Object);
            await shell.InitializeAsync();

            Assert.That(shell.SelectedTabIndex, Is.EqualTo(DropTablesTabIndex));
        }
        finally
        {
            AppErrors.Reporter = previousReporter;
        }
    }

    private static MainWindowShellViewModel CreateShell(
        string? importedVersion = null,
        bool hasStoredSources = false,
        IDatabaseInitializer? databaseInitializer = null,
        SettingsViewModel? settings = null,
        AppearanceViewModel? appearance = null,
        RecruitingMessageTemplateEditorViewModel? recruitingMessageEditor = null)
    {
        var importMetadataReader = new Mock<IImportMetadataReader>();
        importMetadataReader
            .Setup(r => r.GetImportedVersionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(importedVersion);

        var refreshWorkflow = new Mock<IDropTableSourceRefreshWorkflow>();
        refreshWorkflow
            .Setup(w => w.GetStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(DropTableSourceStatus.Missing());
        refreshWorkflow
            .Setup(w => w.HasStoredSourcesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(hasStoredSources);

        var dropTables = new MainWindowViewModel(
            refreshWorkflow.Object,
            Mock.Of<IDropTableImportWorkflow>(),
            importMetadataReader.Object,
            Mock.Of<IDropTableLocalFileSourceWorkflow>(),
            Mock.Of<IDropTableSourceExporter>(),
            Mock.Of<IDropTableImportStore>(),
            CreateSourceStore(hasStoredSources));

        var primeGearReader = new Mock<IPrimeGearReader>();
        primeGearReader.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var primeGear = new PrimeGearViewModel(primeGearReader.Object, WishlistStore().Object, Mock.Of<IOwnedGearStore>());

        var wishlist = new WishlistViewModel(WishlistStore().Object, _ => Task.FromResult<string?>(null));

        var relics = new WishlistRelicsViewModel(
            Mock.Of<IWishlistRelicsReader>(),
            WishlistStore().Object);

        var farming = new WishlistFarmingMissionsViewModel(
            Mock.Of<IWishlistFarmingMissionsReader>(),
            WishlistStore().Object);

        var profiles = new ProfileViewModel(ProfileStore().Object, _ => Task.FromResult<string?>(null));

        settings ??= new SettingsViewModel(Mock.Of<IUserSettingsStore>(), DefaultSettings);

        appearance ??= new AppearanceViewModel(Mock.Of<IUserSettingsStore>(), DefaultSettings);

        recruitingMessageEditor ??= new RecruitingMessageTemplateEditorViewModel(Mock.Of<IUserSettingsStore>(), RecruitingMessageTemplate.Default);

        databaseInitializer ??= Mock.Of<IDatabaseInitializer>();

        return new MainWindowShellViewModel(
            databaseInitializer, dropTables, primeGear, wishlist, relics, farming, profiles, settings, appearance, recruitingMessageEditor);
    }

    private static UserSettings DefaultSettings { get; } = new(
        SourceUrl: new Uri("https://example.test/drops.html"),
        WindowWidth: 900,
        WindowHeight: 600,
        FontSizePt: 14.0,
        RelicsPanelHeight: null,
        RecruitingMessageTemplate: RecruitingMessageTemplate.Default);

    private static Mock<IWishlistStore> WishlistStore()
    {
        var mock = new Mock<IWishlistStore>();
        mock.Setup(s => s.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WishlistEntry(1, "Default", DateTimeOffset.UnixEpoch));
        mock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new WishlistEntry(1, "Default", DateTimeOffset.UnixEpoch)]);
        mock.Setup(s => s.GetItemsAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        return mock;
    }

    private static Mock<IProfileStore> ProfileStore()
    {
        var mock = new Mock<IProfileStore>();
        mock.Setup(s => s.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProfileEntry(1, "Default", DateTimeOffset.UnixEpoch));
        mock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new ProfileEntry(1, "Default", DateTimeOffset.UnixEpoch)]);
        return mock;
    }

    private static IDropTableSourceStore CreateSourceStore(bool hasStoredSources = false)
    {
        // MainWindowViewModel.LoadStatusAsync derives HasStoredSources from
        // AvailableSources.Count after RefreshSourceListAsync reads ListSummariesAsync
        // (overwriting whatever HasStoredSourcesAsync reported), so the source list is
        // the actual signal that must vary here.
        var summaries = hasStoredSources
            ? new List<DropTableSourceSummary> { new(1, "https://example.test/drops.html", DateTimeOffset.UnixEpoch, "08 April, 2026") }
            : new List<DropTableSourceSummary>();

        var mock = new Mock<IDropTableSourceStore>();
        mock.Setup(s => s.ListSummariesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(summaries);
        return mock.Object;
    }
}
