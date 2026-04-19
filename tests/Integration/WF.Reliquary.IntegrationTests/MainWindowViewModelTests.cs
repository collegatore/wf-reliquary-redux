using Moq;
using NUnit.Framework;
using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Presentation;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class MainWindowViewModelTests
{
    [Test]
    public async Task LoadStatusAsync_ShowsEmptyStateWhenNoFileExists()
    {
        var workflow = CreateWorkflow(DropTableSourceStatus.Missing(DropTableSourceOptions.CreateDefault()));
        var viewModel = new MainWindowViewModel(workflow.Object);

        await viewModel.LoadStatusAsync();

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.StatusText, Is.EqualTo("No drop-table file downloaded yet."));
            Assert.That(viewModel.SourceUrlText, Is.EqualTo(DropTableSourceDefaults.SourceUrl));
        });
    }

    [Test]
    public async Task SaveSourceUrlAsync_PersistsAbsoluteUrl()
    {
        var workflow = CreateWorkflow(DropTableSourceStatus.Missing(DropTableSourceOptions.CreateDefault()));
        var sourceUrl = new Uri("https://example.test/custom.html");
        var viewModel = new MainWindowViewModel(workflow.Object)
        {
            SourceUrlText = sourceUrl.ToString()
        };

        await viewModel.SaveSourceUrlAsync();

        Assert.That(viewModel.OperationStatusText, Is.EqualTo("Drop-table source URL saved."));
        workflow.Verify(
            workflow => workflow.SaveSettingsAsync(
                It.Is<DropTableSourceSettings>(settings => settings.SourceUrl == sourceUrl),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task SaveSourceUrlAsync_RejectsRelativeUrl()
    {
        var workflow = CreateWorkflow(DropTableSourceStatus.Missing(DropTableSourceOptions.CreateDefault()));
        var viewModel = new MainWindowViewModel(workflow.Object)
        {
            SourceUrlText = "/relative/path.html"
        };

        await viewModel.SaveSourceUrlAsync();

        Assert.That(viewModel.OperationStatusText, Is.EqualTo("Source URL must be an absolute URL."));
        workflow.Verify(
            workflow => workflow.SaveSettingsAsync(It.IsAny<DropTableSourceSettings>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task LoadStatusAsync_ShowsLastUpdateWhenAvailable()
    {
        var viewModel = new MainWindowViewModel(CreateWorkflow(StatusWithLastUpdate("08 April, 2026")).Object);

        await viewModel.LoadStatusAsync();

        Assert.That(viewModel.StatusText, Is.EqualTo("Last Update: 08 April, 2026"));
    }

    [Test]
    public async Task RefreshAsync_ShowsFailureAndKeepsCommandAvailable()
    {
        var status = StatusWithLastUpdate("08 April, 2026");
        var workflow = CreateWorkflow(status);
        workflow
            .Setup(workflow => workflow.RefreshAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(DropTableRefreshResult.Failure(status, "network unavailable"));
        var viewModel = new MainWindowViewModel(workflow.Object);

        await viewModel.RefreshAsync();

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.StatusText, Is.EqualTo("Last Update: 08 April, 2026"));
            Assert.That(viewModel.OperationStatusText, Is.EqualTo("Download failed: network unavailable"));
            Assert.That(viewModel.IsBusy, Is.False);
            Assert.That(viewModel.DownloadCommand.CanExecute(null), Is.True);
        });
    }

    private static Mock<IDropTableSourceRefreshWorkflow> CreateWorkflow(DropTableSourceStatus status)
    {
        var workflow = new Mock<IDropTableSourceRefreshWorkflow>();
        workflow
            .Setup(workflow => workflow.GetStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(status);
        workflow
            .Setup(workflow => workflow.GetSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DropTableSourceSettings(status.SourceUrl));
        workflow
            .Setup(workflow => workflow.RefreshAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(DropTableRefreshResult.Success(status));

        return workflow;
    }

    private static DropTableSourceStatus StatusWithLastUpdate(string lastUpdate)
    {
        var options = DropTableSourceOptions.CreateDefault();

        return new DropTableSourceStatus(
            true,
            options.SourceUrl,
            "stored.html",
            DateTimeOffset.UnixEpoch,
            lastUpdate);
    }
}
