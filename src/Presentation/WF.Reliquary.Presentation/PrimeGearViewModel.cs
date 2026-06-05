using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.Wishlist;

namespace WF.Reliquary.Presentation;

public sealed class PrimePartViewModel : INotifyPropertyChanged
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

    public event PropertyChangedEventHandler? PropertyChanged;

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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsChecked)));
            _ = PersistCheckedAsync(value);
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

public sealed class PrimeGearCardViewModel
{
    public PrimeGearCardViewModel(string gearName, IEnumerable<PrimePartViewModel> parts, bool isVaulted)
    {
        GearName = gearName;
        Parts = new ObservableCollection<PrimePartViewModel>(parts);
        IsVaulted = isVaulted;
    }

    public string GearName { get; }
    public ObservableCollection<PrimePartViewModel> Parts { get; }
    public bool IsVaulted { get; }
}

public sealed class PrimeGearViewModel : INotifyPropertyChanged
{
    private readonly IPrimeGearReader reader;
    private readonly IWishlistStore wishlistStore;
    private long activeWishlistId;
    private bool showVaulted;

    public PrimeGearViewModel(IPrimeGearReader reader, IWishlistStore wishlistStore)
    {
        this.reader = reader;
        this.wishlistStore = wishlistStore;
        ActiveCards = [];
        VaultedCards = [];
        _ = LoadAsync();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public Func<Task>? OnWishlistChanged { get; set; }

    public ObservableCollection<PrimeGearCardViewModel> ActiveCards { get; }
    public ObservableCollection<PrimeGearCardViewModel> VaultedCards { get; }

    public bool ShowVaulted
    {
        get => showVaulted;
        set
        {
            if (showVaulted == value) return;
            showVaulted = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowVaulted)));
        }
    }

    public async Task ReloadAsync()
    {
        ActiveCards.Clear();
        VaultedCards.Clear();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var activeWishlist = await wishlistStore.GetActiveAsync();
        if (activeWishlist is null)
        {
            // No wishlists at all — show nothing (should only happen in tests).
            return;
        }

        activeWishlistId = activeWishlist.Id;

        var wishlistItems = await wishlistStore.GetItemsAsync(activeWishlistId);
        var importEntries = await reader.GetAllAsync();

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

            var card = new PrimeGearCardViewModel(gearName, partVms, isVaulted);
            if (isVaulted)
                VaultedCards.Add(card);
            else
                ActiveCards.Add(card);
        }
    }
}
