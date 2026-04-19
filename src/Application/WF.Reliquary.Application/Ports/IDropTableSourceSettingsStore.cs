using WF.Reliquary.Application.DropTables;

namespace WF.Reliquary.Application.Ports;

public interface IDropTableSourceSettingsStore
{
    Task<DropTableSourceSettings> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(DropTableSourceSettings settings, CancellationToken cancellationToken = default);
}
