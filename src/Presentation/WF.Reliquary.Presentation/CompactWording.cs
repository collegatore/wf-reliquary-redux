namespace WF.Reliquary.Presentation;

/// <summary>
/// Pure presentation-layer string transforms for Compact/Table view modes. Clips
/// verbatim relic and reward names down to a shorter display form; the stored/domain
/// names are never modified. See docs/card-display-design.md for the rationale.
/// </summary>
public static class CompactWording
{
    private const string RelicSuffix = " Relic";
    private const string PrimeWord = " Prime";
    private const string BlueprintSuffix = " Blueprint";
    private const string PrimeBlueprintSuffix = "Prime Blueprint";

    /// <summary>Strips the trailing " Relic" from a relic display name.</summary>
    public static string ClipRelicName(string relicName)
    {
        return relicName.EndsWith(RelicSuffix, StringComparison.Ordinal)
            ? relicName[..^RelicSuffix.Length]
            : relicName;
    }

    /// <summary>
    /// Removes " Prime" from a reward name, and strips a trailing " Blueprint" unless
    /// the original name ends with "Prime Blueprint" (a main blueprint), in which case
    /// "Blueprint" is retained.
    /// </summary>
    public static string ClipRewardName(string rewardName)
    {
        var isMainBlueprint = rewardName.EndsWith(PrimeBlueprintSuffix, StringComparison.Ordinal);

        var clipped = rewardName.Replace(PrimeWord, string.Empty);

        if (!isMainBlueprint && clipped.EndsWith(BlueprintSuffix, StringComparison.Ordinal))
        {
            clipped = clipped[..^BlueprintSuffix.Length];
        }

        return clipped;
    }
}
