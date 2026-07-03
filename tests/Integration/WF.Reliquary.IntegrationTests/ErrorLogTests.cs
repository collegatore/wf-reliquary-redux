using NUnit.Framework;
using WF.Reliquary.Application;
using WF.Reliquary.Infrastructure.Logging;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class ErrorLogTests
{
    private string tempPath = null!;

    [SetUp]
    public void SetUp()
    {
        tempPath = Path.Combine(Path.GetTempPath(), $"reliquary-test-{Guid.NewGuid():N}", "app.log");
    }

    [TearDown]
    public void TearDown()
    {
        var dir = Path.GetDirectoryName(tempPath);
        if (dir is not null && Directory.Exists(dir))
            Directory.Delete(dir, recursive: true);
    }

    [Test]
    public void LogFilePath_ReturnsAbsolutePath()
    {
        var log = new FileErrorLog(tempPath);

        Assert.That(Path.IsPathRooted(log.LogFilePath), Is.True);
        Assert.That(log.LogFilePath, Is.EqualTo(tempPath));
    }

    [Test]
    public async Task AppendAsync_CreatesFileWhenAbsent()
    {
        var log = new FileErrorLog(tempPath);

        await log.AppendAsync("test message");

        Assert.That(File.Exists(tempPath), Is.True);
    }

    [Test]
    public async Task AppendAsync_WritesTimestampedEntryWithVersion()
    {
        var log = new FileErrorLog(tempPath);

        await log.AppendAsync("something went wrong");

        var content = await File.ReadAllTextAsync(tempPath);
        Assert.That(content, Does.Contain("something went wrong"));
        // Entry starts with an ISO 8601 timestamp bracket followed by the app version
        Assert.That(content, Does.Match(@"\[\d{4}-\d{2}-\d{2}T"));
        Assert.That(content, Does.Contain(ApplicationInfo.Version));
    }

    [Test]
    public async Task AppendAsync_IncludesExceptionDetail()
    {
        var log = new FileErrorLog(tempPath);
        var ex = new InvalidOperationException("boom");

        await log.AppendAsync("operation failed", ex);

        var content = await File.ReadAllTextAsync(tempPath);
        Assert.That(content, Does.Contain("operation failed"));
        Assert.That(content, Does.Contain("InvalidOperationException"));
        Assert.That(content, Does.Contain("boom"));
    }

    [Test]
    public async Task AppendAsync_AccumulatesMultipleEntries()
    {
        var log = new FileErrorLog(tempPath);

        await log.AppendAsync("first");
        await log.AppendAsync("second");

        var content = await File.ReadAllTextAsync(tempPath);
        Assert.That(content, Does.Contain("first"));
        Assert.That(content, Does.Contain("second"));
    }

    [Test]
    public async Task AppendAsync_IsSafeUnderConcurrentWrites()
    {
        var log = new FileErrorLog(tempPath);
        var tasks = Enumerable.Range(0, 20)
            .Select(i => log.AppendAsync($"concurrent entry {i}"))
            .ToArray();

        await Task.WhenAll(tasks);

        var content = await File.ReadAllTextAsync(tempPath);
        for (int i = 0; i < 20; i++)
            Assert.That(content, Does.Contain($"concurrent entry {i}"));
    }
}
