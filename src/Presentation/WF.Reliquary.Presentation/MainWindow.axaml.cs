using Avalonia.Controls;
using WF.Reliquary.Application;

namespace WF.Reliquary.Presentation;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel(
            ApplicationInfo.Name,
            "Boilerplate shell only. Product workflows will be added in future features.");
    }
}

public sealed record MainWindowViewModel(string Title, string Summary);
