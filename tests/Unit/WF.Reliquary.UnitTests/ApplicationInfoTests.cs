using NUnit.Framework;
using WF.Reliquary.Application;

namespace WF.Reliquary.UnitTests;

[TestFixture]
public sealed class ApplicationInfoTests
{
    [Test]
    public void Version_DoesNotContainBuildMetadataSuffix()
    {
        Assert.That(ApplicationInfo.Version, Does.Not.Contain("+"));
    }

    [Test]
    public void Version_IsNotEmpty()
    {
        Assert.That(ApplicationInfo.Version, Is.Not.Empty);
    }

    [Test]
    public void GetStartupSummary_ReturnsApplicationNameAndVersion()
    {
        var summary = ApplicationInfo.GetStartupSummary();

        Assert.That(summary, Does.Contain(ApplicationInfo.Name));
        Assert.That(summary, Does.Contain(ApplicationInfo.Version));
    }
}
