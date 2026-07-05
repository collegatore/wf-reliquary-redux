using Avalonia.Controls;
using Avalonia.Interactivity;
using WF.Reliquary.Application;

namespace WF.Reliquary.Presentation;

public sealed partial class AboutDialog : Window
{
    public AboutDialog()
    {
        InitializeComponent();
        NameText.Text = ApplicationInfo.Name;
        VersionText.Text = $"Version {ApplicationInfo.Version}";
        CloseButton.Click += OnClose;
    }

    /// <summary>
    /// Shows the dialog as a modal child of <paramref name="owner"/>.
    /// </summary>
    public static Task ShowAsync(Window owner)
    {
        var dialog = new AboutDialog();
        return dialog.ShowDialog(owner);
    }

    private void OnClose(object? sender, RoutedEventArgs e) => Close();
}
