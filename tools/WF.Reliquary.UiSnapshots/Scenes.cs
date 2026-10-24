using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Threading;
using Avalonia.VisualTree;
using WF.Reliquary.Application.Settings;
using WF.Reliquary.Presentation;

namespace WF.Reliquary.UiSnapshots;

/// <summary>
/// One snapshot: a view built with stand-in data, the window size to render it at, the
/// app font size (pt) and color palette to apply first, an optional pointer position
/// for hover states, and an optional step run once the window is shown (e.g. opening a
/// dropdown or menu).
/// </summary>
internal sealed record Scene(
    string Name,
    Func<Control> Build,
    double Width,
    double Height,
    double FontSize = Scenes.MediumFontSize,
    Point? Hover = null,
    ColorPalette Palette = ColorPalette.Linen,
    Action<Window>? Prepare = null);

internal static class Scenes
{
    public const double MediumFontSize = 14;

    /// <summary>Add a scene here to make it available to every run.</summary>
    public static IReadOnlyList<Scene> All { get; } =
    [
        new("relics-stacked", () => new RelicsMissionsView { DataContext = StandIn.RelicsShell(stacked: true) }, 900, 900),
        new("relics-side-by-side", () => new RelicsMissionsView { DataContext = StandIn.RelicsShell(stacked: false) }, 1200, 400),
        new("relics-table-mode", () => new RelicsPanelView { DataContext = new { WishlistRelics = StandIn.WishlistRelics(table: true) } }, 900, 260),
        new("relics-table-rows", () => new RelicsPanelView { DataContext = new { WishlistRelics = StandIn.WishlistRelics(table: true) } }, 900, 260, Prepare: FillTable),
        new("relics-table-rows-hover", () => new RelicsPanelView { DataContext = new { WishlistRelics = StandIn.WishlistRelics(table: true) } }, 900, 260, Hover: new Point(88, 180), Prepare: FillTable),
        new("relics-table-rows-stone", () => new RelicsPanelView { DataContext = new { WishlistRelics = StandIn.WishlistRelics(table: true) } }, 900, 260, Prepare: FillTable, Palette: ColorPalette.Stone),
        new("relics-sort-hover", () => new RelicsPanelView { DataContext = new { WishlistRelics = StandIn.WishlistRelics() } }, 900, 260, Hover: new Point(48, 137)),
        new("relics-small-font", () => new RelicsPanelView { DataContext = new { WishlistRelics = StandIn.WishlistRelics() } }, 760, 220, FontSize: 11),
        new("relics-large-font", () => new RelicsPanelView { DataContext = new { WishlistRelics = StandIn.WishlistRelics() } }, 760, 260, FontSize: 18),
        new("prime-gear", () => new PrimeGearView { DataContext = StandIn.PrimeGearShell() }, 900, 300),
        new("prime-gear-hover", () => new PrimeGearView { DataContext = StandIn.PrimeGearShell() }, 900, 300, Hover: new Point(260, 37)),
        new("prime-gear-joined-hover", () => new PrimeGearView { DataContext = StandIn.PrimeGearShell() }, 900, 300, Hover: new Point(240, 149)),
        new("prime-gear-chip-hover", () => new PrimeGearView { DataContext = StandIn.PrimeGearShell() }, 900, 300, Hover: new Point(283, 93)),
        new("prime-gear-cards", () => new PrimeGearView { DataContext = StandIn.PrimeGearShell(groupMode: false) }, 900, 480),
        new("prime-gear-group-mode", () => new PrimeGearView { DataContext = StandIn.PrimeGearShell(groupMode: true, selected: false) }, 900, 480),
        new("prime-gear-group-mode-selected-hover", () => new PrimeGearView { DataContext = StandIn.PrimeGearShell() }, 900, 480, Hover: new Point(300, 300)),
        new("prime-gear-owned-hidden", () => new PrimeGearView { DataContext = StandIn.PrimeGearShell(showOwned: false) }, 900, 480),
        new("relics-side-by-side-stone", () => new RelicsMissionsView { DataContext = StandIn.RelicsShell(stacked: false) }, 1200, 400, Palette: ColorPalette.Stone),
        new("prime-gear-stone", () => new PrimeGearView { DataContext = StandIn.PrimeGearShell() }, 900, 480, Palette: ColorPalette.Stone),
        new("prime-gear-hover-stone", () => new PrimeGearView { DataContext = StandIn.PrimeGearShell() }, 900, 300, Hover: new Point(260, 37), Palette: ColorPalette.Stone),
        new("prime-gear-chip-hover-stone", () => new PrimeGearView { DataContext = StandIn.PrimeGearShell() }, 900, 300, Hover: new Point(283, 93), Palette: ColorPalette.Stone),
        new("prime-gear-large-font", () => new PrimeGearView { DataContext = StandIn.PrimeGearShell() }, 760, 300, FontSize: 18),
        new("prime-gear-wishlist-open", () => new PrimeGearView { DataContext = StandIn.PrimeGearShell() }, 900, 300, Prepare: OpenWishlistSelector),
        new("prime-gear-wishlist-open-long", () => new PrimeGearView { DataContext = StandIn.PrimeGearShell(longWishlistName: true) }, 900, 300, Prepare: OpenWishlistSelector),
        new("prime-gear-wishlist-open-stone", () => new PrimeGearView { DataContext = StandIn.PrimeGearShell() }, 900, 300, Prepare: OpenWishlistSelector, Palette: ColorPalette.Stone),
        new("prime-gear-wishlist-keyboard", () => new PrimeGearView { DataContext = StandIn.PrimeGearShell() }, 900, 300, Prepare: PickSecondWishlistWithKeyboard),
        new("prime-gear-wishlist-actions", () => new PrimeGearView { DataContext = StandIn.PrimeGearShell() }, 900, 300, Prepare: OpenWishlistActions),
        new("prime-gear-wishlist-actions-delete-hover", () => new PrimeGearView { DataContext = StandIn.PrimeGearShell() }, 900, 300, Hover: new Point(260, 97), Prepare: OpenWishlistActions),
        new("prime-gear-wishlist-actions-single", () => new PrimeGearView { DataContext = StandIn.PrimeGearShell(singleWishlist: true) }, 900, 300, Prepare: OpenWishlistActions),
        new("prime-gear-wishlist-actions-stone", () => new PrimeGearView { DataContext = StandIn.PrimeGearShell() }, 900, 300, Prepare: OpenWishlistActions, Palette: ColorPalette.Stone),
        new("prime-gear-wishlist-renamed", () => new PrimeGearView { DataContext = StandIn.PrimeGearShell() }, 900, 120, Prepare: RenameActiveWishlist),
        new("prime-gear-card-menu", () => new PrimeGearView { DataContext = StandIn.PrimeGearShell(groupMode: false) }, 900, 300, Prepare: w => OpenCardMenu(w, keyboard: false)),
        new("prime-gear-card-menu-keyboard", () => new PrimeGearView { DataContext = StandIn.PrimeGearShell(groupMode: false) }, 900, 300, Prepare: w => OpenCardMenu(w, keyboard: true)),
        new("settings-profile-open", () => new SettingsView { DataContext = StandIn.SettingsShell() }, 900, 260, Prepare: w => w.GetVisualDescendants().OfType<ComboBox>().First().IsDropDownOpen = true),
        new("settings-profile-actions", () => new SettingsView { DataContext = StandIn.SettingsShell() }, 900, 260, Prepare: OpenFirstActionsMenu),
        new("recruiting-message-dialog", RecruitingDialogContent, 460, 360),
        new("icons", IconGallery, 760, 200),
        new("icons-large-font", IconGallery, 760, 240, FontSize: 18),
    ];

    private static void OpenWishlistSelector(Window window) =>
        window.GetVisualDescendants().OfType<ComboBox>().First().IsDropDownOpen = true;

    /// <summary>Focus the selector, open it, move down one entry and confirm: the closed
    /// field should then read "Farming".</summary>
    private static void PickSecondWishlistWithKeyboard(Window window)
    {
        var combo = window.GetVisualDescendants().OfType<ComboBox>().First();
        combo.Focus();
        foreach (var (key, physical) in new[] { (Key.F4, PhysicalKey.F4), (Key.Down, PhysicalKey.ArrowDown), (Key.Enter, PhysicalKey.Enter) })
        {
            window.KeyPress(key, RawInputModifiers.None, physical, null);
            Dispatcher.UIThread.RunJobs();
        }
    }

    /// <summary>
    /// Gives Table mode rows. The view builds its table source only for the real shell
    /// view model, so call its (private) builder directly with stand-in rows. Headless,
    /// the star-sized name column renders collapsed; the scene is for the marker and
    /// action-icon columns.
    /// </summary>
    private static void FillTable(Window window)
    {
        var table = window.GetVisualDescendants().OfType<Avalonia.Controls.TreeDataGrid>().First(t => t.Name == "RelicsTable");
        var build = typeof(RelicsPanelView).GetMethod("BuildTableSource", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("RelicsPanelView.BuildTableSource not found; update FillTable.");
        table.Source = (ITreeDataGridSource)build.Invoke(null, [StandIn.TableRows()])!;
    }

    /// <summary>Renames the active list in its view model: the closed field must follow.</summary>
    private static void RenameActiveWishlist(Window window)
    {
        var combo = window.GetVisualDescendants().OfType<ComboBox>().First();
        ((WishlistEntryViewModel)combo.SelectedItem!).Name = "Renamed list";
    }

    /// <summary>Opens the first gear card's ⋮ menu; with <paramref name="keyboard"/>, then
    /// presses Down, which must highlight an item.</summary>
    private static void OpenCardMenu(Window window, bool keyboard)
    {
        var button = window.GetVisualDescendants().OfType<Button>()
            .First(b => b.Flyout is MenuFlyout && !b.Classes.Contains("icon-only") && b.IsEffectivelyVisible);
        button.Flyout!.ShowAt(button);
        Dispatcher.UIThread.RunJobs();
        if (keyboard) window.KeyPress(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, null);
    }

    private static void OpenWishlistActions(Window window) => OpenFirstActionsMenu(window);

    private static void OpenFirstActionsMenu(Window window)
    {
        var button = window.GetVisualDescendants().OfType<Button>().First(b => b.Classes.Contains("icon-only"));
        button.Flyout!.ShowAt(button);
    }

    /// <summary>
    /// The Recruiting Chat message dialog's content, moved into the scene window (a
    /// Window can't be nested). It keeps the dialog's own DataContext.
    /// </summary>
    private static Control RecruitingDialogContent()
    {
        var dialog = new RecruitingMessageDialog();
        var content = (Control)dialog.Content!;
        var dataContext = dialog.DataContext;
        dialog.Content = null;
        content.DataContext = dataContext;
        return content;
    }

    /// <summary>Every <c>Icon.*</c> resource from Styles/Icons.axaml, each with its key.</summary>
    private static Control IconGallery()
    {
        var panel = new WrapPanel { Margin = new Thickness(12) };
        var keys = Avalonia.Application.Current!.Resources.MergedDictionaries
            .OfType<IResourceDictionary>()
            .SelectMany(d => d.Keys.OfType<string>())
            .Where(k => k.StartsWith("Icon.", StringComparison.Ordinal))
            .Order();
        foreach (var key in keys)
        {
            var icon = new PathIcon { [!PathIcon.DataProperty] = new DynamicResourceExtension(key) };
            panel.Children.Add(new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Spacing = 6,
                Margin = new Thickness(0, 0, 18, 10),
                Children = { icon, new TextBlock { Text = key } },
            });
        }
        return panel;
    }
}
