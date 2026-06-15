namespace WF.Reliquary.Application.DropTables;

public sealed record DropTableSourceStatus(
    bool FileExists,
    DateTimeOffset? DownloadedAt,
    string? LastUpdate)
{
    public static DropTableSourceStatus Missing()
    {
        return new DropTableSourceStatus(false, null, null);
    }
}
