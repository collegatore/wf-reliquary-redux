using Avalonia.Controls;
using Avalonia.Interactivity;

namespace WF.Reliquary.Presentation;

public sealed partial class WishlistNameDialog : Window
{
    // Required by Avalonia's XAML loader.
    public WishlistNameDialog() : this(null, "Wishlist name") { }

    private WishlistNameDialog(string? prefill, string label)
    {
        InitializeComponent();
        Title = label;
        PromptLabel.Text = $"{label}:";
        NameBox.Text = prefill ?? string.Empty;
        OkButton.Click += OnOk;
        CancelButton.Click += OnCancel;

        // Select all text so the user can immediately type a new name.
        Opened += (_, _) =>
        {
            NameBox.Focus();
            NameBox.SelectAll();
        };
    }

    /// <summary>
    /// Shows the dialog as a modal child of <paramref name="owner"/>.
    /// Returns the entered name on confirmation, or null on cancel / empty input.
    /// </summary>
    public static Task<string?> ShowAsync(Window owner, string? prefill = null, string label = "Wishlist name")
    {
        var dialog = new WishlistNameDialog(prefill, label);
        return dialog.ShowDialog<string?>(owner);
    }

    private void OnOk(object? sender, RoutedEventArgs e)
    {
        var name = NameBox.Text?.Trim();
        Close(string.IsNullOrEmpty(name) ? null : name);
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }
}
