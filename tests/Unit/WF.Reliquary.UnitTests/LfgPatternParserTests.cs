using NUnit.Framework;
using WF.Reliquary.Application.Lfg;

namespace WF.Reliquary.UnitTests;

[TestFixture]
public sealed class LfgPatternParserTests
{
    // ── Validate: structural rules ─────────────────────────────────────────

    [Test]
    public void Validate_ValidPattern_ReturnsPlaceholdersInOrder()
    {
        var (ok, error, names) = LfgPatternParser.Validate("{participation} [{relic_name}] {refinement}");

        Assert.That(ok, Is.True);
        Assert.That(error, Is.Null);
        Assert.That(names, Is.EqualTo(new[] { "participation", "relic_name", "refinement" }));
    }

    [Test]
    public void Validate_UnclosedPlaceholder_ReturnsError()
    {
        var (ok, error, _) = LfgPatternParser.Validate("{participation} [{relic_name");

        Assert.That(ok, Is.False);
        Assert.That(error, Does.Contain("Unclosed").IgnoreCase);
    }

    [Test]
    public void Validate_NestedPlaceholder_ReturnsError()
    {
        var (ok, error, _) = LfgPatternParser.Validate("{{nested} text}");

        Assert.That(ok, Is.False);
        Assert.That(error, Does.Contain("Nested").IgnoreCase);
    }

    [Test]
    public void Validate_EmptyPlaceholder_ReturnsError()
    {
        var (ok, error, _) = LfgPatternParser.Validate("{} [{relic_name}]");

        Assert.That(ok, Is.False);
        Assert.That(error, Does.Contain("Empty").IgnoreCase);
    }

    [Test]
    public void Validate_StrayClosingBrace_ReturnsError()
    {
        var (ok, error, _) = LfgPatternParser.Validate("text} [{relic_name}]");

        Assert.That(ok, Is.False);
        Assert.That(error, Does.Contain("}").Or.Contain("Unexpected").IgnoreCase);
    }

    [Test]
    public void Validate_InvalidNameChar_ReturnsError()
    {
        var (ok, error, _) = LfgPatternParser.Validate("{bad-name} [{relic_name}]");

        Assert.That(ok, Is.False);
        Assert.That(error, Does.Contain("Invalid").IgnoreCase);
    }

    // ── Validate: semantic rule ────────────────────────────────────────────

    [Test]
    public void Validate_MissingRelicName_ReturnsError()
    {
        var (ok, error, _) = LfgPatternParser.Validate("{participation} {refinement}");

        Assert.That(ok, Is.False);
        Assert.That(error, Does.Contain("relic_name").IgnoreCase);
    }

    [Test]
    public void Validate_PatternWithOnlyRelicName_IsValid()
    {
        var (ok, _, names) = LfgPatternParser.Validate("[{relic_name}]");

        Assert.That(ok, Is.True);
        Assert.That(names, Is.EqualTo(new[] { "relic_name" }));
    }

    [Test]
    public void Validate_ErrorMessageIncludesPosition()
    {
        var (_, error, _) = LfgPatternParser.Validate("{unclosed");

        Assert.That(error, Does.Contain("position").Or.Contain("0").IgnoreCase);
    }
}

[TestFixture]
public sealed class LfgPatternParserReconcileTests
{
    private static LfgOptionGroup MakeGroup(string placeholder, params string[] optionValues) =>
        new(placeholder,
            optionValues.Select(v => new LfgOption($"Label {v}", v)).ToList(),
            optionValues.FirstOrDefault() ?? string.Empty);

    [Test]
    public void Reconcile_PreservesExistingDraftGroup()
    {
        var draft = new[] { MakeGroup("participation", "H", "LF") };
        var saved = new[] { MakeGroup("participation", "H", "LF") };

        var (ok, _, groups) = LfgPatternParser.Reconcile(
            "{participation} [{relic_name}]", draft, saved);

        Assert.That(ok, Is.True);
        Assert.That(groups[0].Placeholder, Is.EqualTo("participation"));
        Assert.That(groups[0].Options, Has.Count.EqualTo(2));
    }

    [Test]
    public void Reconcile_RestoresGroupFromSavedWhenDroppedFromDraft()
    {
        // Draft has no participation group; saved does
        var draft = Array.Empty<LfgOptionGroup>();
        var saved = new[] { MakeGroup("participation", "H", "LF") };

        var (ok, _, groups) = LfgPatternParser.Reconcile(
            "{participation} [{relic_name}]", draft, saved);

        Assert.That(ok, Is.True);
        Assert.That(groups[0].Placeholder, Is.EqualTo("participation"));
        Assert.That(groups[0].Options, Has.Count.EqualTo(2));
    }

    [Test]
    public void Reconcile_CreatesEmptyGroupWhenAbsentFromBoth()
    {
        var draft = Array.Empty<LfgOptionGroup>();
        var saved = Array.Empty<LfgOptionGroup>();

        var (ok, _, groups) = LfgPatternParser.Reconcile(
            "{newgroup} [{relic_name}]", draft, saved);

        Assert.That(ok, Is.True);
        Assert.That(groups[0].Placeholder, Is.EqualTo("newgroup"));
        Assert.That(groups[0].Options, Is.Empty);
    }

    [Test]
    public void Reconcile_DropsGroupWhosePlaceholderVanished()
    {
        var draft = new[] { MakeGroup("participation", "H", "LF"), MakeGroup("refinement", "rad") };
        var saved = draft;

        var (ok, _, groups) = LfgPatternParser.Reconcile(
            "[{relic_name}] {refinement}", draft, saved);

        Assert.That(ok, Is.True);
        Assert.That(groups, Has.Count.EqualTo(1));
        Assert.That(groups[0].Placeholder, Is.EqualTo("refinement"));
    }

    [Test]
    public void Reconcile_OrdersGroupsByPatternAppearance()
    {
        var draft = new[] { MakeGroup("refinement", "rad"), MakeGroup("participation", "H") };
        var saved = draft;

        var (ok, _, groups) = LfgPatternParser.Reconcile(
            "{participation} [{relic_name}] {refinement}", draft, saved);

        Assert.That(ok, Is.True);
        Assert.That(groups[0].Placeholder, Is.EqualTo("participation"));
        Assert.That(groups[1].Placeholder, Is.EqualTo("refinement"));
    }

    [Test]
    public void Reconcile_FailedParse_ReturnsDraftUnchangedAndError()
    {
        var draft = new[] { MakeGroup("participation", "H", "LF") };
        var saved = Array.Empty<LfgOptionGroup>();

        var (ok, error, groups) = LfgPatternParser.Reconcile(
            "{unclosed", draft, saved);

        Assert.That(ok, Is.False);
        Assert.That(error, Is.Not.Null);
        Assert.That(groups, Is.SameAs(draft));
    }

    [Test]
    public void Reconcile_SystemPlaceholderNotAddedAsCustomGroup()
    {
        var draft = Array.Empty<LfgOptionGroup>();
        var saved = Array.Empty<LfgOptionGroup>();

        var (ok, _, groups) = LfgPatternParser.Reconcile(
            "{participation} [{relic_name}] {relic_reward}", draft, saved);

        Assert.That(ok, Is.True);
        Assert.That(groups, Has.Count.EqualTo(1));
        Assert.That(groups[0].Placeholder, Is.EqualTo("participation"));
    }
}
