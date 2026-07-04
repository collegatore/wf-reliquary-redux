namespace WF.Reliquary.Application.DropTables;

public interface IDropTableImportWorkflow
{
    Task<DropTableImportResult> ImportAsync(long sourceId, CancellationToken cancellationToken = default);
}

