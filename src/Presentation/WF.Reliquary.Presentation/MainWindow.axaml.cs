using Avalonia.Controls;
using WF.Reliquary.Composition;

namespace WF.Reliquary.Presentation;

public sealed partial class MainWindow : Window
{
    /// <summary>
    /// Production constructor. Builds the full composition root and seeds user data.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

        var wishlistStore = ReliquaryServiceFactory.CreateWishlistStore();

        // Ensure "Default" wishlist exists before any VM tries to read wishlists.
        wishlistStore.SeedDefaultAsync().GetAwaiter().GetResult();

        // The prompt delegate captures `this`; it is only invoked after the window
        // is shown, so the reference is valid when the dialog is opened.
        var wishlistVm = new WishlistViewModel(
            wishlistStore,
            prefill => WishlistNameDialog.ShowAsync(this, prefill));

        var primeGearVm = new PrimeGearViewModel(
            ReliquaryServiceFactory.CreatePrimeGearReader(),
            wishlistStore);

        var wishlistRelicsVm = new WishlistRelicsViewModel(
            ReliquaryServiceFactory.CreateWishlistRelicsReader(),
            wishlistStore);

        var wishlistFarmingVm = new WishlistFarmingMissionsViewModel(
            ReliquaryServiceFactory.CreateWishlistFarmingMissionsReader(),
            wishlistStore);

        var shell = new MainWindowShellViewModel(
            new MainWindowViewModel(
                ReliquaryServiceFactory.CreateDropTableSourceRefreshService(),
                ReliquaryServiceFactory.CreateDropTableImportService(),
                ReliquaryServiceFactory.CreateImportMetadataReader()),
            primeGearVm,
            wishlistVm,
            wishlistRelicsVm,
            wishlistFarmingVm);

        DataContext = shell;
        _ = shell.DropTables.LoadStatusAsync();

        wishlistVm.FlyoutCloseRequested = () => WishlistDropdownButton.Flyout?.Hide();
    }

    /// <summary>
    /// Test / design-time constructor allowing an externally constructed view-model.
    /// </summary>
    public MainWindow(MainWindowShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        _ = viewModel.DropTables.LoadStatusAsync();
    }
}
