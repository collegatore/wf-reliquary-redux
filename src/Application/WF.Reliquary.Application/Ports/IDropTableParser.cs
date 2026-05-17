using WF.Reliquary.Application.DropTables;

namespace WF.Reliquary.Application.Ports;

public interface IDropTableParser
{
    ParsedDropTableImport Parse(StoredDropTableSource source, DateTimeOffset importedAt);
}

