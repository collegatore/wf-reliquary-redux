namespace WF.Reliquary.Application.DropTables;

public sealed record DropTableSourceContent(Uri SourceUrl, string Html, DateTimeOffset DownloadedAt, string LastUpdate);
