namespace WF.Reliquary.Presentation;

public sealed class MainWindowShellViewModel
{
    public MainWindowShellViewModel(
        MainWindowViewModel dropTables,
        PrimeGearViewModel primeGear,
        WishlistViewModel wishlist)
    {
        DropTables = dropTables;
        PrimeGear = primeGear;
        Wishlist = wishlist;

        dropTables.OnImportSucceeded = primeGear.ReloadAsync;
        wishlist.OnActiveWishlistChanged = primeGear.ReloadAsync;
    }

    public MainWindowViewModel DropTables { get; }

    public PrimeGearViewModel PrimeGear { get; }

    public WishlistViewModel Wishlist { get; }
}
