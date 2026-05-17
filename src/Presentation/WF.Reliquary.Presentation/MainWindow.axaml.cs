using Avalonia.Controls;
using WF.Reliquary.Composition;

namespace WF.Reliquary.Presentation;

public sealed partial class MainWindow : Window
{
    public MainWindow()
        : this(new MainWindowViewModel(
            ReliquaryServiceFactory.CreateDropTableSourceRefreshService(),
            ReliquaryServiceFactory.CreateDropTableImportService()))
    {
    }

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        _ = viewModel.LoadStatusAsync();
    }
}
