namespace WF.Reliquary.Application.DropTables;

public sealed record DropTableDownload(Uri SourceUrl, string Html, DateTimeOffset DownloadedAt);
