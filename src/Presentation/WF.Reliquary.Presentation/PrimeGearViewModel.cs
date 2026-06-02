using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WF.Reliquary.Application.Ports;

namespace WF.Reliquary.Presentation;

public sealed class PrimePartViewModel : INotifyPropertyChanged
{
    private bool isChecked;

    public PrimePartViewModel(string partName)
    {
        PartName = partName;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string PartName { get; }

    public bool IsChecked
    {
        get => isChecked;
        set
        {
            if (isChecked == value) return;
            isChecked = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsChecked)));
        }
    }
}

public sealed class PrimeGearCardViewModel
{
    public PrimeGearCardViewModel(string gearName, IEnumerable<string> parts, bool isVaulted)
    {
        GearName = gearName;
        Parts = new ObservableCollection<PrimePartViewModel>(
            parts.Select(p => new PrimePartViewModel(p)));
        IsVaulted = isVaulted;
    }

    public string GearName { get; }

    public ObservableCollection<PrimePartViewModel> Parts { get; }

    public bool IsVaulted { get; }
}

public sealed class PrimeGearViewModel : INotifyPropertyChanged
{
    private readonly IPrimeGearReader reader;
    private bool showVaulted;

    public PrimeGearViewModel(IPrimeGearReader reader)
    {
        this.reader = reader;
        ActiveCards = [];
        VaultedCards = [];
        _ = LoadAsync();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

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
        var entries = await reader.GetAllAsync();
        foreach (var entry in entries)
        {
            var card = new PrimeGearCardViewModel(entry.GearName, entry.Parts, entry.IsVaulted);
            if (entry.IsVaulted)
                VaultedCards.Add(card);
            else
                ActiveCards.Add(card);
        }
    }
}
