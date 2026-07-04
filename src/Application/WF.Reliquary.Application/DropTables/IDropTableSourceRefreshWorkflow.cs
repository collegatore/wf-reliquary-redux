namespace WF.Reliquary.Application.DropTables;

public interface IDropTableSourceRefreshWorkflow
{
    Task<DropTableSourceStatus> GetStatusAsync(CancellationToken cancellationToken = default);

    Task<DropTableRefreshResult> RefreshAsync(CancellationToken cancellationToken = default);

    Task<bool> HasStoredSourcesAsync(CancellationToken cancellationToken = default);

    Task<DropTableRefreshResult> ConfirmSaveAsync(DropTableSourceContent pending, long existingId, CancellationToken cancellationToken = default);
}
