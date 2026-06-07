using Avalonia.Controls;
using Avalonia.Interactivity;

namespace WF.Reliquary.Presentation;

public sealed partial class ConfirmDiscardDialog : Window
{
    public ConfirmDiscardDialog()
    {
        InitializeComponent();
        DiscardButton.Click += OnDiscard;
        CancelButton.Click += OnCancel;
    }

    /// <summary>
    /// Shows the dialog modally. Returns <c>true</c> if the user chose to discard.
    /// </summary>
    public static Task<bool> ShowAsync(Window owner)
    {
        var dialog = new ConfirmDiscardDialog();
        return dialog.ShowDialog<bool>(owner);
    }

    private void OnDiscard(object? sender, RoutedEventArgs e) => Close(true);

    private void OnCancel(object? sender, RoutedEventArgs e) => Close(false);
}
