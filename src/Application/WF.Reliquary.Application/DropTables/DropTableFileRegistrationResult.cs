namespace WF.Reliquary.Application.DropTables;

public sealed record DropTableFileRegistrationResult(bool Succeeded, string? ErrorMessage, string? LastUpdate = null);
