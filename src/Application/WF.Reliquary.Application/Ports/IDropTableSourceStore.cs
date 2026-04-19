using WF.Reliquary.Application.DropTables;

namespace WF.Reliquary.Application.Ports;

public interface IDropTableSourceStore
{
    Task<StoredDropTableSource?> LoadAsync(string fileName, CancellationToken cancellationToken = default);

    Task SaveAsync(DropTableSourceContent content, string fileName, CancellationToken cancellationToken = default);
}
