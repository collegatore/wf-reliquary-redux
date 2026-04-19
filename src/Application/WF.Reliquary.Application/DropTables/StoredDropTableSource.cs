namespace WF.Reliquary.Application.DropTables;

public sealed record StoredDropTableSource(
    string FilePath,
    string Html,
    DropTableSourceProvenance? Provenance);
