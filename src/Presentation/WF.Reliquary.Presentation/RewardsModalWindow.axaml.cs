using Avalonia.Controls;

namespace WF.Reliquary.Presentation;

public sealed partial class RewardsModalWindow : Window
{
    // Required by Avalonia's XAML loader.
    public RewardsModalWindow() : this(EmptyCard) { }

    private RewardsModalWindow(WishlistRelicCardViewModel card)
    {
        InitializeComponent();
        DataContext = new RewardsModalViewModel(card);
    }

    /// <summary>
    /// Shows the modal as a modal child of <paramref name="owner"/>, hosting
    /// <paramref name="card"/> (an all-rewards, Verbatim-mode card) for the relic
    /// behind a Table row's rewards pictogram.
    /// </summary>
    public static Task ShowAsync(Window owner, WishlistRelicCardViewModel card) =>
        new RewardsModalWindow(card).ShowDialog(owner);

    private static WishlistRelicCardViewModel EmptyCard { get; } =
        new(string.Empty, string.Empty, isVaulted: false, rewards: []);
}

/// <summary>Thin DataContext wrapper so the window binds through a settled property name.</summary>
public sealed class RewardsModalViewModel
{
    public RewardsModalViewModel(WishlistRelicCardViewModel card) => Card = card;

    public WishlistRelicCardViewModel Card { get; }
}
