using WF.Reliquary.Application.DropTables;

namespace WF.Reliquary.Application.Ports;

public interface IDropTableSourceClient
{
    Task<DropTableDownload> DownloadAsync(Uri sourceUrl, CancellationToken cancellationToken = default);
}
