using System.Text.RegularExpressions;

namespace WF.Reliquary.Application.Lfg;

public static partial class LfgMessageBuilder
{
    [GeneratedRegex(@"\{(\w+)\}")]
    private static partial Regex PlaceholderRegex();

    /// <summary>
    /// Substitutes all placeholders in the pattern: {relic_name} with the supplied relic name,
    /// each custom placeholder with the selected option value from the group (or empty string if
    /// no group or no selection exists), and {relic_reward} with empty string (deferred).
    /// </summary>
    public static string Build(
        string pattern,
        string relicName,
        IReadOnlyDictionary<string, string> selectedValues)
    {
        return PlaceholderRegex().Replace(pattern, match =>
        {
            var name = match.Groups[1].Value;
            if (string.Equals(name, "relic_name", StringComparison.Ordinal))
                return relicName;

            selectedValues.TryGetValue(name, out var value);
            return value ?? string.Empty;
        });
    }
}
