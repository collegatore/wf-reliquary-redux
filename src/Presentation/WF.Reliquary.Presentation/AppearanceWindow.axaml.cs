using Avalonia.Controls;

namespace WF.Reliquary.Presentation;

public sealed partial class AppearanceWindow : Window
{
    // Required by Avalonia's XAML loader.
    public AppearanceWindow() => InitializeComponent();

    public AppearanceWindow(AppearanceViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
