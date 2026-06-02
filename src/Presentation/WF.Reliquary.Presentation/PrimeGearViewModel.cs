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

public sealed class PrimeGearViewModel
{
    private readonly IPrimeGearReader reader;

    public PrimeGearViewModel(IPrimeGearReader reader)
    {
        this.reader = reader;
        Cards = [];
        _ = LoadAsync();
    }

    public ObservableCollection<PrimeGearCardViewModel> Cards { get; }

    public async Task ReloadAsync()
    {
        Cards.Clear();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var entries = await reader.GetAllAsync();
        foreach (var entry in entries)
        {
            Cards.Add(new PrimeGearCardViewModel(entry.GearName, entry.Parts, entry.IsVaulted));
        }
    }
}
