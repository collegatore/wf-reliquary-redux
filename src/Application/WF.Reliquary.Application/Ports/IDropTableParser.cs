using WF.Reliquary.Application.DropTables;

namespace WF.Reliquary.Application.Ports;

public interface IDropTableParser
{
    /// <summary>
    /// Parses the stored HTML source into the import model asynchronously.
    /// </summary>
    Task<ParsedDropTableImport> ParseAsync(
        StoredDropTableSource source,
        DateTimeOffset importedAt,
        CancellationToken cancellationToken = default);
}

