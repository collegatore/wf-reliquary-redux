using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using WF.Reliquary.Application.Settings;
using WF.Reliquary.Composition;

namespace WF.Reliquary.Presentation;

public sealed partial class MainWindow : Window
{
    private MainWindowShellViewModel? shell;
    private RotateTransform? _spinnerTransform;
    private DispatcherTimer? _spinnerTimer;

    // Tracks the single open Appearance window, if any (File → Appearance is modeless
    // and single-instance: a second invocation activates this one instead of opening another).
    private AppearanceWindow? appearanceWindow;

    // Tracks the tab index as seen by the Settings-guard logic so we know when
    // we are leaving the Settings tab.  Updated after every guarded transition.
    private int guardedTabIndex;

    // Re-entrancy guard: set while we are programmatically reverting a tab
    // switch so we don't recursively intercept our own correction.
    private bool suppressTabGuard;

    // ── Constructors ────────────────────────────────────────────────────────

    /// <summary>Production constructor.</summary>
    public MainWindow()
    {
        var userSettingsStore = ReliquaryServiceFactory.CreateUserSettingsStore();

        // STARTUP BOUNDARY — the only place (besides OnWindowClosing) where blocking on
        // async work is permitted. The window must be fully initialized synchronously
        // before it is shown, and no message loop is running yet, so there is nothing to
        // starve. Task.Run hops off the UI SynchronizationContext so .GetResult() cannot
        // deadlock it. Do NOT use this pattern in any interactive code path (commands,
        // property setters, reloads) — those must stay fully async.
        var initialSettings = Task.Run(() => userSettingsStore.LoadAsync()).GetAwaiter().GetResult();

        InitializeComponent();

        // Set up the real error reporter now that InitializeComponent has run
        // (ToastPanel is available) and before any VM is constructed (VMs fire
        // SafeFireAndForget reloads whose failures must reach the reporter).
        var errorLog = ReliquaryServiceFactory.CreateErrorLog();
        var fileLauncher = ReliquaryServiceFactory.CreateFileLauncher();
        var errorReporter = new ErrorReporter(
            errorLog,
            fileLauncher,
            setFooterStatus: text => { if (shell is not null) shell.FooterErrorText = text; });
        errorReporter.SetToastPanel(ToastPanel);
        AppErrors.Reporter = errorReporter;

        // Apply saved window size after InitializeComponent (styled properties
        // are fully wired up at this point).
        Width = initialSettings.WindowWidth;
        Height = initialSettings.WindowHeight;

        // Restore saved position only when the target monitor is still connected.
        if (initialSettings.WindowX is { } savedX && initialSettings.WindowY is { } savedY)
        {
            var savedPoint = new PixelPoint(savedX, savedY);
            if (Screens.All.Any(s => s.Bounds.Contains(savedPoint)))
                Position = savedPoint;
        }

        // Apply font size (and relic card width base) to Application resources.
        var initialRelicCardWidth = AppearanceViewModel.ApplyFontSize(initialSettings.FontSizePt, initialSettings.RelicCardWidthPx);

        // Seed the default profile before the default wishlist, so the wishlist
        // seeding has an active profile to attach to.
        var profileStore = ReliquaryServiceFactory.CreateProfileStore();
        profileStore.SeedDefaultAsync().GetAwaiter().GetResult();

        var wishlistStore = ReliquaryServiceFactory.CreateWishlistStore();
        wishlistStore.SeedDefaultAsync().GetAwaiter().GetResult();

        var wishlistVm = new WishlistViewModel(
            wishlistStore,
            request => WishlistNameDialog.ShowAsync(this, request));

        var profileVm = new ProfileViewModel(
            profileStore,
            request => WishlistNameDialog.ShowAsync(this, request, "Profile name"))
        {
            // A freshly created profile gets its own Default wishlist (one-time).
            SeedDefaultWishlist = () => wishlistStore.SeedDefaultAsync(),
        };

        var primeGearVm = new PrimeGearViewModel(
            ReliquaryServiceFactory.CreatePrimeGearReader(),
            wishlistStore,
            ReliquaryServiceFactory.CreateOwnedGearStore());
        primeGearVm.ConfirmOwnedTracking = gearName => OwnedGearTrackingDialog.ShowAsync(this, gearName);
        primeGearVm.ConfirmOwnedTrackingBatch = trackedCount => OwnedGearTrackingDialog.ShowBatchAsync(this, trackedCount);
        primeGearVm.OwnedHideFadeDuration = TimeSpan.FromMilliseconds(180);
        primeGearVm.SearchDebounceDelay = TimeSpan.FromMilliseconds(150);

        var wishlistRelicsVm = new WishlistRelicsViewModel(
            ReliquaryServiceFactory.CreateWishlistRelicsReader(),
            wishlistStore);

        var currentLfgTemplate = initialSettings.LfgTemplate;
        var lfgEditorVm = new LfgTemplateEditorViewModel(userSettingsStore, initialSettings.LfgTemplate);
        lfgEditorVm.OnSaved = t => currentLfgTemplate = t; // SaveCommand path (used by tests)

        wishlistRelicsVm.OpenLfgCompose = async relicName =>
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            await LfgMessageDialog.ShowAsync(
                this,
                currentLfgTemplate,
                relicName,
                async text =>
                {
                    try
                    {
                        if (clipboard is not null)
                        {
                            var data = new DataTransfer();
                            data.Add(DataTransferItem.CreateText(text));
                            await clipboard.SetDataAsync(data);
                        }
                    }
                    catch { }
                });
        };

        wishlistRelicsVm.OpenRewardsModal = card => RewardsModalWindow.ShowAsync(this, card);

        var wishlistFarmingVm = new WishlistFarmingMissionsViewModel(
            ReliquaryServiceFactory.CreateWishlistFarmingMissionsReader(),
            wishlistStore);

        var settingsVm = new SettingsViewModel(userSettingsStore, initialSettings);
        var appearanceVm = new AppearanceViewModel(userSettingsStore, initialSettings);
        settingsVm.ConfirmDiscard = () => ConfirmDiscardDialog.ShowAsync(this);
        settingsVm.CopyLogPath = async () =>
        {
            try
            {
                var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
                if (clipboard is not null)
                {
                    var data = new DataTransfer();
                    data.Add(DataTransferItem.CreateText(errorLog.LogFilePath));
                    await clipboard.SetDataAsync(data);
                }
            }
            catch { }
        };

        settingsVm.GetCurrentLfgTemplate = () => lfgEditorVm.CurrentDraftTemplate();
        settingsVm.IsLfgTemplateDirty = () => lfgEditorVm.HasUnsavedChanges;
        lfgEditorVm.OnDirtyChanged = settingsVm.NotifyLfgChanged;
        // NOTE: settingsVm.OnSaved is wired by the shell (MainWindowShellViewModel),
        // which clears the LFG editor's dirty state via LfgEditor.NotifySaved. The
        // editor's own OnSaved callback (line above) keeps currentLfgTemplate in sync.

        settingsVm.OpenLogFile = () =>
        {
            var logPath = errorLog.LogFilePath;
            if (File.Exists(logPath))
            {
                fileLauncher.Open(logPath);
            }
            else
            {
                // No log file yet (no errors have occurred). Open the log directory
                // so the user can see where logs will appear once errors occur.
                var dir = Path.GetDirectoryName(logPath)!;
                Directory.CreateDirectory(dir);
                fileLauncher.Open(dir);
            }
        };

        var dropTablesVm = new MainWindowViewModel(
            ReliquaryServiceFactory.CreateDropTableSourceRefreshService(userSettingsStore),
            ReliquaryServiceFactory.CreateDropTableImportService(),
            ReliquaryServiceFactory.CreateImportMetadataReader(),
            ReliquaryServiceFactory.CreateDropTableLocalFileSourceWorkflow(),
            ReliquaryServiceFactory.CreateDropTableSourceExporter(),
            ReliquaryServiceFactory.CreateDropTableImportStore(),
            AppErrors.Reporter);

        shell = new MainWindowShellViewModel(
            dropTablesVm,
            primeGearVm,
            wishlistVm,
            wishlistRelicsVm,
            wishlistFarmingVm,
            profileVm,
            settingsVm,
            appearanceVm,
            lfgEditorVm);

        // Sync saved relics panel height and orientation into the shell so the
        // grid initializer can read them when the Relics/Missions tab is first shown.
        shell.RelicsPanelHeight = initialSettings.RelicsPanelHeight;
        shell.Orientation = initialSettings.Orientation;
        shell.SideBySideRelicsWidth = initialSettings.SideBySideRelicsWidth;
        shell.RelicCardWidth = initialRelicCardWidth;
        wishlistRelicsVm.ViewMode = initialSettings.ViewMode;

        DataContext = shell;
        _ = shell.InitializeAsync();

        dropTablesVm.PickSourceFileAsync = async () =>
        {
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select drop-table HTML file",
                AllowMultiple = false,
                FileTypeFilter = [new FilePickerFileType("HTML files") { Patterns = ["*.html"] }]
            });
            return files.Count > 0 ? files[0].TryGetLocalPath() : null;
        };

        dropTablesVm.PickExportFolderAsync = async () =>
        {
            var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select export folder"
            });
            return folders.Count > 0 ? folders[0].TryGetLocalPath() : null;
        };

        dropTablesVm.ConfirmClear = () => ConfirmClearImportDialog.ShowAsync(this);

        Wire();
        InitSpinner();
    }

    /// <summary>Test / design-time constructor.</summary>
    public MainWindow(MainWindowShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        shell = viewModel;
        _ = viewModel.InitializeAsync();
        Wire();
    }

    // ── Wiring ──────────────────────────────────────────────────────────────

    private void InitSpinner()
    {
        _spinnerTransform = new RotateTransform();
        FooterSpinner.RenderTransform = _spinnerTransform;
        _spinnerTimer = new DispatcherTimer(
            TimeSpan.FromMilliseconds(16),
            DispatcherPriority.Normal,
            (_, _) => _spinnerTransform.Angle = (_spinnerTransform.Angle + 7.2) % 360);
        _spinnerTimer.Start();
    }

    private void Wire()
    {
        if (shell is not null)
            shell.OpenAppearance = OpenOrActivateAppearanceWindow;

        // Guard: treat clicking any tab as equivalent to the Back button when
        // on the Settings tab (prompt if unsaved changes, revert font preview).
        MainTabControl.SelectionChanged += OnTabSelectionChanged;

        // Wrap OnNavigateBack so that Back-button-initiated tab transitions
        // bypass the tab guard (NavigateBackAsync changes SelectedTabIndex,
        // which fires SelectionChanged; the guard must not intercept that).
        if (shell is not null)
        {
            var innerBack = shell.Settings.OnNavigateBack;
            shell.Settings.OnNavigateBack = async () =>
            {
                suppressTabGuard = true;
                try { if (innerBack is not null) await innerBack(); }
                finally
                {
                    suppressTabGuard = false;
                    guardedTabIndex = shell.SelectedTabIndex;
                }
            };
        }

        // Save window dimensions on close.
        Closing += OnWindowClosing;
    }

    /// <summary>
    /// Opens the modeless Appearance window, or brings the existing one to the
    /// foreground if it's already open (File → Appearance is single-instance).
    /// </summary>
    private void OpenOrActivateAppearanceWindow()
    {
        if (appearanceWindow is not null)
        {
            appearanceWindow.Activate();
            return;
        }

        appearanceWindow = new AppearanceWindow(shell!.Appearance)
        {
            Position = new PixelPoint(Position.X - 50, Position.Y + 50),
        };
        appearanceWindow.Closed += (_, _) => appearanceWindow = null;
        appearanceWindow.Show(this);
    }

    // ── Event handlers ──────────────────────────────────────────────────────

    private void OnWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        _spinnerTimer?.Stop();
        if (shell is null) return;
        try
        {
            // SHUTDOWN BOUNDARY — like the constructor, blocking on async is permitted here
            // (the window is closing; persisting size/position must complete before exit).
            // Task.Run keeps the block off the UI SynchronizationContext. Best-effort: never
            // block the close on failure.
            var store = ReliquaryServiceFactory.CreateUserSettingsStore();
            var current = Task.Run(() => store.LoadAsync()).GetAwaiter().GetResult();
            var size = ClientSize;
            var pos = Position;
            var relicsHeight = shell?.RelicsPanelHeight;
            var orientation = shell?.Orientation ?? Application.Settings.RelicsMissionsOrientation.Stacked;
            var splitRatio = shell?.SideBySideRelicsWidth;
            Task.Run(() => store.SaveAsync(current with
            {
                WindowWidth = size.Width,
                WindowHeight = size.Height,
                RelicsPanelHeight = relicsHeight,
                WindowX = pos.X,
                WindowY = pos.Y,
                Orientation = orientation,
                SideBySideRelicsWidth = splitRatio,
            })).GetAwaiter().GetResult();
        }
        catch
        {
            // Best-effort; never block the close.
        }
    }

    /// <summary>
    /// Intercepts tab switches while the Settings tab is active so that
    /// clicking another tab behaves identically to pressing Back — including
    /// the unsaved-changes confirmation and font-size revert.
    /// </summary>
    private async void OnTabSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (suppressTabGuard || shell is null) return;

        const int SettingsTabIndex = 3;

        // Not leaving the Settings tab — just record the new index.
        if (guardedTabIndex != SettingsTabIndex)
        {
            guardedTabIndex = shell.SelectedTabIndex;
            return;
        }

        // We were on Settings.  If the destination IS Settings (e.g. File→Settings
        // fired while already there), nothing to guard.
        var destination = shell.SelectedTabIndex;
        if (destination == SettingsTabIndex) return;

        // Revert the visual switch so the user stays on Settings while the
        // async confirmation dialog runs.
        suppressTabGuard = true;
        shell.SelectedTabIndex = SettingsTabIndex;
        suppressTabGuard = false;

        // Check unsaved changes (shows dialog if needed, reverts on discard).
        // Unlike the Back button, we go to the tab the user actually clicked —
        // not the last-visited tab stored in the shell.
        if (!await shell.Settings.TryDiscardAsync())
        {
            guardedTabIndex = SettingsTabIndex;
            return;
        }

        suppressTabGuard = true;
        shell.SelectedTabIndex = destination;
        suppressTabGuard = false;
        guardedTabIndex = destination;
    }

}
