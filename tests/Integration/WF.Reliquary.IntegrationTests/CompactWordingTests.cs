using NUnit.Framework;
using WF.Reliquary.Presentation;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class CompactWordingTests
{
    [Test]
    public void ClipRelicName_StripsTrailingRelicWord()
    {
        Assert.That(CompactWording.ClipRelicName("Axi A15 Relic"), Is.EqualTo("Axi A15"));
    }

    [Test]
    public void ClipRewardName_ComponentBlueprint_DropsPrimeAndBlueprint()
    {
        Assert.That(
            CompactWording.ClipRewardName("Revenant Prime Neuroptics Blueprint"),
            Is.EqualTo("Revenant Neuroptics"));
    }

    [Test]
    public void ClipRewardName_MainBlueprint_DropsPrimeButKeepsBlueprint()
    {
        Assert.That(
            CompactWording.ClipRewardName("Revenant Prime Blueprint"),
            Is.EqualTo("Revenant Blueprint"));
    }

    [Test]
    public void ClipRewardName_NonPrimeTrailingBlueprint_IsClipped()
    {
        Assert.That(CompactWording.ClipRewardName("Forma Blueprint"), Is.EqualTo("Forma"));
    }
}
