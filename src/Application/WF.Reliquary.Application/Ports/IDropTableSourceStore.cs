using WF.Reliquary.Application.DropTables;

namespace WF.Reliquary.Application.Ports;

public interface IDropTableSourceStore
{
    Task<StoredDropTableSource?> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(DropTableSourceContent content, CancellationToken cancellationToken = default);

    Task<bool> HasAnyAsync(CancellationToken cancellationToken = default);
}
