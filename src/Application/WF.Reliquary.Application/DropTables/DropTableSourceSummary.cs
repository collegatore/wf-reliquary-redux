namespace WF.Reliquary.Application.DropTables;

public sealed record DropTableSourceSummary(long Id, string SourceUrl, DateTimeOffset DownloadedAt, string LastUpdate);
