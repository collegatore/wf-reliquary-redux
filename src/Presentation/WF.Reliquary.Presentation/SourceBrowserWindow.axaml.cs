using Avalonia.Controls;
using WF.Reliquary.Application.DropTables;

namespace WF.Reliquary.Presentation;

public sealed partial class SourceBrowserWindow : Window
{
    public SourceBrowserWindow()
    {
        InitializeComponent();
    }

    public static Task ShowAsync(Window owner, IReadOnlyList<DropTableSourceSummary> sources)
    {
        var window = new SourceBrowserWindow();
        window.SourcesList.ItemsSource = sources.Select(s => new SourceBrowserRow(s)).ToList();
        return window.ShowDialog(owner);
    }
}

internal sealed class SourceBrowserRow
{
    public SourceBrowserRow(DropTableSourceSummary summary)
    {
        LastUpdate = string.IsNullOrWhiteSpace(summary.LastUpdate)
            ? "(version unknown)"
            : summary.LastUpdate;
        SourceUrl = FormatSource(summary.SourceUrl);
        DownloadedAtFormatted = summary.DownloadedAt.LocalDateTime.ToString("g");
    }

    public string LastUpdate { get; }
    public string SourceUrl { get; }
    public string DownloadedAtFormatted { get; }

    private static string FormatSource(string sourceUrl)
    {
        if (Uri.TryCreate(sourceUrl, UriKind.Absolute, out var uri) && uri.IsFile)
        {
            return Path.GetFileName(uri.LocalPath);
        }
        return sourceUrl;
    }
}
