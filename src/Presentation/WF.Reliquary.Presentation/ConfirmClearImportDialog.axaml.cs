using Avalonia.Controls;
using Avalonia.Interactivity;

namespace WF.Reliquary.Presentation;

public sealed partial class ConfirmClearImportDialog : Window
{
    public ConfirmClearImportDialog()
    {
        InitializeComponent();
        ConfirmButton.Click += OnConfirm;
        CancelButton.Click += OnCancel;
    }

    /// <summary>
    /// Shows the dialog modally. Returns <c>true</c> if the user chose to confirm.
    /// </summary>
    public static Task<bool> ShowAsync(Window owner)
    {
        var dialog = new ConfirmClearImportDialog();
        return dialog.ShowDialog<bool>(owner);
    }

    private void OnConfirm(object? sender, RoutedEventArgs e) => Close(true);

    private void OnCancel(object? sender, RoutedEventArgs e) => Close(false);
}
