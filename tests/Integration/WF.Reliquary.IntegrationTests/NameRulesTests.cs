using NUnit.Framework;
using WF.Reliquary.Presentation;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class NameRulesTests
{
    private static readonly string[] None = [];

    [Test]
    public void Validate_ReturnsError_ForEmptyName()
    {
        Assert.That(NameRules.Validate("   ", None), Is.Not.Null);
    }

    [Test]
    public void Validate_ReturnsNull_ForNameAtTheLengthLimit()
    {
        var atLimit = new string('a', NameRules.MaxLength);
        Assert.That(NameRules.Validate(atLimit, None), Is.Null);
    }

    [Test]
    public void Validate_ReturnsError_ForNameOverTheLengthLimit()
    {
        var overLimit = new string('a', NameRules.MaxLength + 1);

        var error = NameRules.Validate(overLimit, None);

        Assert.That(error, Is.Not.Null);
        Assert.That(error, Does.Contain(NameRules.MaxLength.ToString()));
    }

    [Test]
    public void Validate_TrimsBeforeMeasuringLength()
    {
        // 50 'a' characters with surrounding whitespace is still within the limit.
        var padded = "  " + new string('a', NameRules.MaxLength) + "  ";
        Assert.That(NameRules.Validate(padded, None), Is.Null);
    }

    [Test]
    public void Validate_ReturnsError_ForDuplicateNameCaseInsensitively()
    {
        Assert.That(NameRules.Validate("alt account", ["Alt Account"]), Is.Not.Null);
    }

    [Test]
    public void Validate_ReturnsError_ForDuplicateNameIgnoringSurroundingWhitespace()
    {
        Assert.That(NameRules.Validate("Alt", [" alt "]), Is.Not.Null);
    }

    [Test]
    public void Validate_ReturnsNull_WhenNameNotTaken()
    {
        Assert.That(NameRules.Validate("Fresh", ["Default", "Second"]), Is.Null);
    }
}
