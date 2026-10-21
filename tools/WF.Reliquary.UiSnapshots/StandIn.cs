using System.Windows.Input;

namespace WF.Reliquary.UiSnapshots;

/// <summary>
/// Stand-in DataContexts for the views. Each is an anonymous object exposing only the
/// property paths the view's bindings read (reflection bindings accept anonymous types),
/// so no services, database, or real view models are needed. When a view gains a new
/// binding, add the property here or the bound control falls back to its default.
/// The real view models are never instantiated, so view code-behind that only reacts to
/// <c>MainWindowShellViewModel</c> stays inert.
/// </summary>
internal static class StandIn
{
    /// <summary>
    /// A command that is always executable. Buttons whose Command binding fails are
    /// disabled by Avalonia, which would render them greyed out.
    /// </summary>
    public static readonly ICommand Command = new NoOpCommand();

    public static object WishlistRelics(bool table = false) => new
    {
        ToggleFilterPanelCommand = Command,
        ToggleSortPanelCommand = Command,
        ToggleSortDirectionCommand = Command,
        IsFilterPanelOpen = true,
        IsSortPanelOpen = true,
        IsBusy = false,
        ShowVaulted = true,
        ShowAllRewards = false,
        IsViewModeTable = table,
        IsViewModeVerbatim = false,
        IsViewModeCompact = !table,
        IsEmpty = true,
        SortDirectionLabel = "Ascending",
        SortModeIndex = 0,
    };

    public static object WishlistFarming() => new { ShowExactDropChances = false };

    public static object RelicsShell(bool stacked) => new
    {
        WishlistRelics = WishlistRelics(),
        WishlistFarming = WishlistFarming(),
        IsStacked = stacked,
        IsSideBySide = !stacked,
    };

    public static object PrimeGearShell() => new
    {
        PrimeGear = new
        {
            ToggleFilterPanelCommand = Command,
            ToggleGroupModeCommand = Command,
            SelectAllCommand = Command,
            SelectNoneCommand = Command,
            MarkSelectedOwnedCommand = Command,
            UnmarkSelectedCommand = Command,
            IsFilterPanelOpen = true,
            IsGroupMode = true,
            SelectedCount = 3,
            IsBusy = false,
            ShowVaulted = true,
            ShowOwned = false,
            ShowWishlistedOnly = true,
            SearchText = "",
            HasHiddenMatches = false,
            HiddenMatchHint = "",
            HasNoSearchResults = false,
        },
        Wishlist = new { ActiveWishlistName = "Default" },
    };

    private sealed class NoOpCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) { }
    }
}
