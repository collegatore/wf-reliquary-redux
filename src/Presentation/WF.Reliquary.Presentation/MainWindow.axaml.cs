using Avalonia.Controls;
using WF.Reliquary.Composition;

namespace WF.Reliquary.Presentation;

public sealed partial class MainWindow : Window
{
    public MainWindow()
        : this(new MainWindowShellViewModel(
            new MainWindowViewModel(
                ReliquaryServiceFactory.CreateDropTableSourceRefreshService(),
                ReliquaryServiceFactory.CreateDropTableImportService()),
            new PrimeGearViewModel(
                ReliquaryServiceFactory.CreatePrimeGearReader())))
    {
    }

    public MainWindow(MainWindowShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        _ = viewModel.DropTables.LoadStatusAsync();
    }
}
