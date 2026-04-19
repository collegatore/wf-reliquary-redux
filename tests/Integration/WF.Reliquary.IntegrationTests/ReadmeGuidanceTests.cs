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
            Assert.That(readme, Does.Contain("src/Application/WF.Reliquary.Application"));
            Assert.That(readme, Does.Contain("src/Infrastructure/WF.Reliquary.Infrastructure"));
            Assert.That(readme, Does.Contain("src/Presentation/WF.Reliquary.Presentation"));
            Assert.That(readme, Does.Contain("Future Work"));
        });
    }
}
