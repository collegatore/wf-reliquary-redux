namespace WF.Reliquary.Presentation;

public sealed class MainWindowShellViewModel
{
    public MainWindowShellViewModel(
        MainWindowViewModel dropTables,
        PrimeGearViewModel primeGear,
        WishlistViewModel wishlist,
        WishlistRelicsViewModel wishlistRelics,
        WishlistFarmingMissionsViewModel wishlistFarming)
    {
        DropTables = dropTables;
        PrimeGear = primeGear;
        Wishlist = wishlist;
        WishlistRelics = wishlistRelics;
        WishlistFarming = wishlistFarming;

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

        primeGear.OnWishlistChanged = async () =>
        {
            await wishlistRelics.ReloadAsync();
            await wishlistFarming.ReloadAsync();
        };
    }

    public MainWindowViewModel DropTables { get; }
    public PrimeGearViewModel PrimeGear { get; }
    public WishlistViewModel Wishlist { get; }
    public WishlistRelicsViewModel WishlistRelics { get; }
    public WishlistFarmingMissionsViewModel WishlistFarming { get; }
}
