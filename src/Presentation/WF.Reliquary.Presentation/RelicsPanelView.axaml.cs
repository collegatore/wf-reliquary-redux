using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;

namespace WF.Reliquary.Presentation;

/// <summary>
/// The relics panel — control bar (Relics label, Filters / sort / Display controls)
/// plus the relic-cards scroll area — shared by both the stacked and side-by-side
/// layouts of <see cref="RelicsMissionsView"/>. Extracted so its content (and the
/// per-view-mode card subtrees) is defined once rather than duplicated per layout.
/// Layout/splitter logic stays in <see cref="RelicsMissionsView"/>'s code-behind.
/// </summary>
public sealed partial class RelicsPanelView : UserControl
{
    public RelicsPanelView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        // Table mode's TreeDataGrid source (columns + hierarchy) is built in code, per
        // the fork's API. It reads the VM's TableRows collection, so populating that on
        // reload flows through automatically (no rebuild needed here).
        if (DataContext is MainWindowShellViewModel shell)
            RelicsTable.Source = BuildTableSource(shell.WishlistRelics.TableRows);
    }

    private static ITreeDataGridSource BuildTableSource(IList<WishlistRelicTableRow> rows)
    {
        var source = new HierarchicalTreeDataGridSource<WishlistRelicTableRow>(rows)
        {
            Columns =
            {
                // Expander + clipped relic name (child rows show bold tier letter + reward name).
                new HierarchicalExpanderColumn<WishlistRelicTableRow>(
                    new TemplateColumn<WishlistRelicTableRow>(
                        null,
                        new FuncDataTemplate<WishlistRelicTableRow>((row, _) =>
                            row is RewardTableRowViewModel reward
                                ? BuildRewardNameCell(reward)
                                : BuildTextCell(row?.DisplayName, row),
                            supportsRecycling: false),
                        width: new GridLength(1, GridUnitType.Star)),
                    childSelector: row => row.Children),

                // Wishlisted-tier markers, e.g. "(C, R)" — blank on child rows.
                new TemplateColumn<WishlistRelicTableRow>(
                    null,
                    new FuncDataTemplate<WishlistRelicTableRow>((row, _) => BuildTextCell(row?.TierMarkers, row), supportsRecycling: false),
                    width: GridLength.Auto),

                // Rewards modal pictogram (relic rows only).
                new TemplateColumn<WishlistRelicTableRow>(
                    null,
                    new FuncDataTemplate<WishlistRelicTableRow>((row, _) => BuildPictogramCell(row, "Icon.Table.Rewards", "View this relic's rewards", static r => r.ShowRewardsCommand), supportsRecycling: false),
                    width: GridLength.Auto),

                // Recruiting Chat message composer pictogram (relic rows only).
                new TemplateColumn<WishlistRelicTableRow>(
                    null,
                    new FuncDataTemplate<WishlistRelicTableRow>((row, _) => BuildPictogramCell(row, "Icon.Table.Recruit", "Compose Recruiting Chat message…", static r => r.ComposeRecruitingMessageCommand), supportsRecycling: false),
                    width: GridLength.Auto),

                // Scrollbar gutter — 18 px matches the Fluent theme scrollbar width so
                // the vertical scrollbar never overlays the composer pictogram column.
                new TemplateColumn<WishlistRelicTableRow>(
                    null,
                    new FuncDataTemplate<WishlistRelicTableRow>((_, _) => new Panel(), supportsRecycling: false),
                    width: new GridLength(18)),
            },
        };

        return source;
    }

    private static Control BuildTextCell(string? text, WishlistRelicTableRow? row) =>
        new TextBlock
        {
            Text = text ?? string.Empty,
            Opacity = row?.RowOpacity ?? 1.0,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 2, 8, 2),
        };

    // Child reward rows: bold tier letter followed by normal-weight compact name.
    private static Control BuildRewardNameCell(RewardTableRowViewModel row)
    {
        var tb = new TextBlock
        {
            Opacity = row.RowOpacity,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 2, 8, 2),
        };
        if (!string.IsNullOrEmpty(row.TierLetterDisplay))
        {
            tb.Inlines!.Add(new Run(row.TierLetterDisplay) { FontWeight = FontWeight.Bold });
            tb.Inlines.Add(new Run(" " + row.RewardCompactName));
        }
        else
        {
            tb.Text = row.RewardCompactName;
        }
        return tb;
    }

    private static Control BuildPictogramCell(WishlistRelicTableRow? row, string iconKey, string tooltip, Func<WishlistRelicTableRow, ICommand?> commandSelector)
    {
        // Pictogram cells are empty on child reward rows.
        if (row is not { ShowsActions: true })
            return new Panel();

        // The icon inherits the row's text color; the button fades with vaulted rows like
        // the text cells. The table-action class dims the icon until hover (App.axaml).
        var icon = new PathIcon();
        icon.Bind(PathIcon.DataProperty, icon.GetResourceObservable(iconKey));
        var button = new Button
        {
            Classes = { "table-action" },
            Opacity = row.RowOpacity,
            Content = icon,
            Command = commandSelector(row),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(6, 2),
            VerticalAlignment = VerticalAlignment.Center,
        };
        ToolTip.SetTip(button, tooltip);
        return button;
    }
}
