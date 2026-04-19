using NUnit.Framework;
using WF.Reliquary.Application;

namespace WF.Reliquary.UnitTests;

[TestFixture]
public sealed class ApplicationInfoTests
{
    [Test]
    public void GetStartupSummary_ReturnsApplicationNameAndVersion()
    {
        var summary = ApplicationInfo.GetStartupSummary();

        Assert.That(summary, Does.Contain(ApplicationInfo.Name));
        Assert.That(summary, Does.Contain(ApplicationInfo.Version));
    }
}
