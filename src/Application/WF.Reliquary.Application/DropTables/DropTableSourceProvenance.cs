namespace WF.Reliquary.Application.DropTables;

public sealed record DropTableSourceProvenance(Uri SourceUrl, DateTimeOffset DownloadedAt, string LastUpdate);
