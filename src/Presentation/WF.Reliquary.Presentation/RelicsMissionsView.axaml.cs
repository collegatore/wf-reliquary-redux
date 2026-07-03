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

            UpdateSideBySideMinWidth();
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

        // Row 0 is the relics region (shared panel + missions separator).
        var height = RelicsMissionsGrid.RowDefinitions[0].ActualHeight;
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
        else if (e.PropertyName == nameof(MainWindowShellViewModel.RelicCardWidth))
        {
            Dispatcher.UIThread.Post(UpdateSideBySideMinWidth);
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
        // Row 0 is the relics region; Row 2 (missions) stays 1* to fill the rest.
        grid.RowDefinitions[0].Height = new GridLength(relicsHeight, GridUnitType.Pixel);
        grid.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
    }

    private double AvailableRelicsHeight()
    {
        var grid = RelicsMissionsGrid;
        // Only the splitter (Row 1) is a fixed row; Row 0 (relics region) and Row 2
        // (missions) split the remaining height. The control bar is now inside the
        // relics region, so it is no longer subtracted separately.
        return grid.Bounds.Height - grid.RowDefinitions[1].ActualHeight;
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

    // Keeps the side-by-side Relics column's minimum width one full card wide,
    // derived from the current effective (font-scaled) relic card width rather
    // than a fixed literal — recomputed whenever card width or font size changes.
    private void UpdateSideBySideMinWidth()
    {
        if (_shell is null) return;

        SideBySideGrid.ColumnDefinitions[0].MinWidth = _shell.RelicCardWidth + 16;
    }

    /// <summary>
    /// Sets the minimum allowed height for the relics region so at least one full
    /// card stays visible in the scroll. Re-evaluated after ShowAllRewards toggles
    /// and on every grid resize.
    /// </summary>
    private void UpdateRelicsMinHeight()
    {
        if (!_splitterInitialized) return;

        // The relics scroll lives inside the shared RelicsPanelView (a separate
        // namescope), so reach it through the visual tree of the stacked instance.
        var scroll = StackedRelicsPanel
            .GetVisualDescendants()
            .OfType<ScrollViewer>()
            .FirstOrDefault();
        if (scroll is null) return;

        var wrapPanel = scroll
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

        if (maxCardHeight <= 0) return;

        // Row 0 now holds the whole relics region (control bar + scroll + missions
        // separator). Add the region's non-scroll overhead so the minimum leaves at
        // least one full card of scroll, not one card total.
        var region = RelicsMissionsGrid.RowDefinitions[0];
        var overhead = Math.Max(0, region.ActualHeight - scroll.Bounds.Height);
        region.MinHeight = maxCardHeight + 8 + overhead;
    }
}
