using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WF.Reliquary.Application.Settings;
using static WF.Reliquary.Application.Settings.RelicsMissionsOrientation;

namespace WF.Reliquary.Presentation;

public sealed class MainWindowShellViewModel : ObservableObject
{
    // Tab indices within the TabControl (declaration order): Prime Gear=0,
    // Relics and Missions=1, Drop Tables=2, Settings=3. Prime Gear is the
    // startup tab (default selectedTabIndex). Drop Tables and Settings are
    // File-menu accessed and hide their header while inactive.
    private const int DropTablesTabIndex = 2;
    private const int SettingsTabIndex = 3;

    private int selectedTabIndex;
    private int previousTabIndex;
    private double? relicsPanelHeight;
    private RelicsMissionsOrientation orientation;
    private double? sideBySideRelicsWidth;
    private double relicCardWidth = 300.0;
    private string footerErrorText = string.Empty;

    public MainWindowShellViewModel(
        MainWindowViewModel dropTables,
        PrimeGearViewModel primeGear,
        WishlistViewModel wishlist,
        WishlistRelicsViewModel wishlistRelics,
        WishlistFarmingMissionsViewModel wishlistFarming,
        ProfileViewModel profiles,
        SettingsViewModel settings,
        AppearanceViewModel appearance,
        RecruitingMessageTemplateEditorViewModel recruitingMessageEditor)
    {
        DropTables = dropTables;
        PrimeGear = primeGear;
        Wishlist = wishlist;
        WishlistRelics = wishlistRelics;
        WishlistFarming = wishlistFarming;
        Profiles = profiles;
        Settings = settings;
        Appearance = appearance;
        RecruitingMessageEditor = recruitingMessageEditor;

        NavigateToSettingsCommand = new RelayCommand(NavigateToSettings);
        NavigateToDropTablesCommand = new RelayCommand(NavigateToDropTables);
        OpenAppearanceCommand = new RelayCommand(() => OpenAppearance?.Invoke());

        dropTables.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainWindowViewModel.HasImportedData))
                OnPropertyChanged(nameof(ShowDropTablesTabHeader));
            if (e.PropertyName == nameof(MainWindowViewModel.IsBusy))
            {
                OnPropertyChanged(nameof(ShowFooterImportStatus));
                OnPropertyChanged(nameof(IsFooterBusy));
            }
            if (e.PropertyName == nameof(MainWindowViewModel.OperationStatusText))
                OnPropertyChanged(nameof(FooterOperationStatusText));
        };

        dropTables.OnImportSucceeded = async () =>
        {
            await primeGear.ReloadAsync();
            await wishlistRelics.ReloadAsync();
            await wishlistFarming.ReloadAsync();
        };

        dropTables.OnImportCleared = async () =>
        {
            await primeGear.ReloadAsync();
            await wishlistRelics.ReloadAsync();
            await wishlistFarming.ReloadAsync();
        };

        wishlist.OnActiveWishlistChanged = async () =>
        {
            await primeGear.ReloadAsync();
            await wishlistRelics.ReloadAsync();
            await wishlistFarming.ReloadAsync();
        };

        // Switching the active profile re-scopes which wishlists exist: reload the
        // selector first, then the wishlist-dependent views.
        profiles.OnActiveProfileChanged = async () =>
        {
            await wishlist.ReloadAsync();
            await primeGear.ReloadAsync();
            await wishlistRelics.ReloadAsync();
            await wishlistFarming.ReloadAsync();
        };

        primeGear.OnWishlistChanged = async () =>
        {
            await wishlistRelics.ReloadAsync();
            await wishlistFarming.ReloadAsync();
        };

        settings.OnNavigateBack = NavigateBackAsync;
        settings.OnSaved = OnSettingsSaved;
        // Apply the layout orientation live so the Appearance window's radio takes
        // effect immediately on the Relics/Missions tab (mirrors font-size behavior).
        appearance.ApplyOrientation = o => Orientation = o;
        // Propagate the effective (font-scaled) relic card width so the side-by-side
        // splitter's minimum width stays correct as either setting changes.
        appearance.ApplyRelicCardWidth = w => RelicCardWidth = w;
        // Apply the relic view mode live so the Appearance window's radio takes
        // effect immediately on the Relics/Missions tab (mirrors orientation).
        appearance.ApplyViewMode = m => wishlistRelics.ViewMode = m;
    }

    public MainWindowViewModel DropTables { get; }
    public PrimeGearViewModel PrimeGear { get; }
    public WishlistViewModel Wishlist { get; }
    public WishlistRelicsViewModel WishlistRelics { get; }
    public WishlistFarmingMissionsViewModel WishlistFarming { get; }
    public ProfileViewModel Profiles { get; }
    public SettingsViewModel Settings { get; }
    public AppearanceViewModel Appearance { get; }
    public RecruitingMessageTemplateEditorViewModel RecruitingMessageEditor { get; }

    public ICommand NavigateToSettingsCommand { get; }
    public ICommand NavigateToDropTablesCommand { get; }
    public ICommand OpenAppearanceCommand { get; }

    /// <summary>
    /// Opens (or activates, if already open) the modeless Appearance window.
    /// Wired by <c>MainWindow</c>, which owns the window instance.
    /// </summary>
    public Action? OpenAppearance { get; set; }

    public int SelectedTabIndex
    {
        get => selectedTabIndex;
        set
        {
            if (SetProperty(ref selectedTabIndex, value))
            {
                OnPropertyChanged(nameof(ShowDropTablesTabHeader));
                OnPropertyChanged(nameof(ShowFooterImportStatus));
            }
        }
    }

    public bool ShowDropTablesTabHeader =>
        !DropTables.HasImportedData || selectedTabIndex == DropTablesTabIndex;

    public bool ShowFooterImportStatus =>
        !DropTables.IsBusy && selectedTabIndex != DropTablesTabIndex;

    public bool IsFooterBusy => DropTables.IsBusy;

    public string FooterOperationStatusText => DropTables.OperationStatusText;

    /// <summary>
    /// Terse error acknowledgment set by <see cref="ErrorReporter"/>. Stays visible
    /// until replaced by the next <c>Report</c> call. Empty when no error has occurred.
    /// </summary>
    public string FooterErrorText
    {
        get => footerErrorText;
        set
        {
            if (SetProperty(ref footerErrorText, value))
                OnPropertyChanged(nameof(HasFooterError));
        }
    }

    public bool HasFooterError => !string.IsNullOrEmpty(footerErrorText);

    /// <summary>Current live relics panel height in pixels (updated by splitter drag).</summary>
    public double? RelicsPanelHeight
    {
        get => relicsPanelHeight;
        set => SetProperty(ref relicsPanelHeight, value);
    }

    /// <summary>Current Relics/Missions tab layout orientation (set from Settings → Appearance).</summary>
    public RelicsMissionsOrientation Orientation
    {
        get => orientation;
        set
        {
            if (SetProperty(ref orientation, value))
            {
                OnPropertyChanged(nameof(IsSideBySide));
                OnPropertyChanged(nameof(IsStacked));
            }
        }
    }

    /// <summary>True when the Relics/Missions tab is in side-by-side layout (drives subtree visibility).</summary>
    public bool IsSideBySide => orientation == SideBySide;

    /// <summary>True when the Relics/Missions tab is in stacked layout (drives subtree visibility).</summary>
    public bool IsStacked => orientation == Stacked;

    /// <summary>Last-dragged relics column pixel width for the side-by-side layout.</summary>
    public double? SideBySideRelicsWidth
    {
        get => sideBySideRelicsWidth;
        set => SetProperty(ref sideBySideRelicsWidth, value);
    }

    /// <summary>
    /// Current effective (font-scaled) relic card width in pixels, mirroring the
    /// live <c>RelicCardWidth</c> Avalonia resource. Used to keep the side-by-side
    /// layout's Relics column minimum width correct.
    /// </summary>
    public double RelicCardWidth
    {
        get => relicCardWidth;
        set => SetProperty(ref relicCardWidth, value);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await DropTables.LoadStatusAsync(cancellationToken);
        if (!DropTables.HasImportedData)
        {
            SelectedTabIndex = DropTablesTabIndex;
        }
    }

    // Drop Tables has no unsaved-changes guard (only Settings does), so this is
    // a plain index switch with no previous-tab/discard handling.
    private void NavigateToDropTables()
    {
        SelectedTabIndex = DropTablesTabIndex;
    }

    private void NavigateToSettings()
    {
        previousTabIndex = selectedTabIndex;
        Settings.SyncRelicsPanelHeight(RelicsPanelHeight);
        SelectedTabIndex = SettingsTabIndex;
    }

    private Task NavigateBackAsync()
    {
        SelectedTabIndex = previousTabIndex;
        return Task.CompletedTask;
    }

    private void OnSettingsSaved(UserSettings saved)
    {
        // If the user saved a relics panel height, sync it back so the splitter
        // reflects the persisted value on next open (e.g. after Reset to default).
        RelicsPanelHeight = saved.RelicsPanelHeight;
        SideBySideRelicsWidth = saved.SideBySideRelicsWidth;

        // The Recruiting Chat message template is persisted through the main Settings Save
        // (no separate Save button), so update the editor's saved baseline and clear its
        // dirty state — otherwise the leave-guard would still flag unsaved changes.
        RecruitingMessageEditor.NotifySaved(saved.RecruitingMessageTemplate);
    }
}
