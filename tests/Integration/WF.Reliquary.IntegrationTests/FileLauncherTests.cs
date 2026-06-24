using System.Diagnostics;
using NUnit.Framework;
using WF.Reliquary.Infrastructure.Shell;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class FileLauncherTests
{
    [Test]
    public void Open_OnWindows_UsesShellExecuteWithPath()
    {
        if (!OperatingSystem.IsWindows())
            Assert.Ignore("Windows-only branch.");

        ProcessStartInfo? captured = null;
        var launcher = new ShellFileLauncher
        {
            StartProcess = psi => { captured = psi; return null; }
        };

        launcher.Open(@"C:\some\file.log");

        Assert.That(captured, Is.Not.Null);
        Assert.That(captured!.FileName, Is.EqualTo(@"C:\some\file.log"));
        Assert.That(captured.UseShellExecute, Is.True);
    }

    [Test]
    public void Open_OnLinux_UsesXdgOpen()
    {
        if (!OperatingSystem.IsLinux())
            Assert.Ignore("Linux-only branch.");

        ProcessStartInfo? captured = null;
        var launcher = new ShellFileLauncher
        {
            StartProcess = psi => { captured = psi; return null; }
        };

        launcher.Open("/home/user/.local/app.log");

        Assert.That(captured, Is.Not.Null);
        Assert.That(captured!.FileName, Is.EqualTo("xdg-open"));
        Assert.That(captured.Arguments, Does.Contain("/home/user/.local/app.log"));
    }

    [Test]
    public void Open_WhenLaunchFails_DoesNotThrow()
    {
        var launcher = new ShellFileLauncher
        {
            StartProcess = _ => throw new InvalidOperationException("no handler")
        };

        Assert.DoesNotThrow(() => launcher.Open("/nonexistent/path.log"));
    }
}
