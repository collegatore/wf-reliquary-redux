using Avalonia.Markup.Xaml.Styling;
using WF.Reliquary.Application.Settings;
using AvaloniaApp = Avalonia.Application;

namespace WF.Reliquary.Presentation;

/// <summary>
/// Switches the app's color palette by replacing the single palette dictionary merged
/// into App.axaml's resources (Styles/Palettes/*.axaml). Every consumer of the palette
/// keys uses DynamicResource, so the swap recolors the running UI immediately.
/// </summary>
public static class PaletteApplier
{
    private const string AssemblyRoot = "avares://WF.Reliquary.Redux/";

    /// <summary>Key every palette file defines; identifies the palette dictionary however it was loaded.</summary>
    private const string MarkerKey = "PaletteName";

    public static void Apply(ColorPalette palette)
    {
        if (AvaloniaApp.Current?.Resources is not { } resources) return;

        var merged = resources.MergedDictionaries;
        // The startup palette may be compiled into a plain dictionary rather than kept as
        // a ResourceInclude, so find it by its marker key, not by its type or source.
        var index = merged.ToList().FindIndex(d => d.TryGetResource(MarkerKey, null, out _));
        var include = new ResourceInclude(new Uri(AssemblyRoot))
        {
            Source = new Uri($"{AssemblyRoot}Styles/Palettes/{palette}.axaml"),
        };

        if (index >= 0) merged[index] = include;
        else merged.Add(include);
    }
}
