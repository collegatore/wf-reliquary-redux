using WF.Reliquary.Application.Ports;

namespace WF.Reliquary.Application.DropTables;

public sealed class DropTableSourceRefreshService : IDropTableSourceRefreshWorkflow
{
    private readonly IDropTableSourceClient sourceClient;
    private readonly IDropTableSourceStore sourceStore;
    private readonly IDropTableMetadataReader metadataReader;
    private readonly IDropTableSourceSettingsStore settingsStore;

    public DropTableSourceRefreshService(
        IDropTableSourceClient sourceClient,
        IDropTableSourceStore sourceStore,
        IDropTableMetadataReader metadataReader,
        IDropTableSourceSettingsStore settingsStore)
    {
        this.sourceClient = sourceClient;
        this.sourceStore = sourceStore;
        this.metadataReader = metadataReader;
        this.settingsStore = settingsStore;
    }

    public async Task<DropTableSourceStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var stored = await sourceStore.LoadAsync(cancellationToken);

        if (stored is null)
        {
            return new DropTableSourceStatus(false, null, null);
        }

        return new DropTableSourceStatus(
            true,
            stored.Provenance.DownloadedAt,
            stored.Provenance.LastUpdate);
    }

    public async Task<DropTableRefreshResult> RefreshAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var settings = await settingsStore.LoadAsync(cancellationToken);
            var download = await sourceClient.DownloadAsync(settings.SourceUrl, cancellationToken);
            var lastUpdate = metadataReader.ReadLastUpdate(download.Html);

            if (string.IsNullOrWhiteSpace(lastUpdate))
            {
                throw new InvalidOperationException("Downloaded drop-table file does not contain a Drop Tables version value.");
            }

            await sourceStore.SaveAsync(
                new DropTableSourceContent(download.SourceUrl, download.Html, download.DownloadedAt, lastUpdate),
                cancellationToken);

            var status = await GetStatusAsync(cancellationToken);
            return DropTableRefreshResult.Success(status);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var status = await GetStatusAsync(CancellationToken.None);
            return DropTableRefreshResult.Failure(status, ex.Message);
        }
    }

    public Task<bool> HasStoredSourcesAsync(CancellationToken cancellationToken = default)
    {
        return sourceStore.HasAnyAsync(cancellationToken);
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
