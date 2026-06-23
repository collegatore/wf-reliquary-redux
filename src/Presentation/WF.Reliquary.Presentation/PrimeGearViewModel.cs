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
    }

    public string GearName { get; }
    public ObservableCollection<PrimePartViewModel> Parts { get; }
    public bool IsVaulted { get; }

    /// <summary>Toggles owned state — invoked from the card's overflow menu.</summary>
    public ICommand MarkOwnedCommand { get; }

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
    private long activeWishlistId;
    private bool showVaulted;
    private bool showOwned;
    private bool isBusy;

    public PrimeGearViewModel(IPrimeGearReader reader, IWishlistStore wishlistStore, IOwnedGearStore ownedGearStore)
    {
        this.reader = reader;
        this.wishlistStore = wishlistStore;
        this.ownedGearStore = ownedGearStore;
        ActiveCards = [];
        VaultedCards = [];
        // Fire-and-forget initial load: failures flow to the global SafeFireAndForget handler.
        ReloadAsync().SafeFireAndForget();
    }

    public Func<Task>? OnWishlistChanged { get; set; }

    /// <summary>Requests the three-way owned-confirmation dialog. Set by the view.</summary>
    public Func<string, Task<OwnedConfirmChoice>>? ConfirmOwnedTracking { get; set; }

    /// <summary>
    /// How long a card fades before it is removed after being marked owned while owned
    /// gear is hidden. Defaults to zero (no fade) so tests update synchronously; the
    /// view sets a short non-zero duration for the visible animation.
    /// </summary>
    public TimeSpan OwnedHideFadeDuration { get; set; } = TimeSpan.Zero;

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

    public bool ShowVaulted
    {
        get => showVaulted;
        set
        {
            if (showVaulted == value) return;
            showVaulted = value;
            OnPropertyChanged();
        }
    }

    public bool ShowOwned
    {
        get => showOwned;
        set
        {
            if (showOwned == value) return;
            showOwned = value;
            OnPropertyChanged();
            RebuildVisibleCards();
        }
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
            built.Add(card);
        }

        // Swap the freshly built set in (single synchronous step — no await between
        // the cancellation check above and here, so no interleaving with another reload).
        allCards.Clear();
        allCards.AddRange(built);

        RebuildVisibleCards();
    }

    /// <summary>
    /// Reacts to a single card's owned state changing. Rather than rebuilding the whole
    /// grid (which regenerates every container), this adds or removes just the affected
    /// card. When the card is being hidden it first plays a short fade so the removal —
    /// and the overlapping database write — read as a smooth transition.
    /// </summary>
    private async Task OnCardOwnedChangedAsync(PrimeGearCardViewModel card)
    {
        var target = card.IsVaulted ? VaultedCards : ActiveCards;

        if (card.IsOwned && !showOwned)
        {
            if (!target.Contains(card)) return;
            if (OwnedHideFadeDuration > TimeSpan.Zero)
            {
                card.IsFadingOut = true;
                await Task.Delay(OwnedHideFadeDuration);
            }
            target.Remove(card);
            return;
        }

        // Card should be visible — clear any fade and insert it in gear-name order.
        card.IsFadingOut = false;
        if (!target.Contains(card))
            InsertInOrder(target, card);
    }

    /// <summary>Inserts <paramref name="card"/> into <paramref name="target"/> at the
    /// position dictated by the full card ordering and the current visibility filter.</summary>
    private void InsertInOrder(ObservableCollection<PrimeGearCardViewModel> target, PrimeGearCardViewModel card)
    {
        var visibleOrder = allCards
            .Where(c => c.IsVaulted == card.IsVaulted && (showOwned || !c.IsOwned))
            .ToList();
        var index = visibleOrder.IndexOf(card);
        if (index < 0 || index > target.Count) index = target.Count;
        target.Insert(index, card);
    }

    /// <summary>
    /// Repopulates the bound Active/Vaulted collections from the full card set,
    /// hiding owned cards unless <see cref="ShowOwned"/> is set. Used for initial load
    /// and the Show owned gear toggle, where every card's visibility may change.
    /// </summary>
    private void RebuildVisibleCards()
    {
        ActiveCards.Clear();
        VaultedCards.Clear();
        foreach (var card in allCards)
        {
            if (card.IsOwned && !showOwned) continue;
            card.IsFadingOut = false;
            if (card.IsVaulted)
                VaultedCards.Add(card);
            else
                ActiveCards.Add(card);
        }
    }
}
