namespace WF.Reliquary.Application.DropTables;

public sealed record DropTableImportResult(
    bool Succeeded,
    string? SourcePath,
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
            import.SourcePath,
            import.SourceLastUpdate,
            import.ImportedAt,
            import.Counts,
            import.Warnings,
            null);
    }

    public static DropTableImportResult Failure(
        string errorMessage,
        string? sourcePath = null,
        string? sourceLastUpdate = null,
        IReadOnlyList<string>? warnings = null)
    {
        return new DropTableImportResult(
            false,
            sourcePath,
            sourceLastUpdate,
            null,
            new DropTableImportCounts(0, 0, 0, 0, 0, 0),
            warnings ?? [],
            errorMessage);
    }
}

