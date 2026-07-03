using System.Text;
using WF.Reliquary.Application;
using WF.Reliquary.Application.Ports;

namespace WF.Reliquary.Infrastructure.Logging;

public sealed class FileErrorLog : IErrorLog
{
    private readonly SemaphoreSlim semaphore = new(1, 1);

    public FileErrorLog(string? logPath = null)
    {
        LogFilePath = logPath ?? DefaultLogPath();
    }

    public string LogFilePath { get; }

    public static string DefaultLogPath()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(localAppData))
            localAppData = AppContext.BaseDirectory;
        return Path.Combine(localAppData, "WF Reliquary Redux", "logs", "app.log");
    }

    public async Task AppendAsync(string message, Exception? exception = null, CancellationToken cancellationToken = default)
    {
        var entry = FormatEntry(message, exception);
        await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogFilePath)!);
            await File.AppendAllTextAsync(LogFilePath, entry, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private static string FormatEntry(string message, Exception? exception)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"[{DateTimeOffset.UtcNow:O} {ApplicationInfo.Version}] {message}");
        if (exception is not null)
            sb.AppendLine(exception.ToString());
        sb.AppendLine();
        return sb.ToString();
    }
}
