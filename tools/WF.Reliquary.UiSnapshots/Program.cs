using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using WF.Reliquary.Presentation;
using WF.Reliquary.UiSnapshots;

// Renders the real Presentation views off-screen and saves one PNG per scene.
//
//   dotnet run -c Release --project tools/WF.Reliquary.UiSnapshots -- [options]
//
//   --out <dir>      output folder (default: artifacts/ui-snapshots under the current directory)
//   --only <text>    render only scenes whose name contains <text>
//   --set <k>=<v>    override an app resource for this run (repeatable); a color value
//                    (#RRGGBB / #AARRGGBB) becomes a SolidColorBrush, a number a double.
//                    Handy for trying palettes: --set ControlHoverBrush=#E3EEE5
//   --list           print scene names and exit
//
// The real App is used, so App.axaml's themes, styles and resources all load; with no
// desktop lifetime, App never creates MainWindow or any services.

var outDir = Path.Combine("artifacts", "ui-snapshots");
string? only = null;
var overrides = new List<(string Key, string Value)>();
for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--out" when i + 1 < args.Length: outDir = args[++i]; break;
        case "--only" when i + 1 < args.Length: only = args[++i]; break;
        case "--set" when i + 1 < args.Length && args[i + 1].Contains('='):
            var pair = args[++i].Split('=', 2);
            overrides.Add((pair[0], pair[1]));
            break;
        case "--list":
            foreach (var s in Scenes.All) Console.WriteLine(s.Name);
            return 0;
        default:
            Console.Error.WriteLine($"Unknown argument: {args[i]}");
            return 2;
    }
}

var scenes = Scenes.All
    .Where(s => only is null || s.Name.Contains(only, StringComparison.OrdinalIgnoreCase))
    .ToList();
if (scenes.Count == 0)
{
    Console.Error.WriteLine($"No scene matches '{only}'. Use --list to see the scenes.");
    return 1;
}

AppBuilder.Configure<App>()
    .UseSkia()
    // Real Skia drawing; the default headless drawing renders nothing.
    .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
    .WithInterFont()
    .SetupWithoutStarting();

// Application.Resources is searched before the resources of Application.Styles, so an
// override here wins over e.g. the brushes declared in Styles/ControlBars.axaml.
foreach (var (key, value) in overrides)
{
    Application.Current!.Resources[key] = Color.TryParse(value, out var color)
        ? new SolidColorBrush(color)
        : double.Parse(value, CultureInfo.InvariantCulture);
}

var windowStyles = WindowStyles.LoadMainWindowStyles();
if (windowStyles is null)
    Console.Error.WriteLine("MainWindow styles not found (run from inside the repo); card styles will be missing.");

Directory.CreateDirectory(outDir);
var failed = 0;
foreach (var scene in scenes)
{
    ApplyFontSize(scene.FontSize);
    PaletteApplier.Apply(scene.Palette);
    var path = Path.GetFullPath(Path.Combine(outDir, scene.Name + ".png"));
    if (Render(scene, path, windowStyles)) Console.WriteLine(path);
    else { Console.Error.WriteLine($"{scene.Name}: no frame rendered"); failed++; }
}
return failed == 0 ? 0 : 1;

static bool Render(Scene scene, string path, Func<Styles>? windowStyles)
{
    var window = new Window { Width = scene.Width, Height = scene.Height, Content = scene.Build() };
    if (windowStyles is not null) window.Styles.Add(windowStyles());
    window.Show();
    Dispatcher.UIThread.RunJobs();
    if (scene.Hover is { } point)
    {
        window.MouseMove(point);
        Dispatcher.UIThread.RunJobs();
    }
    AvaloniaHeadlessPlatform.ForceRenderTimerTick();
    using var frame = window.CaptureRenderedFrame();
    frame?.Save(path);
    window.Close();
    return frame is not null;
}

// Mirrors the font-dependent resources AppearanceViewModel.ApplyFontSize writes (it is
// internal to Presentation), so scenes can be rendered at Small/Medium/Large.
static void ApplyFontSize(double pt)
{
    var resources = Application.Current!.Resources;
    var ratio = pt / Scenes.MediumFontSize;
    resources["ContentFontSize"] = pt;
    resources["SectionHeadingFontSize"] = 16 * ratio;
    resources["RelicCardWidth"] = 300 * ratio;
    resources["MissionCardWidth"] = 300 * ratio;
}
