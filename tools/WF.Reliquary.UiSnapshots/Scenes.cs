using Avalonia;
using Avalonia.Controls;
using WF.Reliquary.Presentation;

namespace WF.Reliquary.UiSnapshots;

/// <summary>
/// One snapshot: a view built with stand-in data, the window size to render it at, the
/// app font size (pt) to apply first, and an optional pointer position for hover states.
/// </summary>
internal sealed record Scene(
    string Name,
    Func<Control> Build,
    double Width,
    double Height,
    double FontSize = Scenes.MediumFontSize,
    Point? Hover = null);

internal static class Scenes
{
    public const double MediumFontSize = 14;

    /// <summary>Add a scene here to make it available to every run.</summary>
    public static IReadOnlyList<Scene> All { get; } =
    [
        new("relics-stacked", () => new RelicsMissionsView { DataContext = StandIn.RelicsShell(stacked: true) }, 900, 900),
        new("relics-side-by-side", () => new RelicsMissionsView { DataContext = StandIn.RelicsShell(stacked: false) }, 1200, 400),
        new("relics-table-mode", () => new RelicsPanelView { DataContext = new { WishlistRelics = StandIn.WishlistRelics(table: true) } }, 900, 260),
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
        new("prime-gear-large-font", () => new PrimeGearView { DataContext = StandIn.PrimeGearShell() }, 760, 300, FontSize: 18),
    ];
}
