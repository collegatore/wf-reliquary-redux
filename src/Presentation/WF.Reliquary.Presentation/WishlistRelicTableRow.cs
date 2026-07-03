using System.Windows.Input;

namespace WF.Reliquary.Presentation;

/// <summary>
/// A row in the Table relic view mode's TreeDataGrid. The tree is two levels:
/// top-level <see cref="RelicTableRowViewModel"/> rows, each expanding into
/// <see cref="RewardTableRowViewModel"/> child rows. Columns bind against this
/// base type; the discriminating properties (children, actions, opacity) have
/// defaults here and are overridden by the concrete rows.
/// </summary>
public abstract class WishlistRelicTableRow
{
    // Match the app-wide vaulted/dimmed opacities (MainWindow.axaml styles:
    // Border.vaulted = 0.45, TextBlock.dimmed = 0.5).
    private const double VaultedOpacity = 0.45;
    private const double DimmedOpacity = 0.5;

    /// <summary>Text shown in the hierarchical expander/name column.</summary>
    public abstract string DisplayName { get; }

    /// <summary>
    /// Wishlisted-tier markers for relic rows, e.g. "(C, R)". Empty on child
    /// reward rows (the markers column is blank there).
    /// </summary>
    public virtual string TierMarkers => string.Empty;

    /// <summary>True for vaulted relic rows (drives the reduced row opacity).</summary>
    public virtual bool IsVaulted => false;

    /// <summary>True for non-wishlisted reward rows (drives the reduced row opacity).</summary>
    public virtual bool IsDimmed => false;

    /// <summary>
    /// True on relic rows, where the pictogram action cells render; false on child
    /// reward rows, whose pictogram cells stay empty.
    /// </summary>
    public virtual bool ShowsActions => false;

    /// <summary>Child reward rows for relic rows; null for reward rows (leaves).</summary>
    public virtual IReadOnlyList<WishlistRelicTableRow>? Children => null;

    /// <summary>Opens the rewards modal for this relic (relic rows only).</summary>
    public virtual ICommand? ShowRewardsCommand => null;

    /// <summary>Opens the LFG composer for this relic (relic rows only).</summary>
    public virtual ICommand? ComposeLfgCommand => null;

    /// <summary>
    /// Effective row opacity: vaulted relic rows and non-wishlisted reward rows are
    /// dimmed. Cells bind their opacity to this so the whole row reads dimmed.
    /// </summary>
    public double RowOpacity => IsVaulted ? VaultedOpacity : IsDimmed ? DimmedOpacity : 1.0;
}

/// <summary>A top-level relic row: clipped name, wishlisted-tier markers, pictograms, child rewards.</summary>
public sealed class RelicTableRowViewModel : WishlistRelicTableRow
{
    public RelicTableRowViewModel(
        string clippedName,
        string tierMarkers,
        bool isVaulted,
        IReadOnlyList<WishlistRelicTableRow> children,
        ICommand? showRewardsCommand = null,
        ICommand? composeLfgCommand = null)
    {
        DisplayName = clippedName;
        TierMarkers = tierMarkers;
        IsVaulted = isVaulted;
        Children = children;
        ShowRewardsCommand = showRewardsCommand;
        ComposeLfgCommand = composeLfgCommand;
    }

    public override string DisplayName { get; }
    public override string TierMarkers { get; }
    public override bool IsVaulted { get; }
    public override bool ShowsActions => true;
    public override IReadOnlyList<WishlistRelicTableRow>? Children { get; }
    public override ICommand? ShowRewardsCommand { get; }
    public override ICommand? ComposeLfgCommand { get; }
}

/// <summary>
/// A child reward row: bold tier letter + compact reward name. Only wishlisted
/// rewards become rows (non-wishlisted are omitted), so there is no dimmed state.
/// </summary>
public sealed class RewardTableRowViewModel : WishlistRelicTableRow
{
    public RewardTableRowViewModel(string tierLetter, string compactName)
    {
        TierLetterDisplay = tierLetter;
        RewardCompactName = compactName;
        DisplayName = string.IsNullOrEmpty(tierLetter) ? compactName : $"{tierLetter} {compactName}";
    }

    public override string DisplayName { get; }

    /// <summary>Tier letter without parentheses, rendered bold in the table cell.</summary>
    public string TierLetterDisplay { get; }

    /// <summary>Compact reward name, rendered after the bold tier letter.</summary>
    public string RewardCompactName { get; }
}
