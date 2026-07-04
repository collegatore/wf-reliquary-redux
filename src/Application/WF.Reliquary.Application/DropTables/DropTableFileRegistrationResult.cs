namespace WF.Reliquary.Application.DropTables;

public sealed record DropTableFileRegistrationResult(
    bool Succeeded,
    string? ErrorMessage,
    string? LastUpdate = null,
    DropTableSourceContent? Pending = null,
    long? ExistingId = null)
{
    public bool IsDuplicateVersion => Pending is not null;

    public static DropTableFileRegistrationResult DuplicateVersion(DropTableSourceContent pending, long existingId)
    {
        return new DropTableFileRegistrationResult(false, null, null, pending, existingId);
    }
}
