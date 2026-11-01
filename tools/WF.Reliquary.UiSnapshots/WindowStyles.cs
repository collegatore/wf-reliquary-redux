using System.Text.RegularExpressions;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace WF.Reliquary.UiSnapshots;

/// <summary>
/// The card styles (gear-card outline, selected wash, group-mode overlay, tier bars...)
/// live in MainWindow.axaml's <c>Window.Styles</c>, not in App.axaml, so a view hosted in
/// a plain window would render without them. MainWindow itself can't be built here (its
/// constructors start services), so its <c>Window.Styles</c> block is read from source and
/// parsed at runtime, once per scene window (a <see cref="Styles"/> can have only one owner).
/// </summary>
internal static partial class WindowStyles
{
    private const string MainWindowPath = "src/Presentation/WF.Reliquary.Presentation/MainWindow.axaml";

    /// <summary>Returns a factory producing a fresh copy of MainWindow's styles, or null if not found.</summary>
    public static Func<Styles>? LoadMainWindowStyles()
    {
        var root = FindRepoRoot();
        if (root is null) return null;

        var xaml = File.ReadAllText(Path.Combine(root, MainWindowPath));
        var block = StylesBlock().Match(xaml);
        if (!block.Success) return null;

        var styles = $"""
            <Styles xmlns="https://github.com/avaloniaui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:local="clr-namespace:WF.Reliquary.Presentation;assembly=WF.Reliquary.Redux">
            {block.Groups[1].Value}
            </Styles>
            """;
        return () => AvaloniaRuntimeXamlLoader.Parse<Styles>(styles, typeof(WindowStyles).Assembly);
    }

    private static string? FindRepoRoot()
    {
        for (var dir = new DirectoryInfo(Directory.GetCurrentDirectory()); dir is not null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "WF.Reliquary.Redux.sln"))) return dir.FullName;
        }
        return null;
    }

    [GeneratedRegex(@"<Window\.Styles>(.*?)</Window\.Styles>", RegexOptions.Singleline)]
    private static partial Regex StylesBlock();
}
