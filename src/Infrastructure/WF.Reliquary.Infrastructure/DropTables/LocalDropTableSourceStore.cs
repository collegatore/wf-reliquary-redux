using System.Text.Json;
using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Application.Ports;

namespace WF.Reliquary.Infrastructure.DropTables;

public sealed class LocalDropTableSourceStore : IDropTableSourceStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly string rootPath;

    public LocalDropTableSourceStore(string? rootPath = null)
    {
        this.rootPath = rootPath ?? DefaultRootPath();
    }

    public static string DefaultRootPath()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        if (string.IsNullOrWhiteSpace(localAppData))
        {
            localAppData = AppContext.BaseDirectory;
        }

        return Path.Combine(localAppData, "WF Reliquary Redux", "DropTables");
    }

    public async Task<StoredDropTableSource?> LoadAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(rootPath, fileName);

        if (!File.Exists(filePath))
        {
            return null;
        }

        var html = await File.ReadAllTextAsync(filePath, cancellationToken);
        var provenance = await LoadProvenanceAsync(fileName, cancellationToken);

        return new StoredDropTableSource(filePath, html, provenance);
    }

    public async Task SaveAsync(DropTableSourceContent content, string fileName, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(rootPath);

        var filePath = Path.Combine(rootPath, fileName);
        await File.WriteAllTextAsync(filePath, content.Html, cancellationToken);

        if (string.IsNullOrWhiteSpace(content.LastUpdate))
        {
            throw new InvalidOperationException("Unable to detect Last Update in the downloaded file. Please ensure that provided download URL exposes an official drop tables file.");
        }

        var provenance = new DropTableSourceProvenance(content.SourceUrl, content.DownloadedAt, content.LastUpdate);
        var metadataJson = JsonSerializer.Serialize(provenance, SerializerOptions);
        await File.WriteAllTextAsync(MetadataPath(fileName), metadataJson, cancellationToken);
    }

    private async Task<DropTableSourceProvenance?> LoadProvenanceAsync(string fileName, CancellationToken cancellationToken)
    {
        var metadataPath = MetadataPath(fileName);

        if (!File.Exists(metadataPath))
        {
            return null;
        }

        var json = await File.ReadAllTextAsync(metadataPath, cancellationToken);
        return JsonSerializer.Deserialize<DropTableSourceProvenance>(json, SerializerOptions);
    }

    private string MetadataPath(string fileName)
    {
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        return Path.Combine(rootPath, $"{nameWithoutExtension}.metadata.json");
    }
}
