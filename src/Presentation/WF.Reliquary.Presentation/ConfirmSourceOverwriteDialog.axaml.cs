using Avalonia.Controls;
using Avalonia.Interactivity;
using WF.Reliquary.Application.DropTables;

namespace WF.Reliquary.Presentation;

public sealed partial class ConfirmSourceOverwriteDialog : Window
{
    public ConfirmSourceOverwriteDialog()
    {
        InitializeComponent();
        ReplaceButton.Click += OnReplace;
        CancelButton.Click += OnCancel;
    }

    public static Task<bool> ShowAsync(Window owner, DropTableSourceSummary existing)
    {
        var dialog = new ConfirmSourceOverwriteDialog();
        dialog.VersionText.Text = string.IsNullOrWhiteSpace(existing.LastUpdate)
            ? "(version unknown)"
            : existing.LastUpdate;
        dialog.DownloadedAtText.Text = $"Downloaded: {existing.DownloadedAt.LocalDateTime:g}";
        return dialog.ShowDialog<bool>(owner);
    }

    private void OnReplace(object? sender, RoutedEventArgs e) => Close(true);

    private void OnCancel(object? sender, RoutedEventArgs e) => Close(false);
}
