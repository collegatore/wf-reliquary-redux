namespace WF.Reliquary.Application.DropTables;

public sealed record DropTableSourceStatus(
    bool FileExists,
    Uri SourceUrl,
    string? FilePath,
    DateTimeOffset? DownloadedAt,
    string? LastUpdate)
{
    public static DropTableSourceStatus Missing(DropTableSourceOptions options)
    {
        return new DropTableSourceStatus(false, options.SourceUrl, null, null, null);
    }
}
