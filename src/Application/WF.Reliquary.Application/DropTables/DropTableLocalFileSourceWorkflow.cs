using WF.Reliquary.Application.Ports;

namespace WF.Reliquary.Application.DropTables;

public sealed class DropTableLocalFileSourceWorkflow : IDropTableLocalFileSourceWorkflow
{
    private readonly IDropTableSourceStore sourceStore;
    private readonly IDropTableMetadataReader metadataReader;
    private readonly Func<DateTimeOffset> now;

    public DropTableLocalFileSourceWorkflow(
        IDropTableSourceStore sourceStore,
        IDropTableMetadataReader metadataReader,
        Func<DateTimeOffset>? now = null)
    {
        this.sourceStore = sourceStore;
        this.metadataReader = metadataReader;
        this.now = now ?? (() => DateTimeOffset.UtcNow);
    }

    public async Task<DropTableFileRegistrationResult> RegisterFromFileAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        string html;

        try
        {
            html = await File.ReadAllTextAsync(filePath, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new DropTableFileRegistrationResult(false, $"Unable to read file: {ex.Message}");
        }

        var lastUpdate = metadataReader.ReadLastUpdate(html);

        if (string.IsNullOrWhiteSpace(lastUpdate))
        {
            return new DropTableFileRegistrationResult(false, "File does not contain a Drop Tables version value.");
        }

        var content = new DropTableSourceContent(new Uri(filePath), html, now(), lastUpdate);

        var existing = await sourceStore.FindSummaryByLastUpdateAsync(lastUpdate, cancellationToken);
        if (existing is not null)
        {
            return DropTableFileRegistrationResult.DuplicateVersion(content, existing.Id);
        }

        await sourceStore.SaveAsync(content, cancellationToken);

        return new DropTableFileRegistrationResult(true, null, lastUpdate);
    }

    public async Task<DropTableFileRegistrationResult> ConfirmSaveAsync(
        DropTableSourceContent pending,
        long existingId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await sourceStore.ReplaceAsync(existingId, pending, cancellationToken);
            return new DropTableFileRegistrationResult(true, null, pending.LastUpdate);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new DropTableFileRegistrationResult(false, ex.Message);
        }
    }
}
