using Avalonia.Controls;
using Avalonia.Interactivity;

namespace WF.Reliquary.Presentation;

public sealed partial class WishlistNameDialog : Window
{
    private readonly IReadOnlyCollection<string> takenNames;

    // Required by Avalonia's XAML loader.
    public WishlistNameDialog() : this(null, "Wishlist name", []) { }

    private WishlistNameDialog(string? prefill, string label, IReadOnlyCollection<string> takenNames)
    {
        InitializeComponent();
        this.takenNames = takenNames;
        Title = label;
        PromptLabel.Text = $"{label}:";
        NameBox.MaxLength = NameRules.MaxLength;
        NameBox.Text = prefill ?? string.Empty;
        NameBox.TextChanged += (_, _) => Validate();
        OkButton.Click += OnOk;
        CancelButton.Click += OnCancel;

        // Select all text so the user can immediately type a new name.
        Opened += (_, _) =>
        {
            NameBox.Focus();
            NameBox.SelectAll();
        };

        // Initialize the error message and OK enabled-state for the prefilled value.
        Validate();
    }

    /// <summary>
    /// Shows the dialog as a modal child of <paramref name="owner"/>.
    /// Returns the entered name (trimmed) on confirmation, or null on cancel.
    /// The name is validated against non-empty, length, and uniqueness rules; the
    /// dialog stays open until the name is valid or the user cancels.
    /// </summary>
    public static Task<string?> ShowAsync(Window owner, NameDialogRequest request, string label = "Wishlist name")
    {
        var dialog = new WishlistNameDialog(request.Prefill, label, request.TakenNames);
        return dialog.ShowDialog<string?>(owner);
    }

    private void Validate()
    {
        var error = NameRules.Validate(NameBox.Text, takenNames);
        ErrorText.Text = error ?? string.Empty;
        ErrorText.IsVisible = error is not null;
        OkButton.IsEnabled = error is null;
    }

    private void OnOk(object? sender, RoutedEventArgs e)
    {
        var name = NameBox.Text?.Trim();
        if (NameRules.Validate(name, takenNames) is not null)
        {
            // Re-show the inline message and keep the dialog open.
            Validate();
            return;
        }

        Close(name);
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }
}
