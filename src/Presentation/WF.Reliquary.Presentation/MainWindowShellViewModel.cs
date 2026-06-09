using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.Settings;

namespace WF.Reliquary.Presentation;

public sealed class MainWindowShellViewModel : INotifyPropertyChanged
{
    // Index of the Settings tab within the TabControl (Drop Tables=0, Prime Gear=1,
    // Relics and Missions=2, Settings=3 — hidden from strip).
    private const int SettingsTabIndex = 3;

    private int selectedTabIndex;
    private int previousTabIndex;
    private double? relicsPanelHeight;

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

        dropTables.OnImportSucceeded = async () =>
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

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainWindowViewModel DropTables { get; }
    public PrimeGearViewModel PrimeGear { get; }
    public WishlistViewModel Wishlist { get; }
    public WishlistRelicsViewModel WishlistRelics { get; }
    public WishlistFarmingMissionsViewModel WishlistFarming { get; }
    public ProfileViewModel Profiles { get; }
    public SettingsViewModel Settings { get; }

    public ICommand NavigateToSettingsCommand { get; }

    public int SelectedTabIndex
    {
        get => selectedTabIndex;
        set => SetProperty(ref selectedTabIndex, value);
    }

    /// <summary>Current live relics panel height in pixels (updated by splitter drag).</summary>
    public double? RelicsPanelHeight
    {
        get => relicsPanelHeight;
        set => SetProperty(ref relicsPanelHeight, value);
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

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
