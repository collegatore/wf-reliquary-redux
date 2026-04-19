using NUnit.Framework;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class ReadmeGuidanceTests
{
    [Test]
    public void Readme_ContainsValidationCommandsAndLayerGuidance()
    {
        var readme = File.ReadAllText(Path.Combine(RepositoryPaths.Root, "README.md"));

        Assert.Multiple(() =>
        {
            Assert.That(readme, Does.Contain("dotnet restore"));
            Assert.That(readme, Does.Contain("dotnet build"));
            Assert.That(readme, Does.Contain("dotnet test"));
            Assert.That(readme, Does.Contain("dotnet: run presentation"));
            Assert.That(readme, Does.Contain(".NET: Launch Presentation"));
            Assert.That(readme, Does.Contain(".NET: Debug Presentation"));
            Assert.That(readme, Does.Contain("src/Application/WF.Reliquary.Application"));
            Assert.That(readme, Does.Contain("src/Composition/WF.Reliquary.Composition"));
            Assert.That(readme, Does.Contain("src/Infrastructure/WF.Reliquary.Infrastructure"));
            Assert.That(readme, Does.Contain("src/Presentation/WF.Reliquary.Presentation"));
            Assert.That(readme, Does.Contain("Edit the source URL in the application UI"));
            Assert.That(readme, Does.Contain("application-level user settings"));
            Assert.That(readme, Does.Contain("Last Update"));
            Assert.That(readme, Does.Contain("DropTables"));
            Assert.That(readme, Does.Contain("Future Work"));
        });
    }
}
