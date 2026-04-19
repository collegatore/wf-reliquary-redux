namespace WF.Reliquary.Application.DropTables;

public interface IDropTableSourceRefreshWorkflow
{
    Task<DropTableSourceStatus> GetStatusAsync(CancellationToken cancellationToken = default);

    Task<DropTableRefreshResult> RefreshAsync(CancellationToken cancellationToken = default);

    Task<DropTableSourceSettings> GetSettingsAsync(CancellationToken cancellationToken = default);

    Task SaveSettingsAsync(DropTableSourceSettings settings, CancellationToken cancellationToken = default);
}
