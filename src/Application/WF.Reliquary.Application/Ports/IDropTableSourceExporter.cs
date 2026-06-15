using WF.Reliquary.Application.DropTables;

namespace WF.Reliquary.Application.Ports;

public interface IDropTableSourceExporter
{
    Task<DropTableExportResult> ExportAllAsync(string destinationFolderPath, CancellationToken cancellationToken = default);
}
