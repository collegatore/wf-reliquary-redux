using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using WF.Reliquary.Application.Settings;
using WF.Reliquary.Composition;

namespace WF.Reliquary.Presentation;

public sealed partial class MainWindow : Window
{
    private MainWindowShellViewModel? shell;
    private RotateTransform? _spinnerTransform;
    private DispatcherTimer? _spinnerTimer;

    // True once the Relics/Missions grid rows have been converted to pixel sizing.
    // This is deferred until the grid is first measured (tab must be visible).
    private bool splitterInitialized;

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

        // Apply font size to Application resources.
        SettingsViewModel.ApplyFontSize(initialSettings.FontSize);

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
        primeGearVm.OwnedHideFadeDuration = TimeSpan.FromMilliseconds(180);

        var wishlistRelicsVm = new WishlistRelicsViewModel(
            ReliquaryServiceFactory.CreateWishlistRelicsReader(),
            wishlistStore);

        var wishlistFarmingVm = new WishlistFarmingMissionsViewModel(
            ReliquaryServiceFactory.CreateWishlistFarmingMissionsReader(),
            wishlistStore);

        var settingsVm = new SettingsViewModel(userSettingsStore, initialSettings);
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
            settingsVm);

        // Sync saved relics panel height into the shell so the grid initializer
        // can read it when the Relics tab is first shown.
        shell.RelicsPanelHeight = initialSettings.RelicsPanelHeight;

        DataContext = shell;
        _ = shell.InitializeAsync();

        wishlistVm.FlyoutCloseRequested = () => WishlistDropdownButton.Flyout?.Hide();

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
        // Grid initialisation: one-shot — unsubscribed as soon as the grid has
        // a measurable height so there is zero SizeChanged overhead after that.
        RelicsMissionsGrid.SizeChanged += OnRelicsGridFirstMeasured;

        // Splitter drag end: capture pixel height and push to shell.
        RelicsSplitter.DragCompleted += OnSplitterDragCompleted;

        // When shell propagates a new saved relics height (from Settings Save
        // or Reset), apply it to the grid immediately.
        if (shell is not null)
        {
            shell.PropertyChanged += OnShellPropertyChanged;
            shell.WishlistRelics.PropertyChanged += OnWishlistRelicsPropertyChanged;
        }

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

    // ── Event handlers ──────────────────────────────────────────────────────

    /// <summary>
    /// One-shot: fires until the grid first has a non-zero height (i.e. the
    /// Relics tab becomes visible and is laid out), converts the star rows to
    /// pixel rows, seeds the initial min-height, then unsubscribes itself so
    /// it never runs again during horizontal resize.
    /// </summary>
    private void OnRelicsGridFirstMeasured(object? sender, SizeChangedEventArgs e)
    {
        var available = AvailableRelicsHeight();
        if (available <= 0) return;   // not yet measured — wait for next event

        // Done — unsubscribe before touching layout to avoid re-entrancy.
        RelicsMissionsGrid.SizeChanged -= OnRelicsGridFirstMeasured;

        var relicsHeight = shell?.RelicsPanelHeight is { } saved && saved > 0
            ? Math.Min(saved, available - 10)
            : available / 3.0; // default 1:2

        SetRelicsRowHeight(relicsHeight);
        splitterInitialized = true;

        UpdateRelicsMinHeight();
    }

    private void OnSplitterDragCompleted(object? sender, VectorEventArgs e)
    {
        if (shell is null) return;

        var height = RelicsMissionsGrid.RowDefinitions[1].ActualHeight;
        shell.RelicsPanelHeight = height > 0 ? height : null;
    }

    private void OnShellPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowShellViewModel.RelicsPanelHeight))
        {
            Dispatcher.UIThread.Post(ApplyRelicsPanelHeight);
        }
    }

    private void OnWishlistRelicsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(WishlistRelicsViewModel.ShowAllRewards))
        {
            // Cards re-render after ShowAllRewards changes; wait for the next
            // layout pass so Bounds.Height values are fresh.
            Dispatcher.UIThread.Post(UpdateRelicsMinHeight, DispatcherPriority.Render);
        }
    }

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
            Task.Run(() => store.SaveAsync(current with
            {
                WindowWidth = size.Width,
                WindowHeight = size.Height,
                RelicsPanelHeight = relicsHeight,
                WindowX = pos.X,
                WindowY = pos.Y,
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

    // ── Grid row helpers ────────────────────────────────────────────────────

    /// <summary>
    /// Applies a (possibly updated) relics panel height from the shell — called
    /// when Settings are saved or the Reset button is pressed.
    /// </summary>
    private void ApplyRelicsPanelHeight()
    {
        if (!splitterInitialized) return;

        var available = AvailableRelicsHeight();
        if (available <= 0) return;

        var relicsHeight = shell?.RelicsPanelHeight is { } h && h > 0
            ? Math.Min(h, available - 10)
            : available / 3.0; // default after reset

        SetRelicsRowHeight(relicsHeight);
    }

    private void SetRelicsRowHeight(double relicsHeight)
    {
        var grid = RelicsMissionsGrid;
        grid.RowDefinitions[1].Height = new GridLength(relicsHeight, GridUnitType.Pixel);
        // Row 3 stays as 1* so the missions panel fills the remaining space.
        grid.RowDefinitions[3].Height = new GridLength(1, GridUnitType.Star);
    }

    private double AvailableRelicsHeight()
    {
        var grid = RelicsMissionsGrid;
        return grid.Bounds.Height
            - grid.RowDefinitions[0].ActualHeight
            - grid.RowDefinitions[2].ActualHeight;
    }

    // ── Min-height enforcement ──────────────────────────────────────────────

    /// <summary>
    /// Sets the minimum allowed height for the relics row to the tallest card
    /// currently visible in the relics panel. Re-evaluated after ShowAllRewards
    /// toggles and on every grid resize.
    /// </summary>
    private void UpdateRelicsMinHeight()
    {
        if (!splitterInitialized) return;

        // Find the WrapPanel that holds the relic cards.
        var wrapPanel = RelicsScrollViewer
            .GetVisualDescendants()
            .OfType<WrapPanel>()
            .FirstOrDefault();

        if (wrapPanel is null) return;

        var maxCardHeight = wrapPanel.Children
            .OfType<Control>()
            .Where(c => c.IsVisible && c.Bounds.Height > 0)
            .Select(c => c.Bounds.Height)
            .DefaultIfEmpty(0)
            .Max();

        if (maxCardHeight > 0)
        {
            // Row 1 is the relics scroller only — the control bar lives in row 0
            // and must not be included in this minimum.
            RelicsMissionsGrid.RowDefinitions[1].MinHeight = maxCardHeight + 8;
        }
    }

}
