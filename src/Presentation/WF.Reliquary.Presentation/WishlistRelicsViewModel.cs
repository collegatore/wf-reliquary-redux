using System.Collections.ObjectModel;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.Settings;
using WF.Reliquary.Application.Wishlist;

namespace WF.Reliquary.Presentation;

public sealed class WishlistRelicRewardViewModel
{
    public WishlistRelicRewardViewModel(string rewardName, string tier, bool isWishlisted, bool showAllRewards)
    {
        RewardName = rewardName;
        Tier = tier;
        IsWishlisted = isWishlisted;
        // When all rewards are shown, dim the ones that are not wishlisted so the
        // wishlisted reward is the only crisp text on the card.
        IsDimmed = showAllRewards && !isWishlisted;
        CompactName = CompactWording.ClipRewardName(rewardName);
        IsCommon = string.Equals(tier, "Common", StringComparison.OrdinalIgnoreCase);
        IsUncommon = string.Equals(tier, "Uncommon", StringComparison.OrdinalIgnoreCase);
        IsRare = string.Equals(tier, "Rare", StringComparison.OrdinalIgnoreCase);
        TierLetter = IsCommon ? "C" : IsUncommon ? "U" : IsRare ? "R" : string.Empty;
    }

    public string RewardName { get; }
    public string Tier { get; }
    public bool IsWishlisted { get; }
    public bool IsDimmed { get; }

    /// <summary>Reward name with "Prime"/"Blueprint" clipped, for Compact and Table modes.</summary>
    public string CompactName { get; }

    /// <summary>Trailing tier marker — "(C)", "(U)", or "(R)" — for Compact and Table modes.</summary>
    public string TierLetter { get; }

    // Per-line tier accent bar classes for Compact mode (mirrors the tier-group
    // booleans on WishlistRelicTierGroupViewModel, one level down).
    public bool IsCommon { get; }
    public bool IsUncommon { get; }
    public bool IsRare { get; }
}

public sealed class WishlistRelicTierGroupViewModel
{
    public WishlistRelicTierGroupViewModel(string tierName, IEnumerable<WishlistRelicRewardViewModel> rewards)
    {
        TierName = tierName;
        IsCommon = string.Equals(tierName, "Common", StringComparison.OrdinalIgnoreCase);
        IsUncommon = string.Equals(tierName, "Uncommon", StringComparison.OrdinalIgnoreCase);
        IsRare = string.Equals(tierName, "Rare", StringComparison.OrdinalIgnoreCase);
        Rewards = new ObservableCollection<WishlistRelicRewardViewModel>(rewards);
    }

    public string TierName { get; }
    public bool IsCommon { get; }
    public bool IsUncommon { get; }
    public bool IsRare { get; }
    public ObservableCollection<WishlistRelicRewardViewModel> Rewards { get; }
}

public sealed class WishlistRelicCardViewModel
{
    private static readonly Dictionary<string, int> TierOrder = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Common"] = 1,
        ["Uncommon"] = 2,
        ["Rare"] = 3
    };

    public WishlistRelicCardViewModel(
        string relicName,
        string era,
        bool isVaulted,
        IEnumerable<WishlistRelicRewardViewModel> rewards,
        bool showAllRewards = true,
        Func<Task>? openCompose = null)
    {
        RelicName = relicName;
        CompactRelicName = CompactWording.ClipRelicName(relicName);
        Era = era;
        IsVaulted = isVaulted;

        var visibleRewards = showAllRewards
            ? rewards.ToList()
            : rewards.Where(r => r.IsWishlisted).ToList();

        var orderedRewards = visibleRewards
            .OrderBy(r => TierOrder.TryGetValue(r.Tier, out var o) ? o : 99)
            .ToList();

        var groups = orderedRewards
            .GroupBy(r => r.Tier, StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => TierOrder.TryGetValue(g.Key, out var o) ? o : 99)
            .Select(g => new WishlistRelicTierGroupViewModel(g.Key, g))
            .ToList();

        TierGroups = new ObservableCollection<WishlistRelicTierGroupViewModel>(groups);
        Rewards = new ObservableCollection<WishlistRelicRewardViewModel>(orderedRewards);
        ComposeCommand = new AsyncRelayCommand(openCompose ?? (() => Task.CompletedTask));
    }

    public string RelicName { get; }

    /// <summary>Relic name with the trailing " Relic" clipped, for Compact and Table modes.</summary>
    public string CompactRelicName { get; }

    public string Era { get; }
    public bool IsVaulted { get; }
    public ObservableCollection<WishlistRelicTierGroupViewModel> TierGroups { get; }

    /// <summary>Flat, tier-ordered reward list (no per-tier grouping), for Compact mode cards.</summary>
    public ObservableCollection<WishlistRelicRewardViewModel> Rewards { get; }

    /// <summary>Opens the Recruiting Chat message dialog for this relic.</summary>
    public AsyncRelayCommand ComposeCommand { get; }
}

public sealed class WishlistRelicsViewModel : ObservableObject
{
    private readonly IWishlistRelicsReader reader;
    private readonly IWishlistStore wishlistStore;
    private readonly ReloadGate reloadGate = new();
    private IReadOnlyList<WishlistRelicEntry> allEntries = [];
    private bool hideVaulted;
    private bool showAllRewards;
    private bool isEmpty = true;
    private bool isBusy;
    private RelicViewMode viewMode = RelicViewMode.Compact;
    private RelicSortMode sortMode = RelicSortMode.Era;
    private bool sortDescending = true;

    /// <summary>
    /// Set by MainWindow to open the Recruiting Chat message dialog for a given relic name.
    /// </summary>
    public Func<string, Task>? OpenRecruitingMessageCompose { get; set; }

    /// <summary>
    /// Set by MainWindow to open the rewards modal (Verbatim card, all rewards) for a
    /// relic's Table row. Wired by the shell; used by each relic row's rewards pictogram.
    /// </summary>
    public Func<WishlistRelicCardViewModel, Task>? OpenRewardsModal { get; set; }

    /// <summary>
    /// Set by MainWindow to persist sort mode and direction when they change.
    /// Called fire-and-forget after each user-initiated sort change.
    /// </summary>
    public Func<Task>? SaveSortSettingsAsync { get; set; }

    /// <summary>
    /// Set by MainWindow to persist the Relics filters (Hide vaulted) when they change.
    /// Called fire-and-forget after each user change; not on <see cref="LoadFilterSettings"/>.
    /// </summary>
    public Func<Task>? SaveFilterSettingsAsync { get; set; }

    /// <summary>
    /// Set by MainWindow to persist the Display menu (view mode, Show all rewards) when it
    /// changes. Called fire-and-forget after each user change; not on <see cref="LoadDisplaySettings"/>.
    /// </summary>
    public Func<Task>? SaveDisplaySettingsAsync { get; set; }

    public WishlistRelicsViewModel(IWishlistRelicsReader reader, IWishlistStore wishlistStore)
    {
        this.reader = reader;
        this.wishlistStore = wishlistStore;
        Cards = [];
        TableRows = [];
        ToggleSortDirectionCommand = new RelayCommand(() => SortDescending = !SortDescending);
        // Fire-and-forget initial load: failures flow to the global SafeFireAndForget handler.
        ReloadAsync().SafeFireAndForget();
    }

    public ObservableCollection<WishlistRelicCardViewModel> Cards { get; }

    /// <summary>Top-level relic rows for the Table view mode (each expands into reward rows).</summary>
    public ObservableCollection<WishlistRelicTableRow> TableRows { get; }

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

    public bool IsEmpty
    {
        get => isEmpty;
        private set
        {
            if (isEmpty == value) return;
            isEmpty = value;
            OnPropertyChanged();
        }
    }

    /// <summary>The Filters menu's "Hide: Vaulted" entry. Off by default (nothing hidden).</summary>
    public bool HideVaulted
    {
        get => hideVaulted;
        set
        {
            if (hideVaulted == value) return;
            hideVaulted = value;
            OnFilterChanged();
            RebuildViews();
            SaveFilterSettingsAsync?.Invoke().SafeFireAndForget();
        }
    }

    /// <summary>The Display menu's Show all rewards entry (card modes only).</summary>
    public bool ShowAllRewards
    {
        get => showAllRewards;
        set
        {
            if (showAllRewards == value) return;
            showAllRewards = value;
            OnDisplayChanged();
            RebuildViews();
            SaveDisplaySettingsAsync?.Invoke().SafeFireAndForget();
        }
    }

    /// <summary>
    /// The active relic view mode (Verbatim / Compact / Table), chosen in the Display menu.
    /// A user change persists; <see cref="LoadDisplaySettings"/> restores without saving.
    /// </summary>
    public RelicViewMode ViewMode
    {
        get => viewMode;
        set
        {
            if (viewMode == value) return;
            viewMode = value;
            OnDisplayChanged();
            SaveDisplaySettingsAsync?.Invoke().SafeFireAndForget();
        }
    }

    public bool IsViewModeVerbatim => viewMode == RelicViewMode.Verbatim;
    public bool IsViewModeCompact => viewMode == RelicViewMode.Compact;
    public bool IsViewModeTable => viewMode == RelicViewMode.Table;

    /// <summary>True while the Filters menu hides anything; drives the Filters button's marker.</summary>
    public bool IsFilterActive => hideVaulted;

    /// <summary>Tooltip text for the Filters button.</summary>
    public string FilterSummary => hideVaulted ? "Filters: hiding vaulted relics" : "Filters: nothing hidden";

    /// <summary>True unless the sort is the default Era descending; drives the sort icon's marker.</summary>
    public bool IsSortNonDefault => sortMode != RelicSortMode.Era || !sortDescending;

    /// <summary>Tooltip text for the sort-mode dropdown and the direction button.</summary>
    public string SortSummary
    {
        get
        {
            var mode = sortMode switch
            {
                RelicSortMode.Era => "Sorted by era",
                RelicSortMode.Alphabetical => "Sorted alphabetically",
                _ => "Sorted by wishlist score",
            };
            return $"{mode}, {(sortDescending ? "descending" : "ascending")}";
        }
    }

    /// <summary>True unless the display is the default (Compact, Show all rewards off).</summary>
    public bool IsDisplayNonDefault => viewMode != RelicViewMode.Compact || showAllRewards;

    /// <summary>Tooltip text for the Display button.</summary>
    public string DisplaySummary
    {
        get
        {
            var text = $"Display: {viewMode} view";
            return showAllRewards && viewMode != RelicViewMode.Table ? text + ", all rewards shown" : text;
        }
    }

    /// <summary>Toggles the sort direction between ascending and descending.</summary>
    public IRelayCommand ToggleSortDirectionCommand { get; }

    /// <summary>
    /// Active sort mode. Changing to a different mode applies that mode's default direction
    /// (descending for Era/WishlistScore, ascending for Alphabetical). Re-selecting the same
    /// mode leaves direction unchanged.
    /// </summary>
    public RelicSortMode SortMode
    {
        get => sortMode;
        set
        {
            if (sortMode == value) return;
            sortMode = value;
            // Apply the new mode's default direction directly on the backing field so that
            // RebuildViews is called only once (not once per field change).
            sortDescending = value != RelicSortMode.Alphabetical;
            OnSortChanged();
            RebuildViews();
            SaveSortSettingsAsync?.Invoke().SafeFireAndForget();
        }
    }

    /// <summary>
    /// Zero-based index into the three sort modes (Era=0, Alphabetical=1, WishlistScore=2),
    /// for binding to a ComboBox.SelectedIndex.
    /// </summary>
    public int SortModeIndex
    {
        get => sortMode switch { RelicSortMode.Era => 0, RelicSortMode.Alphabetical => 1, _ => 2 };
        set => SortMode = value switch { 0 => RelicSortMode.Era, 1 => RelicSortMode.Alphabetical, _ => RelicSortMode.WishlistScore };
    }

    /// <summary>
    /// Whether the active sort is descending. Toggling triggers a view rebuild and persists.
    /// </summary>
    public bool SortDescending
    {
        get => sortDescending;
        set
        {
            if (sortDescending == value) return;
            sortDescending = value;
            OnSortChanged();
            RebuildViews();
            SaveSortSettingsAsync?.Invoke().SafeFireAndForget();
        }
    }

    /// <summary>
    /// Restores persisted sort settings at startup without triggering the default-direction
    /// logic that fires on user-initiated mode changes.
    /// </summary>
    public void LoadSortSettings(RelicSortMode mode, bool descending)
    {
        sortMode = mode;
        sortDescending = descending;
        OnSortChanged();
        if (allEntries.Count > 0)
            RebuildViews();
    }

    /// <summary>Restores the persisted Relics filters at startup without saving them.</summary>
    public void LoadFilterSettings(bool hideVaulted)
    {
        this.hideVaulted = hideVaulted;
        OnFilterChanged();
        if (allEntries.Count > 0)
            RebuildViews();
    }

    /// <summary>Restores the persisted Display menu state at startup without saving it.</summary>
    public void LoadDisplaySettings(RelicViewMode viewMode, bool showAllRewards)
    {
        this.viewMode = viewMode;
        this.showAllRewards = showAllRewards;
        OnDisplayChanged();
        if (allEntries.Count > 0)
            RebuildViews();
    }

    private void OnFilterChanged()
    {
        OnPropertyChanged(nameof(HideVaulted));
        OnPropertyChanged(nameof(IsFilterActive));
        OnPropertyChanged(nameof(FilterSummary));
    }

    private void OnSortChanged()
    {
        OnPropertyChanged(nameof(SortMode));
        OnPropertyChanged(nameof(SortModeIndex));
        OnPropertyChanged(nameof(SortDescending));
        OnPropertyChanged(nameof(IsSortNonDefault));
        OnPropertyChanged(nameof(SortSummary));
    }

    private void OnDisplayChanged()
    {
        OnPropertyChanged(nameof(ViewMode));
        OnPropertyChanged(nameof(IsViewModeVerbatim));
        OnPropertyChanged(nameof(IsViewModeCompact));
        OnPropertyChanged(nameof(IsViewModeTable));
        OnPropertyChanged(nameof(ShowAllRewards));
        OnPropertyChanged(nameof(IsDisplayNonDefault));
        OnPropertyChanged(nameof(DisplaySummary));
    }

    public async Task ReloadAsync()
    {
        var token = reloadGate.Begin();
        IsBusy = true;
        try
        {
            var activeWishlist = await wishlistStore.GetActiveAsync(token);
            if (activeWishlist is null)
            {
                if (token.IsCancellationRequested) return;
                allEntries = [];
                RebuildViews();
                return;
            }

            var entries = await reader.GetRelicsForWishlistAsync(activeWishlist.Id, token);
            if (token.IsCancellationRequested) return;

            allEntries = entries;
            RebuildViews();
        }
        catch (OperationCanceledException)
        {
            // Superseded by a newer reload.
        }
        finally
        {
            if (!token.IsCancellationRequested)
                IsBusy = false;
        }
    }

    private void RebuildViews()
    {
        // Same filtered/sorted relic set feeds every view mode, so the modes stay
        // consistent. Reader guarantees grouping contiguity (display_name ORDER BY) and
        // within-relic reward order (tier + source_order); ViewModel owns relic-level ordering.
        var visible = (hideVaulted
            ? allEntries.Where(e => !e.IsVaulted)
            : allEntries).ToList();

        var sorted = ApplySort(visible);

        RebuildCards(sorted);
        RebuildTableRows(sorted);

        IsEmpty = Cards.Count == 0;
    }

    private IReadOnlyList<WishlistRelicEntry> ApplySort(IReadOnlyList<WishlistRelicEntry> entries)
    {
        return sortMode switch
        {
            RelicSortMode.Alphabetical => sortDescending
                ? [.. entries.OrderByDescending(e => e.DisplayName)]
                : [.. entries.OrderBy(e => e.DisplayName)],

            RelicSortMode.WishlistScore => sortDescending
                ? [.. entries
                    .OrderByDescending(ComputeWishlistScore)
                    .ThenByDescending(e => EraRank(e.Era))
                    .ThenBy(e => e.DisplayName)]
                : [.. entries
                    .OrderBy(ComputeWishlistScore)
                    .ThenBy(e => EraRank(e.Era))
                    .ThenBy(e => e.DisplayName)],

            // Era (default)
            _ => sortDescending
                ? [.. entries.OrderByDescending(e => EraRank(e.Era)).ThenBy(e => e.DisplayName)]
                : [.. entries.OrderBy(e => EraRank(e.Era)).ThenBy(e => e.DisplayName)],
        };
    }

    private static int EraRank(string era) => era switch
    {
        "Lith" => 1,
        "Meso" => 2,
        "Neo" => 3,
        "Axi" => 4,
        _ => 99
    };

    private static int ComputeWishlistScore(WishlistRelicEntry entry) =>
        entry.Rewards.Where(r => r.IsWishlisted).Sum(r => r.RewardTier switch
        {
            "Common" => 3,
            "Uncommon" => 4,
            "Rare" => 5,
            _ => 0
        });

    private void RebuildCards(IReadOnlyList<WishlistRelicEntry> visible)
    {
        Cards.Clear();

        foreach (var e in visible)
        {
            Cards.Add(new WishlistRelicCardViewModel(
                e.DisplayName,
                e.Era,
                e.IsVaulted,
                e.Rewards.Select(r => new WishlistRelicRewardViewModel(r.RewardName, r.RewardTier, r.IsWishlisted, showAllRewards)),
                showAllRewards,
                openCompose: OpenRecruitingMessageCompose is { } fn ? () => fn(e.DisplayName) : null));
        }
    }

    private void RebuildTableRows(IReadOnlyList<WishlistRelicEntry> visible)
    {
        TableRows.Clear();

        foreach (var e in visible)
            TableRows.Add(BuildRelicRow(e));
    }

    private RelicTableRowViewModel BuildRelicRow(WishlistRelicEntry e)
    {
        // Child reward rows show only wishlisted rewards (Show all rewards is hidden
        // in Table mode, and non-wishlisted rewards are simply omitted rather than
        // dimmed), tier-ordered.
        var children = e.Rewards
            .Where(r => r.IsWishlisted)
            .OrderBy(r => TierRank(r.RewardTier))
            .Select(r =>
            {
                var reward = new WishlistRelicRewardViewModel(r.RewardName, r.RewardTier, r.IsWishlisted, showAllRewards: false);
                return (WishlistRelicTableRow)new RewardTableRowViewModel(reward.TierLetter, reward.CompactName);
            })
            .ToList();

        // All-rewards Verbatim card the rewards pictogram opens in a modal.
        var allRewardsCard = new WishlistRelicCardViewModel(
            e.DisplayName,
            e.Era,
            e.IsVaulted,
            e.Rewards.Select(r => new WishlistRelicRewardViewModel(r.RewardName, r.RewardTier, r.IsWishlisted, showAllRewards: true)),
            showAllRewards: true,
            openCompose: OpenRecruitingMessageCompose is { } compose ? () => compose(e.DisplayName) : null);

        var showRewards = new AsyncRelayCommand(() => OpenRewardsModal?.Invoke(allRewardsCard) ?? Task.CompletedTask);
        var composeRecruitingMessage = new AsyncRelayCommand(() => OpenRecruitingMessageCompose is { } fn ? fn(e.DisplayName) : Task.CompletedTask);

        return new RelicTableRowViewModel(
            CompactWording.ClipRelicName(e.DisplayName),
            BuildTierMarkers(e.Rewards),
            e.IsVaulted,
            children,
            showRewards,
            composeRecruitingMessage);
    }

    private static int TierRank(string tier) =>
        string.Equals(tier, "Common", StringComparison.OrdinalIgnoreCase) ? 1
        : string.Equals(tier, "Uncommon", StringComparison.OrdinalIgnoreCase) ? 2
        : string.Equals(tier, "Rare", StringComparison.OrdinalIgnoreCase) ? 3
        : 99;

    /// <summary>
    /// Tiers with ≥1 wishlisted reward, in C→U→R order. Count prefix when >1,
    /// e.g. "C, 2 R" for 1 wishlisted common and 2 wishlisted rares.
    /// </summary>
    private static string BuildTierMarkers(IReadOnlyList<WishlistRelicReward> rewards)
    {
        var parts = new List<string>(3);
        AddMarker(parts, rewards, "Common", "C");
        AddMarker(parts, rewards, "Uncommon", "U");
        AddMarker(parts, rewards, "Rare", "R");
        return string.Join(", ", parts);

        static void AddMarker(List<string> parts, IReadOnlyList<WishlistRelicReward> rewards, string tier, string letter)
        {
            var n = rewards.Count(r => r.IsWishlisted && string.Equals(r.RewardTier, tier, StringComparison.OrdinalIgnoreCase));
            if (n == 1) parts.Add(letter);
            else if (n > 1) parts.Add($"{n} {letter}");
        }
    }
}
