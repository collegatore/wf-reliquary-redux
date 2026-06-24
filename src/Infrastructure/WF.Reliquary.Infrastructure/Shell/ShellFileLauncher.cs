using System.Diagnostics;
using WF.Reliquary.Application.Ports;

namespace WF.Reliquary.Infrastructure.Shell;

public sealed class ShellFileLauncher : IFileLauncher
{
    // Overridable in tests to capture the ProcessStartInfo without spawning a process.
    internal Func<ProcessStartInfo, Process?> StartProcess { get; init; } = Process.Start!;

    public void Open(string path)
    {
        try
        {
            StartProcess(BuildStartInfo(path));
        }
        catch
        {
            // launch failures are silently swallowed; copy-path is the fallback
        }
    }

    private static ProcessStartInfo BuildStartInfo(string path)
    {
        if (OperatingSystem.IsWindows())
            return new ProcessStartInfo(path) { UseShellExecute = true };

        // Linux: xdg-open is the portable desktop default-handler convention
        return new ProcessStartInfo("xdg-open", path);
    }
}
