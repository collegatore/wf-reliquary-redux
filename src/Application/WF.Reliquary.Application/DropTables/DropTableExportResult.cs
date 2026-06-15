namespace WF.Reliquary.Application.DropTables;

public sealed record DropTableExportResult(bool Succeeded, int FileCount, string? ErrorMessage);
