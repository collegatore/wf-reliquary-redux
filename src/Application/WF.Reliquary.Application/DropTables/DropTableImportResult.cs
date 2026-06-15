namespace WF.Reliquary.Application.DropTables;

public sealed record DropTableImportResult(
    bool Succeeded,
    string? SourceLastUpdate,
    DateTimeOffset? ImportedAt,
    DropTableImportCounts Counts,
    IReadOnlyList<string> Warnings,
    string? ErrorMessage)
{
    public static DropTableImportResult Success(ParsedDropTableImport import)
    {
        return new DropTableImportResult(
            true,
            import.SourceLastUpdate,
            import.ImportedAt,
            import.Counts,
            import.Warnings,
            null);
    }

    public static DropTableImportResult Failure(
        string errorMessage,
        string? sourceLastUpdate = null,
        IReadOnlyList<string>? warnings = null)
    {
        return new DropTableImportResult(
            false,
            sourceLastUpdate,
            null,
            new DropTableImportCounts(0, 0, 0, 0, 0, 0),
            warnings ?? [],
            errorMessage);
    }
}

