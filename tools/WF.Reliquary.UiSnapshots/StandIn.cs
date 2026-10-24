using System.Windows.Input;
using WF.Reliquary.Presentation;

namespace WF.Reliquary.UiSnapshots;

/// <summary>
/// Stand-in DataContexts for the views. Each is an anonymous object exposing only the
/// property paths the view's bindings read (reflection bindings accept anonymous types),
/// so no services, database, or real view models are needed. When a view gains a new
/// binding, add the property here or the bound control falls back to its default.
/// The only real view models are the selectors' (<see cref="Wishlist"/>), backed by
/// in-memory stores, because their entry types are what the views' typed templates need.
/// The shell view model is never instantiated, so view code-behind that only reacts to
/// <c>MainWindowShellViewModel</c> stays inert.
/// </summary>
internal static class StandIn
{
    /// <summary>
    /// A command that is always executable. Buttons whose Command binding fails are
    /// disabled by Avalonia, which would render them greyed out.
    /// </summary>
    public static readonly ICommand Command = new NoOpCommand(canExecute: true);

    /// <summary>A command that can't execute, so its button renders inactive.</summary>
    public static readonly ICommand DisabledCommand = new NoOpCommand(canExecute: false);

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

    /// <param name="groupMode">Group-operations mode on (one card selected) or off.</param>
    /// <param name="selected">Whether the first card is selected (only meaningful in group mode).</param>
    /// <param name="showOwned">The "Show owned gear" filter; when off, owned cards are hidden.</param>
    /// <param name="singleWishlist">Only one wishlist exists (so Delete is unavailable).</param>
    /// <param name="longWishlistName">A second list's name is wider than the selector field.</param>
    public static object PrimeGearShell(bool groupMode = true, bool selected = true, bool showOwned = true, bool singleWishlist = false, bool longWishlistName = false)
    {
        var cards = GearCards();
        foreach (var card in cards.Where(c => c.IsOwned)) card.IsShown = showOwned;
        if (groupMode && selected) cards[0].IsSelected = true;
        var selectedCount = cards.Count(c => c.IsSelected);
        return new
        {
            PrimeGear = new
            {
                ToggleFilterPanelCommand = Command,
                ToggleGroupModeCommand = Command,
                SelectAllCommand = Command,
                SelectNoneCommand = selectedCount > 0 ? Command : DisabledCommand,
                MarkSelectedOwnedCommand = cards.Any(c => c.IsSelected && !c.IsOwned) ? Command : DisabledCommand,
                UnmarkSelectedCommand = cards.Any(c => c.IsSelected && c.IsOwned) ? Command : DisabledCommand,
                IsFilterPanelOpen = true,
                IsGroupMode = groupMode,
                SelectedCount = selectedCount,
                SelectionStatusText = selectedCount == 0 ? "Click cards to select them" : $"{selectedCount} selected",
                IsBusy = false,
                ShowVaulted = false,
                ShowOwned = showOwned,
                ShowWishlistedOnly = false,
                SearchText = "",
                HasHiddenMatches = false,
                HiddenMatchHint = "",
                HasNoSearchResults = false,
                ActiveCards = cards,
                VaultedCards = new List<PrimeGearCardViewModel>(),
            },
            Wishlist = singleWishlist ? Wishlist("Default")
                : longWishlistName ? Wishlist("Default", "Stupidly long obtrusive and completely unnecessary name")
                : Wishlist(),
        };
    }

    /// <summary>Table mode rows: two available relics and one vaulted one (faded).</summary>
    public static List<WishlistRelicTableRow> TableRows() =>
    [
        new RelicTableRowViewModel("Axi A1", "C, R", isVaulted: false, [], Command, Command),
        new RelicTableRowViewModel("Lith B2", "U", isVaulted: false, [], Command, Command),
        new RelicTableRowViewModel("Meso C3", "2 R", isVaulted: true, [], Command, Command),
    ];

    /// <summary>The Settings tab's profile section; the other sections render empty.</summary>
    public static object SettingsShell() => new
    {
        Profiles = Profiles(),
        Settings = new { SourceUrlText = "", BackCommand = Command, SaveCommand = Command, CopyLogPathCommand = Command, OpenLogFileCommand = Command },
    };

    /// <summary>The real Wishlist selector view model, the first list active (three lists by default).</summary>
    public static WishlistViewModel Wishlist(params string[] names) =>
        new(new StubWishlistStore(names.Length > 0 ? names : ["Default", "Farming", "Trade fodder"]), _ => Task.FromResult<string?>(null));

    /// <summary>The real Profile selector view model over two profiles, "Main" active.</summary>
    public static ProfileViewModel Profiles() =>
        new(new StubProfileStore("Main", "Alt account"), _ => Task.FromResult<string?>(null));

    /// <summary>
    /// Real card view models, so the view's typed card template applies. Their stores
    /// are only touched by card actions, which a static render never invokes.
    /// </summary>
    private static List<PrimeGearCardViewModel> GearCards()
    {
        PrimeGearCardViewModel Card(string gear, bool owned, params (string Part, bool Wished)[] parts) =>
            new(gear,
                parts.Select(p => new PrimePartViewModel(p.Part, p.Part.ToLowerInvariant(), gear, p.Wished, isGhost: false, null!, () => 0)),
                isVaulted: false, isOwned: owned, ownedGearStore: null!, confirmOwned: null, onOwnedChanged: null);

        return
        [
            Card("Ash Prime", false, ("Blueprint", true), ("Chassis", false), ("Neuroptics", true), ("Systems", false)),
            Card("Banshee Prime", false, ("Blueprint", false), ("Chassis", true), ("Neuroptics", false), ("Systems", false)),
            Card("Ember Prime", true, ("Blueprint", false), ("Chassis", false), ("Neuroptics", false), ("Systems", false)),
        ];
    }

    private sealed class NoOpCommand(bool canExecute) : ICommand
    {
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => canExecute;
        public void Execute(object? parameter) { }
    }
}
