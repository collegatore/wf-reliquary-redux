namespace WF.Reliquary.Presentation;

public sealed class MainWindowShellViewModel
{
    public MainWindowShellViewModel(
        MainWindowViewModel dropTables,
        PrimeGearViewModel primeGear,
        WishlistViewModel wishlist,
        WishlistRelicsViewModel wishlistRelics)
    {
        DropTables = dropTables;
        PrimeGear = primeGear;
        Wishlist = wishlist;
        WishlistRelics = wishlistRelics;

        dropTables.OnImportSucceeded = async () =>
        {
            await primeGear.ReloadAsync();
            await wishlistRelics.ReloadAsync();
        };

        wishlist.OnActiveWishlistChanged = async () =>
        {
            await primeGear.ReloadAsync();
            await wishlistRelics.ReloadAsync();
        };
    }

    public MainWindowViewModel DropTables { get; }
    public PrimeGearViewModel PrimeGear { get; }
    public WishlistViewModel Wishlist { get; }
    public WishlistRelicsViewModel WishlistRelics { get; }
}
