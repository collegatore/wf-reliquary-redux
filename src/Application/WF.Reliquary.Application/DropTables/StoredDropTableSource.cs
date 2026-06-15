namespace WF.Reliquary.Application.DropTables;

public sealed record StoredDropTableSource(
    string Html,
    DropTableSourceProvenance Provenance);
