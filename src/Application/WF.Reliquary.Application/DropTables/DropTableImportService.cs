using WF.Reliquary.Application.Ports;

namespace WF.Reliquary.Application.DropTables;

public sealed class DropTableImportService : IDropTableImportWorkflow
{
    private readonly IDropTableSourceStore sourceStore;
    private readonly IDropTableParser parser;
    private readonly IDropTableImportStore importStore;
    private readonly Func<DateTimeOffset> now;

    public DropTableImportService(
        IDropTableSourceStore sourceStore,
        IDropTableParser parser,
        IDropTableImportStore importStore,
        Func<DateTimeOffset>? now = null)
    {
        this.sourceStore = sourceStore;
        this.parser = parser;
        this.importStore = importStore;
        this.now = now ?? (() => DateTimeOffset.UtcNow);
    }

    public async Task<DropTableImportResult> ImportAsync(CancellationToken cancellationToken = default)
    {
        DropTableSourceContent? stored;

        try
        {
            stored = await sourceStore.LoadAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return DropTableImportResult.Failure($"Unable to load stored drop-table source: {ex.Message}");
        }

        if (stored is null)
        {
            return DropTableImportResult.Failure("Drop-table source file must be downloaded before import can run.");
        }

        ParsedDropTableImport parsed;

        try
        {
            parsed = await parser.ParseAsync(stored, now(), cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return DropTableImportResult.Failure(ex.Message, stored.LastUpdate);
        }

        try
        {
            await importStore.ReplaceAsync(parsed, cancellationToken);
            return DropTableImportResult.Success(parsed);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return DropTableImportResult.Failure(
                $"Unable to persist parsed drop-table import: {ex.Message}",
                parsed.SourceLastUpdate,
                parsed.Warnings);
        }
    }
}
