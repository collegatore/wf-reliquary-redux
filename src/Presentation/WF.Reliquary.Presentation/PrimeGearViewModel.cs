using System.Collections.ObjectModel;
using System.Windows.Input;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WF.Reliquary.Application.Ports;

namespace WF.Reliquary.Presentation;

/// <summary>
/// Outcome of the dialog shown when a user marks gear owned that still has parts
/// tracked in the active wishlist.
/// </summary>
public enum OwnedConfirmChoice
{
    // Cancel is 0 so a dialog dismissed via the window chrome (no button) defaults to Cancel.
    Cancel = 0,
    KeepTracking,
    ReleaseTracking,
}

public sealed class PrimePartViewModel : ObservableObject
{
    private readonly IWishlistStore wishlistStore;
    private readonly Func<long> getActiveWishlistId;
    private readonly Func<Task>? onPersisted;
    private bool isChecked;

    public PrimePartViewModel(
        string partName,
        string normalizedName,
        string gearName,
        bool isChecked,
        bool isGhost,
        IWishlistStore wishlistStore,
        Func<long> getActiveWishlistId,
        Func<Task>? onPersisted = null)
    {
        PartName = partName;
        NormalizedName = normalizedName;
        GearName = gearName;
        this.isChecked = isChecked;
        IsGhost = isGhost;
        this.wishlistStore = wishlistStore;
        this.getActiveWishlistId = getActiveWishlistId;
        this.onPersisted = onPersisted;
    }

    public string PartName { get; }
    public string NormalizedName { get; }
    public string GearName { get; }
    public bool IsGhost { get; }

    public bool IsChecked
    {
        get => isChecked;
        set
        {
            if (isChecked == value) return;
            isChecked = value;
            OnPropertyChanged();
            // Fire-and-forget: failures flow to the global SafeFireAndForget handler.
            PersistCheckedAsync(value).SafeFireAndForget();
        }
    }

    private async Task PersistCheckedAsync(bool checked_)
    {
        var wishlistId = getActiveWishlistId();
        if (checked_)
            await wishlistStore.AddItemAsync(wishlistId, NormalizedName, GearName, PartName);
        else
            await wishlistStore.RemoveItemAsync(wishlistId, NormalizedName);

        if (onPersisted != null)
            await onPersisted();
    }
}

public sealed class PrimeGearCardViewModel : ObservableObject
{
    private readonly IOwnedGearStore ownedGearStore;
    private readonly Func<string, Task<OwnedConfirmChoice>>? confirmOwned;
    private readonly Func<Task>? onOwnedChanged;
    private bool isOwned;
    private bool isFadingOut;
    private bool isShown = true;
    private bool isSelected;

    public PrimeGearCardViewModel(
        string gearName,
        IEnumerable<PrimePartViewModel> parts,
        bool isVaulted,
        bool isOwned,
        IOwnedGearStore ownedGearStore,
        Func<string, Task<OwnedConfirmChoice>>? confirmOwned,
        Func<Task>? onOwnedChanged)
    {
        GearName = gearName;
        Parts = new ObservableCollection<PrimePartViewModel>(parts);
        IsVaulted = isVaulted;
        this.isOwned = isOwned;
        this.ownedGearStore = ownedGearStore;
        this.confirmOwned = confirmOwned;
        this.onOwnedChanged = onOwnedChanged;
        MarkOwnedCommand = new RelayCommand(() => IsOwned = !IsOwned);
        ToggleSelectCommand = new RelayCommand(() => IsSelected = !IsSelected);
    }

    public string GearName { get; }
    public ObservableCollection<PrimePartViewModel> Parts { get; }
    public bool IsVaulted { get; }

    public bool IsShown
    {
        get => isShown;
        set
        {
            if (isShown == value) return;
            isShown = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Toggles owned state — invoked from the card's overflow menu.</summary>
    public ICommand MarkOwnedCommand { get; }

    /// <summary>Toggles selection — invoked from the full-card overlay in group mode.</summary>
    public ICommand ToggleSelectCommand { get; }

    /// <summary>Invoked (by the parent view model) whenever <see cref="IsSelected"/> changes.</summary>
    public Action? SelectionChanged { get; set; }

    /// <summary>
    /// Whether this card is selected for group operations. Independent of <see cref="IsOwned"/>
    /// and of the per-part wishlist check state; only meaningful while group mode is on.
    /// </summary>
    public bool IsSelected
    {
        get => isSelected;
        set
        {
            if (isSelected == value) return;
            isSelected = value;
            OnPropertyChanged();
            SelectionChanged?.Invoke();
        }
    }

    /// <summary>
    /// Marks this card owned for a bulk operation, bypassing the per-card confirmation
    /// dialog (the batch path asks once for the whole selection). When
    /// <paramref name="releaseTracking"/> is true, the card's wished parts are unchecked
    /// (and their wishlist removal persisted) before the owned mark is applied.
    /// </summary>
    internal async Task MarkOwnedForBulkAsync(bool releaseTracking)
    {
        if (releaseTracking)
        {
            foreach (var part in Parts.Where(p => p.IsChecked).ToList())
                part.IsChecked = false;
        }

        if (isOwned) return;
        isOwned = true;
        RaiseOwnedChanged();
        await PersistOwnedAsync(true);
    }

    /// <summary>True when any of this card's parts is tracked in the active wishlist.</summary>
    internal bool HasWishedPartsForBulk => HasWishedParts;

    /// <summary>Overflow-menu label, reflecting the action the entry will perform.</summary>
    public string OwnedMenuLabel => isOwned ? "Unmark as owned" : "Mark as owned";

    /// <summary>True while the card plays its fade-out before being removed from view.</summary>
    public bool IsFadingOut
    {
        get => isFadingOut;
        set
        {
            if (isFadingOut == value) return;
            isFadingOut = value;
            OnPropertyChanged();
        }
    }

    private bool HasWishedParts => Parts.Any(p => p.IsChecked);

    public bool IsOwned
    {
        get => isOwned;
        set
        {
            if (isOwned == value) return;

            // Commit the value (and notify) immediately so the checkbox tracks the
            // click. When parts are still tracked, the choice may revert it below —
            // because the field genuinely went true→false, the bound checkbox unticks.
            isOwned = value;
            RaiseOwnedChanged();

            if (value && HasWishedParts && confirmOwned is not null)
            {
                HandleOwnTransitionAsync().SafeFireAndForget();
                return;
            }

            PersistOwnedAsync(value).SafeFireAndForget();
        }
    }

    private async Task HandleOwnTransitionAsync()
    {
        var choice = await confirmOwned!(GearName);
        switch (choice)
        {
            case OwnedConfirmChoice.ReleaseTracking:
                // Unchecking each wished part persists its removal from the active wishlist.
                foreach (var part in Parts.Where(p => p.IsChecked).ToList())
                    part.IsChecked = false;
                await PersistOwnedAsync(true);
                break;

            case OwnedConfirmChoice.KeepTracking:
                await PersistOwnedAsync(true);
                break;

            case OwnedConfirmChoice.Cancel:
            default:
                // Misclick — revert the owned mark.
                isOwned = false;
                RaiseOwnedChanged();
                break;
        }
    }

    private void RaiseOwnedChanged()
    {
        OnPropertyChanged(nameof(IsOwned));
        OnPropertyChanged(nameof(OwnedMenuLabel));
    }

    private async Task PersistOwnedAsync(bool owned)
    {
        // Start the DB write but let the UI respond (fade + targeted hide/show) without
        // waiting on it — the write overlaps the fade animation, masking its latency.
        var persist = ownedGearStore.SetOwnedAsync(GearName, owned);
        if (onOwnedChanged is not null)
            await onOwnedChanged();
        await persist;
    }
}

public sealed class PrimeGearViewModel : ObservableObject
{
    private readonly IPrimeGearReader reader;
    private readonly IWishlistStore wishlistStore;
    private readonly IOwnedGearStore ownedGearStore;
    private readonly List<PrimeGearCardViewModel> allCards = [];
    private readonly ReloadGate reloadGate = new();
    private CancellationTokenSource searchDebounce = new();
    private long activeWishlistId;
    private bool hideVaulted;
    private bool hideOwned;
    private bool isGroupMode;
    private bool hideNotWishlisted;
    private int selectedCount;
    private bool isBusy;
    private string searchText = string.Empty;
    private int hiddenOwnedMatchCount;
    private int hiddenVaultedMatchCount;
    private string hiddenMatchHint = string.Empty;
    private bool hasHiddenMatches;
    private bool hasVisibleCards;
    private bool hasNoSearchResults;

    public PrimeGearViewModel(IPrimeGearReader reader, IWishlistStore wishlistStore, IOwnedGearStore ownedGearStore)
    {
        this.reader = reader;
        this.wishlistStore = wishlistStore;
        this.ownedGearStore = ownedGearStore;
        ActiveCards = [];
        VaultedCards = [];
        SelectAllCommand = new RelayCommand(SelectAllVisible);
        // Selection-dependent actions are inactive when they have nothing to act on:
        // Select none without a selection, Mark owned without a selected unowned card,
        // Unmark owned without a selected owned card. RecountSelected refreshes them.
        selectNoneCommand = new RelayCommand(ClearSelection, HasSelection);
        markSelectedOwnedCommand = new RelayCommand(() => MarkSelectedOwnedAsync().SafeFireAndForget(), HasUnownedSelected);
        unmarkSelectedCommand = new RelayCommand(UnmarkSelected, HasOwnedSelected);
        ToggleGroupModeCommand = new RelayCommand(() => IsGroupMode = !IsGroupMode);
        // Fire-and-forget initial load: failures flow to the global SafeFireAndForget handler.
        ReloadAsync().SafeFireAndForget();
    }

    public Func<Task>? OnWishlistChanged { get; set; }

    /// <summary>
    /// Set by MainWindow to persist the Prime Gear filters when they change. Called
    /// fire-and-forget after each user change; not on <see cref="LoadFilterSettings"/>.
    /// </summary>
    public Func<Task>? SaveFilterSettingsAsync { get; set; }

    /// <summary>Requests the three-way owned-confirmation dialog. Set by the view.</summary>
    public Func<string, Task<OwnedConfirmChoice>>? ConfirmOwnedTracking { get; set; }

    /// <summary>
    /// Requests the three-way tracking dialog once for a bulk mark, given the number of
    /// selected cards that still track parts. Set by the view.
    /// </summary>
    public Func<int, Task<OwnedConfirmChoice>>? ConfirmOwnedTrackingBatch { get; set; }

    /// <summary>Selects every currently visible card for group operations.</summary>
    public ICommand SelectAllCommand { get; }

    private readonly RelayCommand selectNoneCommand;
    private readonly RelayCommand markSelectedOwnedCommand;
    private readonly RelayCommand unmarkSelectedCommand;

    /// <summary>Clears the group-operations selection. Inactive while nothing is selected.</summary>
    public ICommand SelectNoneCommand => selectNoneCommand;

    /// <summary>
    /// Marks every selected card owned (with a single batched tracking prompt).
    /// Inactive unless at least one selected card is not owned yet.
    /// </summary>
    public ICommand MarkSelectedOwnedCommand => markSelectedOwnedCommand;

    /// <summary>Clears the owned mark on every selected card. Inactive unless a selected card is owned.</summary>
    public ICommand UnmarkSelectedCommand => unmarkSelectedCommand;

    private bool HasSelection() => selectedCount > 0;
    private bool HasUnownedSelected() => allCards.Any(c => c.IsSelected && !c.IsOwned);
    private bool HasOwnedSelected() => allCards.Any(c => c.IsSelected && c.IsOwned);

    /// <summary>Toggles group-operations mode on or off.</summary>
    public ICommand ToggleGroupModeCommand { get; }

    /// <summary>
    /// The Filters menu's "Hide: Not wishlisted" entry: hides cards with no part checked
    /// in the active wishlist. Off by default; persisted.
    /// </summary>
    public bool HideNotWishlisted
    {
        get => hideNotWishlisted;
        set
        {
            if (hideNotWishlisted == value) return;
            hideNotWishlisted = value;
            OnFilterChanged();
        }
    }

    /// <summary>
    /// How long a card fades before it collapses after being marked owned while owned
    /// gear is hidden. Defaults to zero (no fade) so tests update synchronously; the
    /// view sets a short non-zero duration for the visible animation.
    /// </summary>
    public TimeSpan OwnedHideFadeDuration { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// How long to wait after the last SearchText change before applying the filter.
    /// Defaults to zero so tests apply filters synchronously; the view sets ~150ms.
    /// </summary>
    public TimeSpan SearchDebounceDelay { get; set; } = TimeSpan.Zero;

    public ObservableCollection<PrimeGearCardViewModel> ActiveCards { get; }
    public ObservableCollection<PrimeGearCardViewModel> VaultedCards { get; }

    /// <summary>True while this view reloads, for the in-content busy indicator.</summary>
    public bool IsBusy
    {
        get => isBusy;
        private set
        {
            if (isBusy == value) return;
            isBusy = value;
            OnPropertyChanged();
        }
    }

    /// <summary>The Filters menu's "Hide: Vaulted" entry: hides the Vaulted section. Off by default; persisted.</summary>
    public bool HideVaulted
    {
        get => hideVaulted;
        set
        {
            if (hideVaulted == value) return;
            hideVaulted = value;
            OnFilterChanged();
        }
    }

    /// <summary>The Filters menu's "Hide: Owned" entry: hides owned gear in both sections. Off by default; persisted.</summary>
    public bool HideOwned
    {
        get => hideOwned;
        set
        {
            if (hideOwned == value) return;
            hideOwned = value;
            OnFilterChanged();
        }
    }

    /// <summary>True while the Filters menu hides anything; drives the Filters button's marker.</summary>
    public bool IsFilterActive => hideVaulted || hideOwned || hideNotWishlisted;

    /// <summary>Tooltip text for the Filters button, e.g. "Filters: hiding vaulted, owned".</summary>
    public string FilterSummary
    {
        get
        {
            var hidden = new List<string>(3);
            if (hideVaulted) hidden.Add("vaulted");
            if (hideOwned) hidden.Add("owned");
            if (hideNotWishlisted) hidden.Add("not wishlisted");
            return hidden.Count == 0 ? "Filters: nothing hidden" : "Filters: hiding " + string.Join(", ", hidden);
        }
    }

    /// <summary>Restores the persisted Prime Gear filters at startup without saving them.</summary>
    public void LoadFilterSettings(bool hideVaulted, bool hideOwned, bool hideNotWishlisted)
    {
        this.hideVaulted = hideVaulted;
        this.hideOwned = hideOwned;
        this.hideNotWishlisted = hideNotWishlisted;
        NotifyFilterProperties();
        ApplyFilters();
    }

    /// <summary>A user filter change: the visible set changes, so clear the selection, re-filter, and persist.</summary>
    private void OnFilterChanged()
    {
        NotifyFilterProperties();
        ClearSelection();
        ApplyFilters();
        SaveFilterSettingsAsync?.Invoke().SafeFireAndForget();
    }

    private void NotifyFilterProperties()
    {
        OnPropertyChanged(nameof(HideVaulted));
        OnPropertyChanged(nameof(HideOwned));
        OnPropertyChanged(nameof(HideNotWishlisted));
        OnPropertyChanged(nameof(IsFilterActive));
        OnPropertyChanged(nameof(FilterSummary));
    }

    /// <summary>
    /// Whether group-operations mode is active. Ephemeral — it always starts off and is
    /// never persisted. Turning it off clears any current selection.
    /// </summary>
    public bool IsGroupMode
    {
        get => isGroupMode;
        set
        {
            if (isGroupMode == value) return;
            isGroupMode = value;
            OnPropertyChanged();
            if (!value) ClearSelection();
        }
    }

    /// <summary>Number of cards currently selected for group operations.</summary>
    public int SelectedCount
    {
        get => selectedCount;
        private set
        {
            if (selectedCount == value) return;
            var hadSelection = selectedCount > 0;
            selectedCount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectionStatusText));
            if (hadSelection != selectedCount > 0)
                selectNoneCommand.NotifyCanExecuteChanged();
        }
    }

    /// <summary>
    /// The group-operations row's status: a hint on how to select while nothing is
    /// selected, the selection count otherwise. Every path that changes the selection
    /// goes through <see cref="SelectedCount"/>, so the hint returns on its own.
    /// </summary>
    public string SelectionStatusText =>
        selectedCount == 0 ? "Click cards to select them" : $"{selectedCount} selected";

    /// <summary>
    /// Filters cards by a case-insensitive title substring. Active when 3+ characters;
    /// below that threshold the search has no effect. Debounced per <see cref="SearchDebounceDelay"/>.
    /// </summary>
    public string SearchText
    {
        get => searchText;
        set
        {
            if (searchText == value) return;
            searchText = value;
            OnPropertyChanged();
            ClearSelection();
            ScheduleSearchAsync().SafeFireAndForget();
        }
    }

    /// <summary>Number of title matches currently hidden because they are owned and Hide Owned is on.</summary>
    public int HiddenOwnedMatchCount
    {
        get => hiddenOwnedMatchCount;
        private set { if (hiddenOwnedMatchCount == value) return; hiddenOwnedMatchCount = value; OnPropertyChanged(); }
    }

    /// <summary>Number of title matches currently hidden because they are vaulted and Hide Vaulted is on.</summary>
    public int HiddenVaultedMatchCount
    {
        get => hiddenVaultedMatchCount;
        private set { if (hiddenVaultedMatchCount == value) return; hiddenVaultedMatchCount = value; OnPropertyChanged(); }
    }

    /// <summary>Human-readable hint listing hidden match counts, or empty when none are hidden.</summary>
    public string HiddenMatchHint
    {
        get => hiddenMatchHint;
        private set { if (hiddenMatchHint == value) return; hiddenMatchHint = value; OnPropertyChanged(); }
    }

    /// <summary>True while a search is active and at least one title match is hidden by a filter.</summary>
    public bool HasHiddenMatches
    {
        get => hasHiddenMatches;
        private set { if (hasHiddenMatches == value) return; hasHiddenMatches = value; OnPropertyChanged(); }
    }

    /// <summary>True when at least one card is visible under the current filters. False only when search is active and excludes everything.</summary>
    public bool HasVisibleCards
    {
        get => hasVisibleCards;
        private set { if (hasVisibleCards == value) return; hasVisibleCards = value; OnPropertyChanged(); }
    }

    /// <summary>True when a search is active (3+ chars) and no card title matches it under the current filters.</summary>
    public bool HasNoSearchResults
    {
        get => hasNoSearchResults;
        private set { if (hasNoSearchResults == value) return; hasNoSearchResults = value; OnPropertyChanged(); }
    }

    public async Task ReloadAsync()
    {
        // Latest-wins: a newer reload cancels this one's token.
        var token = reloadGate.Begin();
        IsBusy = true;
        try
        {
            await LoadAsync(token);
        }
        catch (OperationCanceledException)
        {
            // Superseded by a newer reload; leave state to the winner.
        }
        finally
        {
            // Only the latest (non-cancelled) reload clears the busy state.
            if (!token.IsCancellationRequested)
                IsBusy = false;
        }
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        var activeWishlist = await wishlistStore.GetActiveAsync(cancellationToken);
        if (activeWishlist is null)
        {
            // No wishlists at all — show nothing (should only happen in tests).
            if (cancellationToken.IsCancellationRequested) return;
            allCards.Clear();
            ActiveCards.Clear();
            VaultedCards.Clear();
            ApplyFilters();
            return;
        }

        activeWishlistId = activeWishlist.Id;

        var wishlistItems = await wishlistStore.GetItemsAsync(activeWishlistId, cancellationToken);
        var importEntries = await reader.GetAllAsync(cancellationToken);
        var ownedGearNames = new HashSet<string>(
            await ownedGearStore.GetOwnedGearNamesAsync(cancellationToken),
            StringComparer.OrdinalIgnoreCase);

        // All awaited fetches are done; if a newer reload superseded us, stop before
        // touching any bound state so we never leave a half-rebuilt view.
        if (cancellationToken.IsCancellationRequested) return;

        // Build lookup: normalizedName → WishlistItem for ghost detection.
        var wishlistLookup = wishlistItems.ToDictionary(
            wi => wi.PartNormalizedName,
            StringComparer.OrdinalIgnoreCase);

        // Build lookup: gearName → (isVaulted, parts[]) from import.
        var importGearLookup = importEntries.ToDictionary(
            e => e.GearName,
            StringComparer.OrdinalIgnoreCase);

        // Build a normalizedName lookup for import parts (to detect which wishlist
        // items are present in the import, and what their normalized name is).
        // reward_items.normalized_name is upper-cased in existing data (see tests),
        // so we store a mapping: normalizedName → (gearName, partName, isVaulted).
        var importPartLookup = new Dictionary<string, (string GearName, string PartName, bool IsVaulted)>(
            StringComparer.OrdinalIgnoreCase);
        foreach (var entry in importEntries)
        {
            foreach (var part in entry.Parts)
            {
                var normalized = (entry.GearName + " " + part).ToUpperInvariant();
                importPartLookup[normalized] = (entry.GearName, part, entry.IsVaulted);
            }
        }

        // Collect all gear names we need to render: import gear ∪ ghost gear.
        var allGearNames = new HashSet<string>(
            importEntries.Select(e => e.GearName),
            StringComparer.OrdinalIgnoreCase);
        foreach (var wi in wishlistItems)
        {
            allGearNames.Add(wi.GearName);
        }

        // Build the full card set into a local list first, then swap in one step.
        var built = new List<PrimeGearCardViewModel>();

        foreach (var gearName in allGearNames.OrderBy(n => n, StringComparer.OrdinalIgnoreCase))
        {
            var isVaulted = importGearLookup.TryGetValue(gearName, out var importEntry) && importEntry.IsVaulted;

            // Import parts for this gear (ordered).
            var importParts = importGearLookup.TryGetValue(gearName, out var ie)
                ? ie.Parts.ToList()
                : [];

            // Ghost parts: wishlisted parts for this gear not in the import.
            var ghostParts = wishlistItems
                .Where(wi => string.Equals(wi.GearName, gearName, StringComparison.OrdinalIgnoreCase)
                             && !importPartLookup.ContainsKey(wi.PartNormalizedName))
                .ToList();

            var partVms = new List<PrimePartViewModel>();

            foreach (var part in importParts.OrderBy(p => p))
            {
                var normalized = (gearName + " " + part).ToUpperInvariant();
                var isChecked_ = wishlistLookup.ContainsKey(normalized);
                partVms.Add(new PrimePartViewModel(
                    partName: part,
                    normalizedName: normalized,
                    gearName: gearName,
                    isChecked: isChecked_,
                    isGhost: false,
                    wishlistStore: wishlistStore,
                    getActiveWishlistId: () => activeWishlistId,
                    onPersisted: () => OnWishlistChanged?.Invoke() ?? Task.CompletedTask));
            }

            foreach (var ghost in ghostParts.OrderBy(wi => wi.PartDisplayName))
            {
                partVms.Add(new PrimePartViewModel(
                    partName: ghost.PartDisplayName,
                    normalizedName: ghost.PartNormalizedName,
                    gearName: gearName,
                    isChecked: true,
                    isGhost: true,
                    wishlistStore: wishlistStore,
                    getActiveWishlistId: () => activeWishlistId,
                    onPersisted: () => OnWishlistChanged?.Invoke() ?? Task.CompletedTask));
            }

            if (partVms.Count == 0) continue;

            PrimeGearCardViewModel card = null!;
            card = new PrimeGearCardViewModel(
                gearName,
                partVms,
                isVaulted,
                isOwned: ownedGearNames.Contains(gearName),
                ownedGearStore: ownedGearStore,
                confirmOwned: gn => ConfirmOwnedTracking?.Invoke(gn)
                    ?? Task.FromResult(OwnedConfirmChoice.KeepTracking),
                onOwnedChanged: () => OnCardOwnedChangedAsync(card));
            card.SelectionChanged = RecountSelected;
            built.Add(card);
        }

        // Swap the freshly built set in (single synchronous step — no await between
        // the cancellation check above and here, so no interleaving with another reload).
        allCards.Clear();
        allCards.AddRange(built);

        // Populate collections with the full set; ApplyFilters sets per-card IsShown.
        ActiveCards.Clear();
        VaultedCards.Clear();
        foreach (var card in allCards)
        {
            if (card.IsVaulted) VaultedCards.Add(card);
            else ActiveCards.Add(card);
        }

        // Freshly built cards start unselected; reset the count (e.g. after a wishlist switch).
        RecountSelected();
        ApplyFilters();
    }

    /// <summary>
    /// Reacts to a card's owned state changing. When the card should be hidden (just
    /// marked owned while owned gear is hidden), it fades then collapses via IsShown.
    /// Otherwise re-evaluates all visibility so the card's IsShown reflects current filters.
    /// </summary>
    private async Task OnCardOwnedChangedAsync(PrimeGearCardViewModel card)
    {
        if (card.IsOwned && hideOwned)
        {
            if (OwnedHideFadeDuration > TimeSpan.Zero)
            {
                card.IsFadingOut = true;
                await Task.Delay(OwnedHideFadeDuration);
            }
            card.IsFadingOut = false;
            card.IsShown = false;
            // Update hint/empty-state tallies after hiding.
            ApplyFilters();
            return;
        }

        // Owned state changed in a way that may make the card visible — re-evaluate all.
        card.IsFadingOut = false;
        ApplyFilters();
    }

    // --- Group operations -------------------------------------------------

    /// <summary>
    /// Recomputes <see cref="SelectedCount"/> from the current card selection, and refreshes
    /// Mark/Unmark owned, whose availability depends on which cards are selected.
    /// </summary>
    private void RecountSelected()
    {
        SelectedCount = allCards.Count(c => c.IsSelected);
        markSelectedOwnedCommand.NotifyCanExecuteChanged();
        unmarkSelectedCommand.NotifyCanExecuteChanged();
    }

    /// <summary>Clears the selection on every card and resets the count.</summary>
    private void ClearSelection()
    {
        foreach (var card in allCards)
            card.IsSelected = false;
        RecountSelected();
    }

    /// <summary>
    /// Selects every card that is currently visible: shown by the owned/search filters and,
    /// for vaulted cards, only when the Vaulted section is expanded. Hidden cards stay unselected.
    /// </summary>
    private void SelectAllVisible()
    {
        foreach (var card in allCards)
            card.IsSelected = card.IsShown && (!card.IsVaulted || !hideVaulted);
        RecountSelected();
    }

    /// <summary>Clears the owned mark on every selected, currently-owned card.</summary>
    private void UnmarkSelected()
    {
        var targets = allCards.Where(c => c.IsSelected && c.IsOwned).ToList();

        // Each setter persists the change and triggers the owned-changed re-evaluation.
        foreach (var card in targets)
            card.IsOwned = false;

        ClearSelection();
    }

    /// <summary>
    /// Marks every selected, not-yet-owned card owned. If any selected card still tracks
    /// parts, asks once (Keep / Release / Cancel) and applies that choice to the whole batch.
    /// </summary>
    private async Task MarkSelectedOwnedAsync()
    {
        var targets = allCards.Where(c => c.IsSelected && !c.IsOwned).ToList();
        if (targets.Count == 0)
        {
            ClearSelection();
            return;
        }

        var withParts = targets.Where(c => c.HasWishedPartsForBulk).ToList();
        var choice = OwnedConfirmChoice.KeepTracking;
        if (withParts.Count > 0 && ConfirmOwnedTrackingBatch is not null)
        {
            choice = await ConfirmOwnedTrackingBatch(withParts.Count);
            // Cancel aborts the whole bulk mark and leaves the selection untouched.
            if (choice == OwnedConfirmChoice.Cancel)
                return;
        }

        var releaseTracking = choice == OwnedConfirmChoice.ReleaseTracking;
        // Run concurrently so the fade-outs (when owned gear is hidden) overlap rather
        // than cascading one card at a time.
        await Task.WhenAll(targets.Select(card => card.MarkOwnedForBulkAsync(releaseTracking)));

        ClearSelection();
    }

    private async Task ScheduleSearchAsync()
    {
        searchDebounce.Cancel();
        searchDebounce.Dispose();
        searchDebounce = new CancellationTokenSource();
        var token = searchDebounce.Token;

        if (SearchDebounceDelay > TimeSpan.Zero)
        {
            try { await Task.Delay(SearchDebounceDelay, token); }
            catch (OperationCanceledException) { return; }
        }

        if (!token.IsCancellationRequested)
            ApplyFilters();
    }

    /// <summary>
    /// Sets IsShown on every card from the composite predicate (owned filter + search),
    /// then updates hidden-match hint and visible-card signal.
    /// </summary>
    private void ApplyFilters()
    {
        var query = searchText;
        bool searchActive = query.Length >= 3;
        int hiddenOwned = 0, hiddenVaulted = 0, visible = 0;

        foreach (var card in allCards)
        {
            bool ownedHidden = card.IsOwned && hideOwned;
            bool wishlistedHidden = hideNotWishlisted && !card.Parts.Any(p => p.IsChecked);
            bool searchMatch = !searchActive || card.GearName.Contains(query, StringComparison.OrdinalIgnoreCase);

            card.IsShown = !ownedHidden && !wishlistedHidden && (!searchActive || searchMatch);

            if (searchActive && searchMatch)
            {
                if (ownedHidden)
                    hiddenOwned++;
                else if (wishlistedHidden)
                { /* not broken out in the hint */ }
                else if (card.IsVaulted && hideVaulted)
                    hiddenVaulted++;
                else
                    visible++;
            }
            else if (!searchActive && !ownedHidden && !wishlistedHidden)
            {
                // When no search, every non-hidden card counts as visible
                // (vaulted cards are visible within their section even when the section is collapsed).
                visible++;
            }
        }

        HiddenOwnedMatchCount = hiddenOwned;
        HiddenVaultedMatchCount = hiddenVaulted;

        var hintParts = new List<string>(2);
        if (hiddenOwned > 0) hintParts.Add($"{hiddenOwned} owned");
        if (hiddenVaulted > 0) hintParts.Add($"{hiddenVaulted} vaulted");
        HiddenMatchHint = hintParts.Count > 0
            ? string.Join(" and ", hintParts) + " hidden by filters"
            : string.Empty;
        HasHiddenMatches = hintParts.Count > 0;

        HasVisibleCards = visible > 0;
        HasNoSearchResults = searchActive && visible == 0;
    }
}
