using System.Collections.ObjectModel;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.Wishlist;

namespace WF.Reliquary.Presentation;

public sealed class WishlistRelicRewardViewModel
{
    public WishlistRelicRewardViewModel(string rewardName, bool isWishlisted, bool showAllRewards)
    {
        RewardName = rewardName;
        IsWishlisted = isWishlisted;
        // When all rewards are shown, dim the ones that are not wishlisted so the
        // wishlisted reward is the only crisp text on the card.
        IsDimmed = showAllRewards && !isWishlisted;
    }

    public string RewardName { get; }
    public bool IsWishlisted { get; }
    public bool IsDimmed { get; }
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
        IEnumerable<string> rewardTiers,
        bool showAllRewards = true)
    {
        RelicName = relicName;
        Era = era;
        IsVaulted = isVaulted;

        var rewardList = rewards.ToList();
        var tierList = rewardTiers.ToList();

        var pairs = rewardList.Zip(tierList, (r, t) => (Reward: r, Tier: t));
        if (!showAllRewards)
        {
            pairs = pairs.Where(x => x.Reward.IsWishlisted);
        }

        var groups = pairs
            .GroupBy(x => x.Tier, StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => TierOrder.TryGetValue(g.Key, out var o) ? o : 99)
            .Select(g => new WishlistRelicTierGroupViewModel(g.Key, g.Select(x => x.Reward)))
            .ToList();

        TierGroups = new ObservableCollection<WishlistRelicTierGroupViewModel>(groups);
    }

    public string RelicName { get; }
    public string Era { get; }
    public bool IsVaulted { get; }
    public ObservableCollection<WishlistRelicTierGroupViewModel> TierGroups { get; }
}

public sealed class WishlistRelicsViewModel : ObservableObject
{
    private readonly IWishlistRelicsReader reader;
    private readonly IWishlistStore wishlistStore;
    private readonly ReloadGate reloadGate = new();
    private IReadOnlyList<WishlistRelicEntry> allEntries = [];
    private bool showVaulted;
    private bool showAllRewards;
    private bool isEmpty = true;
    private bool isBusy;

    public WishlistRelicsViewModel(IWishlistRelicsReader reader, IWishlistStore wishlistStore)
    {
        this.reader = reader;
        this.wishlistStore = wishlistStore;
        Cards = [];
        // Fire-and-forget initial load: failures flow to the global SafeFireAndForget handler.
        ReloadAsync().SafeFireAndForget();
    }

    public ObservableCollection<WishlistRelicCardViewModel> Cards { get; }

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

    public bool ShowVaulted
    {
        get => showVaulted;
        set
        {
            if (showVaulted == value) return;
            showVaulted = value;
            OnPropertyChanged();
            RebuildCards();
        }
    }

    public bool ShowAllRewards
    {
        get => showAllRewards;
        set
        {
            if (showAllRewards == value) return;
            showAllRewards = value;
            OnPropertyChanged();
            RebuildCards();
        }
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
                RebuildCards();
                return;
            }

            var entries = await reader.GetRelicsForWishlistAsync(activeWishlist.Id, token);
            if (token.IsCancellationRequested) return;

            allEntries = entries;
            RebuildCards();
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

    private void RebuildCards()
    {
        Cards.Clear();

        var visible = showVaulted
            ? allEntries
            : allEntries.Where(e => !e.IsVaulted);

        var cards = visible.Select(e => new WishlistRelicCardViewModel(
            e.DisplayName,
            e.Era,
            e.IsVaulted,
            e.Rewards.Select(r => new WishlistRelicRewardViewModel(r.RewardName, r.IsWishlisted, showAllRewards)),
            e.Rewards.Select(r => r.RewardTier),
            showAllRewards));

        foreach (var card in cards)
            Cards.Add(card);

        IsEmpty = Cards.Count == 0;
    }
}
