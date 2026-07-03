namespace WF.Reliquary.Application.RecruitingMessage;

/// <summary>
/// Validates and reconciles Recruiting Chat message patterns.
/// </summary>
public static class RecruitingMessagePatternParser
{
    private static readonly HashSet<string> SystemPlaceholders =
        new(StringComparer.Ordinal) { "relic_name", "relic_reward" };

    /// <summary>
    /// Validates the pattern structurally and semantically.
    /// Returns the placeholder names in order on success, or a positioned error message on failure.
    /// </summary>
    public static (bool Ok, string? Error, IReadOnlyList<string> PlaceholderNames) Validate(string pattern)
    {
        var names = new List<string>();
        var inPlaceholder = false;
        var nameStart = 0;

        for (var i = 0; i < pattern.Length; i++)
        {
            var c = pattern[i];

            if (c == '{')
            {
                if (inPlaceholder)
                    return (false, $"Nested placeholder at position {i}.", []);

                inPlaceholder = true;
                nameStart = i + 1;
            }
            else if (c == '}')
            {
                if (!inPlaceholder)
                    return (false, $"Unexpected '}}' at position {i}.", []);

                var name = pattern[nameStart..i];
                if (name.Length == 0)
                    return (false, $"Empty placeholder at position {nameStart - 1}.", []);

                names.Add(name);
                inPlaceholder = false;
            }
            else if (inPlaceholder)
            {
                if (!IsValidNameChar(c, i == nameStart))
                    return (false, $"Invalid character '{c}' in placeholder name at position {i}.", []);
            }
        }

        if (inPlaceholder)
            return (false, $"Unclosed placeholder starting at position {nameStart - 1}.", []);

        if (!names.Contains("relic_name", StringComparer.Ordinal))
            return (false, "Pattern must contain {relic_name}.", []);

        return (true, null, names);
    }

    /// <summary>
    /// Reconciles draft option groups against a new pattern using three-tier lookup:
    /// current draft group → saved group → empty group. Groups for vanished placeholders are dropped.
    /// Returns the error message on validation failure, leaving groups unchanged.
    /// </summary>
    public static (bool Ok, string? Error, IReadOnlyList<RecruitingMessageOptionGroup> Groups) Reconcile(
        string newPattern,
        IReadOnlyList<RecruitingMessageOptionGroup> currentGroups,
        IReadOnlyList<RecruitingMessageOptionGroup> savedGroups)
    {
        var (ok, error, names) = Validate(newPattern);
        if (!ok)
            return (false, error, currentGroups);

        var currentByName = currentGroups.ToDictionary(g => g.Placeholder, StringComparer.Ordinal);
        var savedByName = savedGroups.ToDictionary(g => g.Placeholder, StringComparer.Ordinal);

        var result = new List<RecruitingMessageOptionGroup>();
        foreach (var name in names)
        {
            if (SystemPlaceholders.Contains(name))
                continue;

            if (currentByName.TryGetValue(name, out var current))
                result.Add(current);
            else if (savedByName.TryGetValue(name, out var saved))
                result.Add(saved);
            else
                result.Add(new RecruitingMessageOptionGroup(name, [], string.Empty));
        }

        return (true, null, result);
    }

    /// <summary>
    /// Returns the custom placeholder names from a validated pattern (system names excluded).
    /// </summary>
    public static IReadOnlyList<string> CustomPlaceholderNames(IReadOnlyList<string> allNames) =>
        allNames.Where(n => !SystemPlaceholders.Contains(n)).ToList();

    private static bool IsValidNameChar(char c, bool isFirst) =>
        isFirst
            ? char.IsLetter(c) || c == '_'
            : char.IsLetterOrDigit(c) || c == '_';
}
