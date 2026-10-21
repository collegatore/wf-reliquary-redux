using CommunityToolkit.Mvvm.ComponentModel;
using AvaloniaApp = Avalonia.Application;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.Settings;

namespace WF.Reliquary.Presentation;

public sealed class AppearanceViewModel : ObservableObject
{
    private readonly IUserSettingsStore settingsStore;

    private const double SmallPt = 11.0;
    private const double MediumPt = 14.0;
    private const double LargePt = 18.0;

    private double fontSizePt;          // sole font-size state; SelectedFontSize is derived
    private double relicCardWidthPx;    // base width at Medium font size
    private RelicsMissionsOrientation orientation;
    private RelicViewMode viewMode;

    public AppearanceViewModel(IUserSettingsStore settingsStore, UserSettings initialSettings)
    {
        this.settingsStore = settingsStore;

        fontSizePt = initialSettings.FontSizePt;
        relicCardWidthPx = initialSettings.RelicCardWidthPx;
        orientation = initialSettings.Orientation;
        viewMode = initialSettings.ViewMode;
    }

    // ── Delegates wired by the shell ────────────────────────────────────────

    /// <summary>
    /// Applies the chosen Relics/Missions layout orientation to the live shell so the
    /// switch takes effect immediately (like font size). Wired by the shell.
    /// </summary>
    public Action<RelicsMissionsOrientation>? ApplyOrientation { get; set; }

    /// <summary>
    /// Applies the current effective (font-scaled) relic card width to the live shell,
    /// so the side-by-side splitter's minimum width stays correct. Wired by the shell.
    /// </summary>
    public Action<double>? ApplyRelicCardWidth { get; set; }

    /// <summary>
    /// Applies the chosen relic view mode to the live shell (main window) so the
    /// switch takes effect immediately, like orientation. Wired by the shell.
    /// </summary>
    public Action<RelicViewMode>? ApplyViewMode { get; set; }

    // ── Properties ─────────────────────────────────────────────────────────

    /// <summary>
    /// The effective font size in points. Always visible; drives the radio selection.
    /// Setting it auto-selects the matching preset radio, or Custom if no preset matches.
    /// </summary>
    public double EffectiveFontSizePt
    {
        get => fontSizePt;
        set
        {
            var pt = Math.Clamp(value, 8.0, 32.0);
            if (fontSizePt == pt) return;

            var prevOption = SelectedFontSize;
            fontSizePt = pt;

            OnPropertyChanged();
            if (SelectedFontSize != prevOption)
                OnPropertyChanged(nameof(SelectedFontSize));

            ApplyRelicCardWidth?.Invoke(ApplyFontSize(pt, relicCardWidthPx));
            _ = SaveFontSizeNow();
        }
    }

    /// <summary>
    /// The relic card's base width in pixels — its effective width at Medium font
    /// size. Font size continues to scale the rendered width proportionally from
    /// this base, exactly as it does from the historical fixed 300px value.
    /// Applies live and auto-saves, like <see cref="EffectiveFontSizePt"/>.
    /// </summary>
    public double RelicCardWidthPx
    {
        get => relicCardWidthPx;
        set
        {
            var px = Math.Clamp(value, 180.0, 600.0);
            if (relicCardWidthPx == px) return;

            relicCardWidthPx = px;
            OnPropertyChanged();

            ApplyRelicCardWidth?.Invoke(ApplyFontSize(fontSizePt, relicCardWidthPx));
            _ = SaveRelicCardWidthNow();
        }
    }

    /// <summary>
    /// Derived from <see cref="EffectiveFontSizePt"/>: Small/Medium/Large when the value
    /// matches a preset, Custom otherwise. Setting a preset updates <see cref="EffectiveFontSizePt"/>.
    /// </summary>
    public FontSizeOption SelectedFontSize
    {
        get => fontSizePt == SmallPt ? FontSizeOption.Small
             : fontSizePt == LargePt ? FontSizeOption.Large
             : fontSizePt == MediumPt ? FontSizeOption.Medium
             : FontSizeOption.Custom;
        set
        {
            var pt = value switch
            {
                FontSizeOption.Small => SmallPt,
                FontSizeOption.Large => LargePt,
                FontSizeOption.Custom => fontSizePt,   // clicking Custom is a no-op
                _ => MediumPt,
            };
            EffectiveFontSizePt = pt;
        }
    }

    /// <summary>
    /// The Relics/Missions layout orientation. Applies live to the shell and
    /// auto-saves on selection, like font size and relic card width.
    /// </summary>
    public RelicsMissionsOrientation SelectedOrientation
    {
        get => orientation;
        set
        {
            if (orientation == value) return;

            orientation = value;
            OnPropertyChanged();

            ApplyOrientation?.Invoke(value);
            _ = SaveOrientationNow();
        }
    }

    /// <summary>
    /// The active relic view mode. Applies live to the shell and auto-saves on
    /// selection, like font size, relic card width, and orientation.
    /// </summary>
    public RelicViewMode SelectedViewMode
    {
        get => viewMode;
        set
        {
            if (viewMode == value) return;

            viewMode = value;
            OnPropertyChanged();

            ApplyViewMode?.Invoke(value);
            _ = SaveViewModeNow();
        }
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private async Task SaveFontSizeNow()
    {
        var current = await settingsStore.LoadAsync();
        await settingsStore.SaveAsync(current with { FontSizePt = fontSizePt });
    }

    private async Task SaveOrientationNow()
    {
        var current = await settingsStore.LoadAsync();
        await settingsStore.SaveAsync(current with { Orientation = orientation });
    }

    private async Task SaveRelicCardWidthNow()
    {
        var current = await settingsStore.LoadAsync();
        await settingsStore.SaveAsync(current with { RelicCardWidthPx = relicCardWidthPx });
    }

    private async Task SaveViewModeNow()
    {
        var current = await settingsStore.LoadAsync();
        await settingsStore.SaveAsync(current with { ViewMode = viewMode });
    }

    /// <summary>
    /// Applies font size app-wide and recomputes <c>RelicCardWidth</c> from
    /// <paramref name="relicCardBasePx"/> — the card's effective width at Medium
    /// font size — scaled by the current font-size ratio. <c>MissionCardWidth</c>
    /// keeps its historical fixed 300px base (not user-configurable), and
    /// <c>SectionHeadingFontSize</c> its fixed 16pt base. Returns the
    /// resulting effective relic card width, so callers can propagate it live
    /// (e.g. to the side-by-side layout's minimum column width).
    /// </summary>
    internal static double ApplyFontSize(double pt, double relicCardBasePx)
    {
        if (AvaloniaApp.Current?.Resources is not { } resources) return relicCardBasePx;

        resources["ContentFontSize"] = pt;

        if (resources["FontSizeMedium"] is double medium && medium > 0)
        {
            var ratio = pt / medium;
            var relicCardWidth = relicCardBasePx * ratio;
            resources["RelicCardWidth"] = relicCardWidth;
            resources["MissionCardWidth"] = 300 * ratio;
            resources["SectionHeadingFontSize"] = 16 * ratio;
            return relicCardWidth;
        }

        return relicCardBasePx;
    }
}
