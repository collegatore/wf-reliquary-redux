using NUnit.Framework;
using WF.Reliquary.Presentation;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class UiConcurrencyContractTests
{
    // ── BusyScope ───────────────────────────────────────────────────────────────

    [Test]
    public void BusyScope_FirstEntrySucceeds_ReentrantEntryReturnsNull()
    {
        var changes = new List<bool>();
        var scope = new BusyScope(changes.Add);

        using var first = scope.TryEnter();
        var reentrant = scope.TryEnter();

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.Not.Null, "first entry acquires the scope");
            Assert.That(reentrant, Is.Null, "a reentrant entry while busy is refused");
            Assert.That(scope.IsBusy, Is.True);
            Assert.That(changes, Is.EqualTo(new[] { true }));
        });
    }

    [Test]
    public void BusyScope_ResetsOnDispose_AndCanBeReEntered()
    {
        var scope = new BusyScope();

        using (var token = scope.TryEnter())
        {
            Assert.That(token, Is.Not.Null);
        }

        Assert.That(scope.IsBusy, Is.False);
        using var again = scope.TryEnter();
        Assert.That(again, Is.Not.Null, "the scope is re-enterable after the previous token is disposed");
    }

    // ── ReloadGate ──────────────────────────────────────────────────────────────

    [Test]
    public void ReloadGate_Begin_CancelsThePreviousToken()
    {
        using var gate = new ReloadGate();

        var first = gate.Begin();
        var second = gate.Begin();

        Assert.Multiple(() =>
        {
            Assert.That(first.IsCancellationRequested, Is.True, "the superseded reload's token is cancelled");
            Assert.That(second.IsCancellationRequested, Is.False, "the latest reload's token stays live");
        });
    }

    [Test]
    public void ReloadGate_Dispose_CancelsTheCurrentToken()
    {
        var gate = new ReloadGate();
        var token = gate.Begin();

        gate.Dispose();

        Assert.That(token.IsCancellationRequested, Is.True);
    }

    // ── Source-inspection guard ──────────────────────────────────────────────────

    // ViewModels must not offload work themselves (offload lives in Infrastructure) and
    // must not use ConfigureAwait(false) (continuations must resume on the UI thread to
    // mutate bound collections). MainWindow.axaml.cs is the bounded startup/shutdown
    // exception that may block.
    [Test]
    public void ViewModels_DoNotCallTaskRun_OrConfigureAwaitFalse()
    {
        var vmDir = Path.Combine(
            RepositoryPaths.Root, "src", "Presentation", "WF.Reliquary.Presentation");

        var viewModelFiles = Directory.GetFiles(vmDir, "*ViewModel.cs", SearchOption.TopDirectoryOnly);
        Assert.That(viewModelFiles, Is.Not.Empty, "expected to find ViewModel source files");

        var offenders = new List<string>();
        foreach (var file in viewModelFiles)
        {
            var text = File.ReadAllText(file);
            if (text.Contains("Task.Run"))
                offenders.Add($"{Path.GetFileName(file)} contains Task.Run (offload belongs in Infrastructure)");
            if (text.Contains("ConfigureAwait(false)"))
                offenders.Add($"{Path.GetFileName(file)} contains ConfigureAwait(false) (ViewModels must resume on the UI thread)");
        }

        Assert.That(offenders, Is.Empty, string.Join(Environment.NewLine, offenders));
    }

    [Test]
    public void InteractiveCode_DoesNotBlockOnAsync_OutsideStartupShutdown()
    {
        var presentationDir = Path.Combine(
            RepositoryPaths.Root, "src", "Presentation", "WF.Reliquary.Presentation");

        // MainWindow.axaml.cs is the sanctioned startup/shutdown boundary (constructor +
        // OnWindowClosing); every other Presentation file must stay non-blocking.
        var offenders = Directory
            .GetFiles(presentationDir, "*.cs", SearchOption.TopDirectoryOnly)
            .Where(f => Path.GetFileName(f) != "MainWindow.axaml.cs")
            .Where(f => File.ReadAllText(f) is var t
                && (t.Contains(".GetAwaiter().GetResult()") || t.Contains(".Result;") || t.Contains(".Wait()")))
            .Select(Path.GetFileName)
            .ToList();

        Assert.That(offenders, Is.Empty,
            "blocking-on-async found outside the startup/shutdown boundary: " + string.Join(", ", offenders));
    }
}
