namespace WF.Reliquary.Application.DropTables;

public sealed record DropTableRefreshResult(
    bool Succeeded,
    DropTableSourceStatus Status,
    string? ErrorMessage)
{
    public static DropTableRefreshResult Success(DropTableSourceStatus status)
    {
        return new DropTableRefreshResult(true, status, null);
    }

    public static DropTableRefreshResult Failure(DropTableSourceStatus status, string errorMessage)
    {
        return new DropTableRefreshResult(false, status, errorMessage);
    }
}
