using System.Collections.ObjectModel;
using System.ComponentModel;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.Wishlist;

namespace WF.Reliquary.Presentation;

public sealed class WishlistFarmingMissionRelicViewModel
{
    public WishlistFarmingMissionRelicViewModel(string relicName, double chancePercent)
    {
        RelicName = relicName;
        FormattedChance = $"{chancePercent:F2}%";
    }

    public string RelicName { get; }
    public string FormattedChance { get; }
}

public sealed class WishlistFarmingRotationViewModel : INotifyPropertyChanged
{
    private bool showExactDropChances = false;

    public WishlistFarmingRotationViewModel(
        string rotationLabel,
        double cumulativeChance,
        IEnumerable<WishlistFarmingMissionRelicViewModel> relics)
    {
        RotationLabel = rotationLabel;
        FormattedCumulativeChance = $"{cumulativeChance:F2}%";
        Relics = new ObservableCollection<WishlistFarmingMissionRelicViewModel>(relics);
        CompactRelicNames = string.Join(", ", Relics.Select(r => r.RelicName));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string RotationLabel { get; }
    public string FormattedCumulativeChance { get; }
    public ObservableCollection<WishlistFarmingMissionRelicViewModel> Relics { get; }
    public string CompactRelicNames { get; }

    public bool ShowExactDropChances
    {
        get => showExactDropChances;
        set
        {
            if (showExactDropChances == value) return;
            showExactDropChances = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowExactDropChances)));
        }
    }
}

public sealed class WishlistFarmingMissionGroupCardViewModel : INotifyPropertyChanged
{
    private bool showExactDropChances = false;

    public WishlistFarmingMissionGroupCardViewModel(WishlistFarmingMissionGroupEntry entry)
    {
        GroupTitle = entry.GroupTitle;
        NodesLabel = string.Join(", ", entry.Nodes);
        FormattedEffectiveChance = entry.HasRotations
            ? $"avg {entry.EffectiveChance:F2}%"
            : $"{entry.EffectiveChance:F2}%";
        HasRotations = entry.HasRotations;

        Rotations = new ObservableCollection<WishlistFarmingRotationViewModel>(
            entry.Rotations.Select(r => new WishlistFarmingRotationViewModel(
                r.RotationDisplayLabel,
                r.CumulativeChance,
                r.Relics.Select(rel => new WishlistFarmingMissionRelicViewModel(rel.RelicName, rel.ChancePercent)))));

        Relics = new ObservableCollection<WishlistFarmingMissionRelicViewModel>(
            entry.Relics.Select(r => new WishlistFarmingMissionRelicViewModel(r.RelicName, r.ChancePercent)));

        CompactRelicNames = string.Join(", ", entry.Relics.Select(r => r.RelicName));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string GroupTitle { get; }
    public string NodesLabel { get; }
    public string FormattedEffectiveChance { get; }
    public bool HasRotations { get; }
    public ObservableCollection<WishlistFarmingRotationViewModel> Rotations { get; }
    public ObservableCollection<WishlistFarmingMissionRelicViewModel> Relics { get; }
    public string CompactRelicNames { get; }

    public bool ShowExactDropChances
    {
        get => showExactDropChances;
        set
        {
            if (showExactDropChances == value) return;
            showExactDropChances = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowExactDropChances)));
            foreach (var rotation in Rotations)
                rotation.ShowExactDropChances = value;
        }
    }
}

public sealed class WishlistFarmingMissionsViewModel : INotifyPropertyChanged
{
    private readonly IWishlistFarmingMissionsReader reader;
    private readonly IWishlistStore wishlistStore;
    private bool isEmpty = true;
    private bool showExactDropChances = false;

    public WishlistFarmingMissionsViewModel(
        IWishlistFarmingMissionsReader reader,
        IWishlistStore wishlistStore)
    {
        this.reader = reader;
        this.wishlistStore = wishlistStore;
        Cards = [];
        _ = ReloadAsync();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<WishlistFarmingMissionGroupCardViewModel> Cards { get; }

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

    public bool ShowExactDropChances
    {
        get => showExactDropChances;
        set
        {
            if (showExactDropChances == value) return;
            showExactDropChances = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowExactDropChances)));
            foreach (var card in Cards)
                card.ShowExactDropChances = value;
        }
    }

    public async Task ReloadAsync()
    {
        var activeWishlist = await wishlistStore.GetActiveAsync();
        if (activeWishlist is null)
        {
            Cards.Clear();
            IsEmpty = true;
            return;
        }

        var entries = await reader.GetMissionsForWishlistAsync(activeWishlist.Id);
        Cards.Clear();
        foreach (var entry in entries)
        {
            var card = new WishlistFarmingMissionGroupCardViewModel(entry)
            {
                ShowExactDropChances = showExactDropChances
            };
            Cards.Add(card);
        }
        IsEmpty = Cards.Count == 0;
    }
}
