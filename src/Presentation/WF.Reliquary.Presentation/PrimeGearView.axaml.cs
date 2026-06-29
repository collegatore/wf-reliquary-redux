namespace WF.Reliquary.Presentation;

public sealed partial class PrimeGearView : Avalonia.Controls.UserControl
{
    public PrimeGearView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (DataContext is MainWindowShellViewModel shell)
        {
            shell.Wishlist.FlyoutCloseRequested = () => WishlistDropdownButton.Flyout?.Hide();
        }
    }
}
