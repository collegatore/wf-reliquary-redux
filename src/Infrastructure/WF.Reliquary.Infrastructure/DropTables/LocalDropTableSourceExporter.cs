using Microsoft.EntityFrameworkCore;
using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Infrastructure.Storage;

namespace WF.Reliquary.Infrastructure.DropTables;

public sealed class LocalDropTableSourceExporter : IDropTableSourceExporter
{
    private readonly ReliquaryDbExecutor executor;

    public LocalDropTableSourceExporter(string? databasePath = null)
    {
        executor = new ReliquaryDbExecutor(ReliquaryDbContext.BuildOptions(
            databasePath ?? LocalDropTableSourceStore.DefaultDatabasePath()));
    }

    public async Task<DropTableExportResult> ExportAllAsync(
        string destinationFolderPath,
        CancellationToken cancellationToken = default)
    {
        var entities = await executor.RunAsync(
            (context, ct) => context.SourceFiles
                .AsNoTracking()
                .OrderBy(e => e.Id)
                .ToListAsync(ct),
            cancellationToken);

        if (entities.Count == 0)
        {
            return new DropTableExportResult(true, 0, null);
        }

        Directory.CreateDirectory(destinationFolderPath);
        int count = 0;

        foreach (var entity in entities)
        {
            var suffix = !string.IsNullOrWhiteSpace(entity.SourceLastUpdate)
                ? SanitizeForFileName(entity.SourceLastUpdate)
                : DateTimeOffset
                    .Parse(entity.DownloadedAt, null, System.Globalization.DateTimeStyles.RoundtripKind)
                    .ToString("yyyyMMddTHHmmss");

            var fileName = $"warframe-drop-tables-{suffix}.html";
            var filePath = Path.Combine(destinationFolderPath, fileName);

            try
            {
                await File.WriteAllTextAsync(filePath, entity.Content, cancellationToken);
                count++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                return new DropTableExportResult(false, count, $"Failed to write '{fileName}': {ex.Message}");
            }
        }

        return new DropTableExportResult(true, count, null);
    }

    private static string SanitizeForFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = value.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            if (Array.IndexOf(invalid, chars[i]) >= 0)
                chars[i] = '-';
        }
        return new string(chars);
    }
}
