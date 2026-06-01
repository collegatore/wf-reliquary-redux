namespace WF.Reliquary.Presentation;

public sealed class MainWindowShellViewModel
{
    public MainWindowShellViewModel(MainWindowViewModel dropTables, PrimeGearViewModel primeGear)
    {
        DropTables = dropTables;
        PrimeGear = primeGear;
    }

    public MainWindowViewModel DropTables { get; }

    public PrimeGearViewModel PrimeGear { get; }
}
