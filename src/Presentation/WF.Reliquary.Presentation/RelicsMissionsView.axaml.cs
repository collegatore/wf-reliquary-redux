using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using WF.Reliquary.Application.Settings;

namespace WF.Reliquary.Presentation;

public sealed partial class RelicsMissionsView : UserControl
{
    private MainWindowShellViewModel? _shell;

    // True once the Relics/Missions grid rows have been converted to pixel sizing.
    // This is deferred until the grid is first measured (tab must be visible).
    private bool _splitterInitialized;

    public RelicsMissionsView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        RelicsMissionsGrid.SizeChanged += OnRelicsGridFirstMeasured;
        RelicsSplitter.DragCompleted += OnSplitterDragCompleted;
        SideBySideSplitter.DragCompleted += OnSideBySideSplitterDragCompleted;
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (_shell is not null)
        {
            _shell.PropertyChanged -= OnShellPropertyChanged;
            _shell.WishlistRelics.PropertyChanged -= OnWishlistRelicsPropertyChanged;
        }

        _shell = DataContext as MainWindowShellViewModel;

        if (_shell is not null)
        {
            _shell.PropertyChanged += OnShellPropertyChanged;
            _shell.WishlistRelics.PropertyChanged += OnWishlistRelicsPropertyChanged;

            // Restore column ratio immediately if starting in side-by-side orientation.
            if (_shell.Orientation == RelicsMissionsOrientation.SideBySide)
                ApplySideBySideRelicsWidth();
        }
    }

    /// <summary>
    /// One-shot: fires until the grid first has a non-zero height (i.e. the
    /// Relics tab becomes visible and is laid out), converts the star rows to
    /// pixel rows, seeds the initial min-height, then unsubscribes itself so
    /// it never runs again during horizontal resize.
    /// </summary>
    private void OnRelicsGridFirstMeasured(object? sender, SizeChangedEventArgs e)
    {
        var available = AvailableRelicsHeight();
        if (available <= 0) return;   // not yet measured — wait for next event

        // Done — unsubscribe before touching layout to avoid re-entrancy.
        RelicsMissionsGrid.SizeChanged -= OnRelicsGridFirstMeasured;

        var relicsHeight = _shell?.RelicsPanelHeight is { } saved && saved > 0
            ? Math.Min(saved, available - 10)
            : available / 3.0; // default 1:2

        SetRelicsRowHeight(relicsHeight);
        _splitterInitialized = true;

        UpdateRelicsMinHeight();
    }

    private void OnSplitterDragCompleted(object? sender, VectorEventArgs e)
    {
        if (_shell is null) return;

        var height = RelicsMissionsGrid.RowDefinitions[1].ActualHeight;
        _shell.RelicsPanelHeight = height > 0 ? height : null;
    }

    private void OnShellPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowShellViewModel.RelicsPanelHeight))
        {
            Dispatcher.UIThread.Post(ApplyRelicsPanelHeight);
        }
        else if (e.PropertyName == nameof(MainWindowShellViewModel.Orientation)
            && _shell?.Orientation == RelicsMissionsOrientation.SideBySide)
        {
            // Restore the saved column ratio whenever switching to side-by-side.
            ApplySideBySideRelicsWidth();
        }
    }

    private void OnWishlistRelicsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(WishlistRelicsViewModel.ShowAllRewards))
        {
            // Cards re-render after ShowAllRewards changes; wait for the next
            // layout pass so Bounds.Height values are fresh.
            Dispatcher.UIThread.Post(UpdateRelicsMinHeight, DispatcherPriority.Render);
        }
    }

    /// <summary>
    /// Applies a (possibly updated) relics panel height from the shell — called
    /// when Settings are saved or the Reset button is pressed.
    /// </summary>
    private void ApplyRelicsPanelHeight()
    {
        if (!_splitterInitialized) return;

        var available = AvailableRelicsHeight();
        if (available <= 0) return;

        var relicsHeight = _shell?.RelicsPanelHeight is { } h && h > 0
            ? Math.Min(h, available - 10)
            : available / 3.0; // default after reset

        SetRelicsRowHeight(relicsHeight);
    }

    private void SetRelicsRowHeight(double relicsHeight)
    {
        var grid = RelicsMissionsGrid;
        grid.RowDefinitions[1].Height = new GridLength(relicsHeight, GridUnitType.Pixel);
        // Row 3 stays as 1* so the missions panel fills the remaining space.
        grid.RowDefinitions[3].Height = new GridLength(1, GridUnitType.Star);
    }

    private double AvailableRelicsHeight()
    {
        var grid = RelicsMissionsGrid;
        return grid.Bounds.Height
            - grid.RowDefinitions[0].ActualHeight
            - grid.RowDefinitions[2].ActualHeight;
    }

    private void OnSideBySideSplitterDragCompleted(object? sender, VectorEventArgs e)
    {
        if (_shell is null) return;

        var width = SideBySideGrid.ColumnDefinitions[0].ActualWidth;
        if (width > 0)
            _shell.SideBySideRelicsWidth = width;
    }

    // Restores the relics column to its saved pixel width; the missions column
    // takes the remaining space as a star (so it stretches on window resize).
    private void ApplySideBySideRelicsWidth()
    {
        if (_shell?.SideBySideRelicsWidth is not { } width || width <= 0) return;

        SideBySideGrid.ColumnDefinitions[0].Width = new GridLength(width, GridUnitType.Pixel);
        SideBySideGrid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
    }

    /// <summary>
    /// Sets the minimum allowed height for the relics row to the tallest card
    /// currently visible in the relics panel. Re-evaluated after ShowAllRewards
    /// toggles and on every grid resize.
    /// </summary>
    private void UpdateRelicsMinHeight()
    {
        if (!_splitterInitialized) return;

        // Find the WrapPanel that holds the relic cards.
        var wrapPanel = RelicsScrollViewer
            .GetVisualDescendants()
            .OfType<WrapPanel>()
            .FirstOrDefault();

        if (wrapPanel is null) return;

        var maxCardHeight = wrapPanel.Children
            .OfType<Control>()
            .Where(c => c.IsVisible && c.Bounds.Height > 0)
            .Select(c => c.Bounds.Height)
            .DefaultIfEmpty(0)
            .Max();

        if (maxCardHeight > 0)
        {
            // Row 1 is the relics scroller only — the control bar lives in row 0
            // and must not be included in this minimum.
            RelicsMissionsGrid.RowDefinitions[1].MinHeight = maxCardHeight + 8;
        }
    }
}
