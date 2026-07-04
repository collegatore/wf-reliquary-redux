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
            stored.DownloadedAt,
            stored.LastUpdate);
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

            var pending = new DropTableSourceContent(download.SourceUrl, download.Html, download.DownloadedAt, lastUpdate);

            var existing = await sourceStore.FindSummaryByLastUpdateAsync(lastUpdate, cancellationToken);
            if (existing is not null)
            {
                return DropTableRefreshResult.DuplicateVersion(pending, existing.Id);
            }

            await sourceStore.SaveAsync(pending, cancellationToken);

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

    public async Task<DropTableRefreshResult> ConfirmSaveAsync(
        DropTableSourceContent pending,
        long existingId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await sourceStore.ReplaceAsync(existingId, pending, cancellationToken);
            var status = await GetStatusAsync(cancellationToken);
            return DropTableRefreshResult.Success(status);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var status = await GetStatusAsync(CancellationToken.None);
            return DropTableRefreshResult.Failure(status, ex.Message);
        }
    }
}
