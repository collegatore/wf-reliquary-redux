using System.Collections.ObjectModel;
using System.ComponentModel;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.Wishlist;

namespace WF.Reliquary.Presentation;

public sealed class WishlistRelicRewardViewModel
{
    public WishlistRelicRewardViewModel(string rewardName, bool isWishlisted)
    {
        RewardName = rewardName;
        IsWishlisted = isWishlisted;
    }

    public string RewardName { get; }
    public bool IsWishlisted { get; }
}

public sealed class WishlistRelicTierGroupViewModel
{
    public WishlistRelicTierGroupViewModel(string tierName, IEnumerable<WishlistRelicRewardViewModel> rewards)
    {
        TierName = tierName + ":";
        Rewards = new ObservableCollection<WishlistRelicRewardViewModel>(rewards);
    }

    public string TierName { get; }
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

        TierGroups = new ObservableCollection<WishlistRelicTierGroupViewModel>(
            pairs
                .GroupBy(x => x.Tier, StringComparer.OrdinalIgnoreCase)
                .OrderBy(g => TierOrder.TryGetValue(g.Key, out var o) ? o : 99)
                .Select(g => new WishlistRelicTierGroupViewModel(g.Key, g.Select(x => x.Reward))));
    }

    public string RelicName { get; }
    public string Era { get; }
    public bool IsVaulted { get; }
    public ObservableCollection<WishlistRelicTierGroupViewModel> TierGroups { get; }
}

public sealed class WishlistRelicsViewModel : INotifyPropertyChanged
{
    private readonly IWishlistRelicsReader reader;
    private readonly IWishlistStore wishlistStore;
    private IReadOnlyList<WishlistRelicEntry> allEntries = [];
    private bool showVaulted;
    private bool showAllRewards;
    private bool isEmpty = true;

    public WishlistRelicsViewModel(IWishlistRelicsReader reader, IWishlistStore wishlistStore)
    {
        this.reader = reader;
        this.wishlistStore = wishlistStore;
        Cards = [];
        _ = ReloadAsync();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<WishlistRelicCardViewModel> Cards { get; }

    public bool IsEmpty
    {
        get => isEmpty;
        private set
        {
            if (isEmpty == value) return;
            isEmpty = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEmpty)));
        }
    }

    public bool ShowVaulted
    {
        get => showVaulted;
        set
        {
            if (showVaulted == value) return;
            showVaulted = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowVaulted)));
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowAllRewards)));
            RebuildCards();
        }
    }

    public async Task ReloadAsync()
    {
        var activeWishlist = await wishlistStore.GetActiveAsync();
        if (activeWishlist is null)
        {
            allEntries = [];
            RebuildCards();
            return;
        }

        allEntries = await reader.GetRelicsForWishlistAsync(activeWishlist.Id);
        RebuildCards();
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
            e.Rewards.Select(r => new WishlistRelicRewardViewModel(r.RewardName, r.IsWishlisted)),
            e.Rewards.Select(r => r.RewardTier),
            showAllRewards));

        foreach (var card in cards)
            Cards.Add(card);

        IsEmpty = Cards.Count == 0;
    }
}
