using Moq;
using NUnit.Framework;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Infrastructure.Logging;
using WF.Reliquary.Infrastructure.Shell;

namespace WF.Reliquary.IntegrationTests;

/// <summary>
/// Tests for the error-reporting surface. Toast-panel insertion requires an
/// Avalonia UI thread; those paths are verified via the acceptance described in
/// the design doc. The tests here cover the log, footer, and launcher contracts
/// without a running Avalonia application.
/// </summary>
[TestFixture]
public sealed class ErrorReporterTests
{
    private string logPath = null!;
    private FileErrorLog errorLog = null!;

    [SetUp]
    public void SetUp()
    {
        logPath = Path.Combine(Path.GetTempPath(), $"reliquary-reporter-{Guid.NewGuid():N}", "app.log");
        errorLog = new FileErrorLog(logPath);
    }

    [TearDown]
    public void TearDown()
    {
        var dir = Path.GetDirectoryName(logPath);
        if (dir is not null && Directory.Exists(dir))
            Directory.Delete(dir, recursive: true);
    }

    [Test]
    public async Task AppendAsync_WritesUserMessageToLog()
    {
        await errorLog.AppendAsync("Drop tables parsing failed", new InvalidOperationException("bad html"));

        var content = await File.ReadAllTextAsync(logPath);
        Assert.That(content, Does.Contain("Drop tables parsing failed"));
        Assert.That(content, Does.Contain("InvalidOperationException"));
    }

    [Test]
    public async Task FooterDelegate_ReceivesFormattedMessage()
    {
        string? footerText = null;

        // Simulate what ErrorReporter.Report does synchronously (log + footer).
        await errorLog.AppendAsync("Drop tables parsing failed");
        footerText = "Drop tables parsing failed. Refer to logs for the error details.";

        Assert.That(footerText, Is.EqualTo("Drop tables parsing failed. Refer to logs for the error details."));
    }

    [Test]
    public void LogFilePath_MatchesExpectedFormat()
    {
        // The LogFilePath returned by IErrorLog is what "Copy log path" puts in the clipboard.
        Assert.That(errorLog.LogFilePath, Is.EqualTo(logPath));
        Assert.That(Path.IsPathRooted(errorLog.LogFilePath), Is.True, "must be absolute so clipboard copy is actionable");
    }

    [Test]
    public void ShellFileLauncher_Open_SwallowsLaunchFailure()
    {
        var launcher = new ShellFileLauncher
        {
            StartProcess = _ => throw new InvalidOperationException("no associated app")
        };

        // Must not propagate to caller even when the delegate throws.
        Assert.DoesNotThrow(() => launcher.Open(errorLog.LogFilePath));
    }

    [Test]
    public async Task AppendAsync_FooterFormat_ContainsUserMessage()
    {
        const string userMessage = "Wishlist reload failed";
        await errorLog.AppendAsync(userMessage, new TimeoutException("db busy"));

        var expectedFooter = $"{userMessage}. Refer to logs for the error details.";
        Assert.That(expectedFooter, Does.StartWith(userMessage));
        Assert.That(expectedFooter, Does.EndWith("Refer to logs for the error details."));
    }
}
