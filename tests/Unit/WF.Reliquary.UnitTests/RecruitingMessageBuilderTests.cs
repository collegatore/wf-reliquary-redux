using NUnit.Framework;
using WF.Reliquary.Application.RecruitingMessage;

namespace WF.Reliquary.UnitTests;

[TestFixture]
public sealed class RecruitingMessageBuilderTests
{
    [Test]
    public void Build_DefaultTemplate_ProducesExpectedMessage()
    {
        var template = RecruitingMessageTemplate.Default;
        var selected = new Dictionary<string, string>
        {
            ["participation"] = "LF",
            ["refinement"] = "rad",
        };

        var result = RecruitingMessageBuilder.Build(template.Pattern, "Axi A1 Relic", selected);

        Assert.That(result, Is.EqualTo("LF [Axi A1 Relic] rad"));
    }

    [Test]
    public void Build_SubstitutesValue_NotDescriptor()
    {
        var pattern = "{participation} [{relic_name}]";
        var selected = new Dictionary<string, string> { ["participation"] = "H" };

        var result = RecruitingMessageBuilder.Build(pattern, "Neo N1 Relic", selected);

        Assert.That(result, Is.EqualTo("H [Neo N1 Relic]"));
        Assert.That(result, Does.Not.Contain("Hosting"));
    }

    [Test]
    public void Build_MissingGroupValue_SubstitutesEmptyString()
    {
        var pattern = "{participation} [{relic_name}]";
        var selected = new Dictionary<string, string>(); // participation not selected

        var result = RecruitingMessageBuilder.Build(pattern, "Meso M1 Relic", selected);

        Assert.That(result, Is.EqualTo(" [Meso M1 Relic]"));
    }

    [Test]
    public void Build_RelicNameSubstituted()
    {
        var pattern = "[{relic_name}]";
        var result = RecruitingMessageBuilder.Build(pattern, "Lith L1 Relic", new Dictionary<string, string>());

        Assert.That(result, Is.EqualTo("[Lith L1 Relic]"));
    }

    [Test]
    public void Build_RelicReward_SubstitutesEmpty_WhenDeferred()
    {
        var pattern = "[{relic_name}] {relic_reward}";
        var result = RecruitingMessageBuilder.Build(pattern, "Axi A1 Relic", new Dictionary<string, string>());

        Assert.That(result, Is.EqualTo("[Axi A1 Relic] "));
    }
}

[TestFixture]
public sealed class RecruitingMessageTemplateDefaultTests
{
    [Test]
    public void Default_HasExpectedPattern()
    {
        Assert.That(RecruitingMessageTemplate.Default.Pattern, Is.EqualTo("{participation} [{relic_name}] {refinement}"));
    }

    [Test]
    public void Default_HasTwoGroups()
    {
        Assert.That(RecruitingMessageTemplate.Default.Groups, Has.Count.EqualTo(2));
    }

    [Test]
    public void Default_ParticipationGroup_HasCorrectOptions()
    {
        var group = RecruitingMessageTemplate.Default.Groups[0];

        Assert.That(group.Placeholder, Is.EqualTo("participation"));
        Assert.That(group.DefaultValue, Is.EqualTo("LF"));
        Assert.That(group.Options.Select(o => o.Value), Is.EquivalentTo(new[] { "H", "LF" }));
        Assert.That(group.Options.Select(o => o.Descriptor), Is.EquivalentTo(new[] { "Hosting", "Looking for group" }));
    }

    [Test]
    public void Default_RefinementGroup_HasCorrectOptions()
    {
        var group = RecruitingMessageTemplate.Default.Groups[1];

        Assert.That(group.Placeholder, Is.EqualTo("refinement"));
        Assert.That(group.DefaultValue, Is.EqualTo("rad"));
        Assert.That(group.Options.Select(o => o.Value), Is.EquivalentTo(new[] { "inta", "exceptional", "flawless", "rad" }));
    }
}
