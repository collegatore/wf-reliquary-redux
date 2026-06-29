using Avalonia.Controls;
using Avalonia.Interactivity;

namespace WF.Reliquary.Presentation;

public sealed partial class OwnedGearTrackingDialog : Window
{
    public OwnedGearTrackingDialog()
    {
        InitializeComponent();
        KeepTrackingButton.Click += OnKeepTracking;
        ReleaseTrackingButton.Click += OnReleaseTracking;
        CancelButton.Click += OnCancel;
    }

    /// <summary>
    /// Shows the dialog modally for <paramref name="gearName"/>. Closing or dismissing
    /// the dialog resolves to <see cref="OwnedConfirmChoice.Cancel"/>.
    /// </summary>
    public static Task<OwnedConfirmChoice> ShowAsync(Window owner, string gearName)
    {
        var dialog = new OwnedGearTrackingDialog();
        dialog.MessageText.Text =
            $"“{gearName}” has parts you are still tracking in your wishlist. " +
            "What should happen to them?";
        return dialog.ShowDialog<OwnedConfirmChoice>(owner);
    }

    /// <summary>
    /// Shows the dialog modally for a bulk mark, where <paramref name="trackedCount"/> of the
    /// selected gear still have tracked parts. The chosen outcome applies to the whole batch.
    /// Closing or dismissing the dialog resolves to <see cref="OwnedConfirmChoice.Cancel"/>.
    /// </summary>
    public static Task<OwnedConfirmChoice> ShowBatchAsync(Window owner, int trackedCount)
    {
        var noun = trackedCount == 1 ? "gear" : "gear items";
        var dialog = new OwnedGearTrackingDialog();
        dialog.MessageText.Text =
            $"{trackedCount} selected {noun} still have parts you are tracking in your wishlist. " +
            "What should happen to them?";
        return dialog.ShowDialog<OwnedConfirmChoice>(owner);
    }

    private void OnKeepTracking(object? sender, RoutedEventArgs e) => Close(OwnedConfirmChoice.KeepTracking);

    private void OnReleaseTracking(object? sender, RoutedEventArgs e) => Close(OwnedConfirmChoice.ReleaseTracking);

    private void OnCancel(object? sender, RoutedEventArgs e) => Close(OwnedConfirmChoice.Cancel);
}
