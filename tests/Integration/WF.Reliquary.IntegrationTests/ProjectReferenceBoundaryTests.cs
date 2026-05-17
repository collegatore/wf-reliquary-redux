using System.Xml.Linq;
using NUnit.Framework;
using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Composition;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class ProjectReferenceBoundaryTests
{
    [Test]
    public void ApplicationProject_HasNoLayerProjectReferences()
    {
        var references = ProjectReferences("src", "Application", "WF.Reliquary.Application", "WF.Reliquary.Application.csproj");

        Assert.That(references, Is.Empty);
    }

    [Test]
    public void PresentationProject_DoesNotReferenceInfrastructureDirectly()
    {
        var references = ProjectReferences("src", "Presentation", "WF.Reliquary.Presentation", "WF.Reliquary.Presentation.csproj");

        Assert.That(references, Has.None.Contains("Infrastructure"));
        Assert.That(references, Has.Some.Contains("Application"));
        Assert.That(references, Has.Some.Contains("Composition"));
    }

    [Test]
    public void InfrastructureProject_MayReferenceApplication()
    {
        var references = ProjectReferences("src", "Infrastructure", "WF.Reliquary.Infrastructure", "WF.Reliquary.Infrastructure.csproj");

        Assert.That(references, Has.Some.Contains("Application"));
    }

    [Test]
    public void CompositionProject_WiresApplicationAndInfrastructure()
    {
        var references = ProjectReferences("src", "Composition", "WF.Reliquary.Composition", "WF.Reliquary.Composition.csproj");

        Assert.That(references, Has.Some.Contains("Application"));
        Assert.That(references, Has.Some.Contains("Infrastructure"));
    }

    [Test]
    public void CompositionFactory_CreatesImportWorkflowThroughApplicationContract()
    {
        var workflow = ReliquaryServiceFactory.CreateDropTableImportService();

        Assert.That(workflow, Is.InstanceOf<IDropTableImportWorkflow>());
    }

    private static string[] ProjectReferences(params string[] pathParts)
    {
        var projectPath = Path.Combine(new[] { RepositoryPaths.Root }.Concat(pathParts).ToArray());
        var document = XDocument.Load(projectPath);

        return document
            .Descendants("ProjectReference")
            .Select(reference => reference.Attribute("Include")?.Value ?? string.Empty)
            .Where(value => value.Length > 0)
            .ToArray();
    }
}
