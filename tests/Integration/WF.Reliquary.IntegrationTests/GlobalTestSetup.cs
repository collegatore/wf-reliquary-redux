using AsyncAwaitBestPractices;
using NUnit.Framework;

namespace WF.Reliquary.IntegrationTests;

/// <summary>
/// Mirrors <c>Program.Main</c>: installs a default <c>SafeFireAndForget</c> exception
/// handler so fire-and-forget failures (e.g. a ViewModel's constructor reload running
/// against an intentionally-incomplete mock) are observed and swallowed rather than
/// rethrown onto NUnit's synchronization context and failing unrelated tests.
/// </summary>
[SetUpFixture]
public sealed class GlobalTestSetup
{
    [OneTimeSetUp]
    public void Init()
    {
        SafeFireAndForgetExtensions.SetDefaultExceptionHandling(_ => { });
    }
}
