using WF.Reliquary.Application.Ports;

namespace WF.Reliquary.Application.DropTables;

public sealed class DropTableSourceRefreshService : IDropTableSourceRefreshWorkflow
{
    private readonly DropTableSourceOptions options;
    private readonly IDropTableSourceClient sourceClient;
    private readonly IDropTableSourceStore sourceStore;
    private readonly IDropTableMetadataReader metadataReader;
    private readonly IDropTableSourceSettingsStore settingsStore;

    public DropTableSourceRefreshService(
        DropTableSourceOptions options,
        IDropTableSourceClient sourceClient,
        IDropTableSourceStore sourceStore,
        IDropTableMetadataReader metadataReader,
        IDropTableSourceSettingsStore settingsStore)
    {
        this.options = options;
        this.sourceClient = sourceClient;
        this.sourceStore = sourceStore;
        this.metadataReader = metadataReader;
        this.settingsStore = settingsStore;
    }

    public async Task<DropTableSourceStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var settings = await settingsStore.LoadAsync(cancellationToken);
        var stored = await sourceStore.LoadAsync(options.FileName, cancellationToken);

        if (stored is null)
        {
            return new DropTableSourceStatus(false, settings.SourceUrl, null, null, null);
        }

        var sourceUrl = stored.Provenance?.SourceUrl ?? settings.SourceUrl;

        return new DropTableSourceStatus(
            true,
            sourceUrl,
            stored.FilePath,
            stored.Provenance?.DownloadedAt,
            stored.Provenance?.LastUpdate);
    }

    public async Task<DropTableRefreshResult> RefreshAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var settings = await settingsStore.LoadAsync(cancellationToken);
            var content = await sourceClient.DownloadAsync(settings.SourceUrl, cancellationToken);
            var lastUpdate = metadataReader.ReadLastUpdate(content.Html);

            if (string.IsNullOrWhiteSpace(lastUpdate))
            {
                throw new InvalidOperationException("Downloaded drop-table file does not contain a Last Update value.");
            }

            await sourceStore.SaveAsync(content with { LastUpdate = lastUpdate }, options.FileName, cancellationToken);

            var status = await GetStatusAsync(cancellationToken);
            return DropTableRefreshResult.Success(status);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var status = await GetStatusAsync(CancellationToken.None);
            return DropTableRefreshResult.Failure(status, ex.Message);
        }
    }

    public Task<DropTableSourceSettings> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        return settingsStore.LoadAsync(cancellationToken);
    }

    public Task SaveSettingsAsync(DropTableSourceSettings settings, CancellationToken cancellationToken = default)
    {
        return settingsStore.SaveAsync(settings, cancellationToken);
    }
}
