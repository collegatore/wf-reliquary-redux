using System.Collections.ObjectModel;
using System.ComponentModel;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
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

public sealed class WishlistFarmingMissionsViewModel : ObservableObject
{
    private readonly IWishlistFarmingMissionsReader reader;
    private readonly IWishlistStore wishlistStore;
    private readonly ReloadGate reloadGate = new();
    private bool isEmpty = true;
    private bool showExactDropChances = false;
    private bool isBusy;

    public WishlistFarmingMissionsViewModel(
        IWishlistFarmingMissionsReader reader,
        IWishlistStore wishlistStore)
    {
        this.reader = reader;
        this.wishlistStore = wishlistStore;
        Cards = [];
        // Fire-and-forget initial load: failures flow to the global SafeFireAndForget handler.
        ReloadAsync().SafeFireAndForget();
    }

    public ObservableCollection<WishlistFarmingMissionGroupCardViewModel> Cards { get; }

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

    public bool ShowExactDropChances
    {
        get => showExactDropChances;
        set
        {
            if (showExactDropChances == value) return;
            showExactDropChances = value;
            OnPropertyChanged();
            foreach (var card in Cards)
                card.ShowExactDropChances = value;
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
                Cards.Clear();
                IsEmpty = true;
                return;
            }

            var entries = await reader.GetMissionsForWishlistAsync(activeWishlist.Id, token);
            if (token.IsCancellationRequested) return;

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
}
