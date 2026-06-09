namespace WF.Reliquary.Presentation;

/// <summary>
/// Shared validation rules for profile and wishlist names. Names are non-empty,
/// capped at <see cref="MaxLength"/> characters (trimmed), and unique within their
/// scope (case-insensitive on the trimmed value). Used by both the name modal and the
/// view models so the dialog and the authoritative view-model guard agree.
/// </summary>
public static class NameRules
{
    /// <summary>The "reasonably long" maximum name length, shared by profiles and wishlists.</summary>
    public const int MaxLength = 50;

    /// <summary>
    /// Validates a candidate name against the non-empty, length, and uniqueness rules.
    /// Returns <c>null</c> when the name is valid, otherwise a human-readable error message.
    /// <paramref name="takenNames"/> must already exclude the name being renamed so an
    /// unchanged rename passes.
    /// </summary>
    public static string? Validate(string? name, IReadOnlyCollection<string> takenNames)
    {
        var trimmed = name?.Trim() ?? string.Empty;

        if (trimmed.Length == 0)
            return "Name cannot be empty.";

        if (trimmed.Length > MaxLength)
            return $"Name must be {MaxLength} characters or fewer.";

        if (takenNames.Any(n => string.Equals(n.Trim(), trimmed, StringComparison.OrdinalIgnoreCase)))
            return "That name is already in use.";

        return null;
    }
}

/// <summary>
/// Input for the name modal: the value to pre-fill (null for create) and the set of names
/// already taken in the relevant scope (excluding the current name when renaming).
/// </summary>
public sealed record NameDialogRequest(string? Prefill, IReadOnlyCollection<string> TakenNames);
