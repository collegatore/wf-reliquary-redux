using WF.Reliquary.Application.DropTables;

namespace WF.Reliquary.Application.Ports;

public interface IDropTableSourceClient
{
    Task<DropTableSourceContent> DownloadAsync(Uri sourceUrl, CancellationToken cancellationToken = default);
}
