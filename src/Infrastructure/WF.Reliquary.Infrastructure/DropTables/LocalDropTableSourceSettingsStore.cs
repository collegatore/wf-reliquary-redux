using System.Text.Json;
using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Application.Ports;

namespace WF.Reliquary.Infrastructure.DropTables;

public sealed class LocalDropTableSourceSettingsStore : IDropTableSourceSettingsStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly string rootPath;

    public LocalDropTableSourceSettingsStore(string? rootPath = null)
    {
        this.rootPath = rootPath ?? LocalDropTableSourceStore.DefaultRootPath();
    }

    public async Task<DropTableSourceSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        var path = SettingsPath();

        if (!File.Exists(path))
        {
            return new DropTableSourceSettings(new Uri(DropTableSourceDefaults.SourceUrl));
        }

        var json = await File.ReadAllTextAsync(path, cancellationToken);
        var settings = JsonSerializer.Deserialize<SettingsFile>(json, SerializerOptions);

        if (settings is not null && Uri.TryCreate(settings.SourceUrl, UriKind.Absolute, out var sourceUrl))
        {
            return new DropTableSourceSettings(sourceUrl);
        }

        return new DropTableSourceSettings(new Uri(DropTableSourceDefaults.SourceUrl));
    }

    public async Task SaveAsync(DropTableSourceSettings settings, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(rootPath);
        var settingsFile = new SettingsFile(settings.SourceUrl.ToString());
        var json = JsonSerializer.Serialize(settingsFile, SerializerOptions);
        await File.WriteAllTextAsync(SettingsPath(), json, cancellationToken);
    }

    private string SettingsPath()
    {
        return Path.Combine(rootPath, "drop-table-source.settings.json");
    }

    private sealed record SettingsFile(string SourceUrl);
}
