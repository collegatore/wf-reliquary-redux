namespace WF.Reliquary.Presentation;

public sealed partial class SettingsView : Avalonia.Controls.UserControl
{
    public SettingsView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (DataContext is MainWindowShellViewModel shell)
        {
            shell.Profiles.FlyoutCloseRequested = () => ProfileDropdownButton.Flyout?.Hide();
        }
    }
}
