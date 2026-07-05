namespace WF.Reliquary.Application.DropTables;

public sealed record DropTableRefreshResult(
    bool Succeeded,
    DropTableSourceStatus Status,
    string? ErrorMessage,
    DropTableSourceContent? Pending = null,
    long? ExistingId = null,
    long? Id = null)
{
    public bool IsDuplicateVersion => Pending is not null;

    public static DropTableRefreshResult Success(DropTableSourceStatus status, long? id = null)
    {
        return new DropTableRefreshResult(true, status, null, Id: id);
    }

    public static DropTableRefreshResult Failure(DropTableSourceStatus status, string errorMessage)
    {
        return new DropTableRefreshResult(false, status, errorMessage);
    }

    public static DropTableRefreshResult DuplicateVersion(DropTableSourceContent pending, long existingId)
    {
        return new DropTableRefreshResult(false, DropTableSourceStatus.Missing(), null, pending, existingId);
    }
}
