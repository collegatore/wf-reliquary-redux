using WF.Reliquary.Application.DropTables;

namespace WF.Reliquary.Application.Ports;

public interface IDropTableSourceStore
{
    Task<DropTableSourceContent?> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(DropTableSourceContent content, CancellationToken cancellationToken = default);

    Task<bool> HasAnyAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DropTableSourceSummary>> ListSummariesAsync(CancellationToken cancellationToken = default);

    Task<DropTableSourceSummary?> FindSummaryByLastUpdateAsync(string lastUpdate, CancellationToken cancellationToken = default);

    Task<DropTableSourceContent?> LoadByIdAsync(long id, CancellationToken cancellationToken = default);

    Task ReplaceAsync(long existingId, DropTableSourceContent newContent, CancellationToken cancellationToken = default);
}
