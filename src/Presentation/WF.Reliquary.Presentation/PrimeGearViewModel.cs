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
    public PrimeGearCardViewModel(string gearName, IEnumerable<string> parts)
    {
        GearName = gearName;
        Parts = new ObservableCollection<PrimePartViewModel>(
            parts.Select(p => new PrimePartViewModel(p)));
    }

    public string GearName { get; }

    public ObservableCollection<PrimePartViewModel> Parts { get; }
}

public sealed class PrimeGearViewModel
{
    public PrimeGearViewModel(IPrimeGearReader reader)
    {
        Cards = [];
        _ = LoadAsync(reader);
    }

    public ObservableCollection<PrimeGearCardViewModel> Cards { get; }

    private async Task LoadAsync(IPrimeGearReader reader)
    {
        var entries = await reader.GetAllAsync();
        foreach (var entry in entries)
        {
            Cards.Add(new PrimeGearCardViewModel(entry.GearName, entry.Parts));
        }
    }
}
