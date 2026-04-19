using NUnit.Framework;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class BoilerplateSmokeTests
{
    [Test]
    public void RequiredProjectFiles_Exist()
    {
        var root = RepositoryPaths.Root;

        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(Path.Combine(root, "WF.Reliquary.Redux.sln")), Is.True);
            Assert.That(File.Exists(Path.Combine(root, "src", "Application", "WF.Reliquary.Application", "WF.Reliquary.Application.csproj")), Is.True);
            Assert.That(File.Exists(Path.Combine(root, "src", "Infrastructure", "WF.Reliquary.Infrastructure", "WF.Reliquary.Infrastructure.csproj")), Is.True);
            Assert.That(File.Exists(Path.Combine(root, "src", "Presentation", "WF.Reliquary.Presentation", "WF.Reliquary.Presentation.csproj")), Is.True);
            Assert.That(File.Exists(Path.Combine(root, "tests", "Unit", "WF.Reliquary.UnitTests", "WF.Reliquary.UnitTests.csproj")), Is.True);
            Assert.That(File.Exists(Path.Combine(root, "tests", "Integration", "WF.Reliquary.IntegrationTests", "WF.Reliquary.IntegrationTests.csproj")), Is.True);
        });
    }
}
