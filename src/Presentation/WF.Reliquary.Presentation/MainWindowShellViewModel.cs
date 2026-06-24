using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WF.Reliquary.Application.Settings;

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
    private string footerErrorText = string.Empty;

    public MainWindowShellViewModel(
        MainWindowViewModel dropTables,
        PrimeGearViewModel primeGear,
        WishlistViewModel wishlist,
        WishlistRelicsViewModel wishlistRelics,
        WishlistFarmingMissionsViewModel wishlistFarming,
        ProfileViewModel profiles,
        SettingsViewModel settings)
    {
        DropTables = dropTables;
        PrimeGear = primeGear;
        Wishlist = wishlist;
        WishlistRelics = wishlistRelics;
        WishlistFarming = wishlistFarming;
        Profiles = profiles;
        Settings = settings;

        NavigateToSettingsCommand = new RelayCommand(NavigateToSettings);
        NavigateToDropTablesCommand = new RelayCommand(NavigateToDropTables);

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
    }

    public MainWindowViewModel DropTables { get; }
    public PrimeGearViewModel PrimeGear { get; }
    public WishlistViewModel Wishlist { get; }
    public WishlistRelicsViewModel WishlistRelics { get; }
    public WishlistFarmingMissionsViewModel WishlistFarming { get; }
    public ProfileViewModel Profiles { get; }
    public SettingsViewModel Settings { get; }

    public ICommand NavigateToSettingsCommand { get; }
    public ICommand NavigateToDropTablesCommand { get; }

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
    }
}
