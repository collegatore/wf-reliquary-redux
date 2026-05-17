namespace WF.Reliquary.Application.DropTables;

public interface IDropTableImportWorkflow
{
    Task<DropTableImportResult> ImportAsync(CancellationToken cancellationToken = default);
}

