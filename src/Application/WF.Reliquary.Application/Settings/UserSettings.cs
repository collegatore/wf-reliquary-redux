using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Application.Lfg;

namespace WF.Reliquary.Application.Settings;

public sealed record UserSettings(
    Uri SourceUrl,
    double WindowWidth,
    double WindowHeight,
    double FontSizePt,
    double? RelicsPanelHeight,
    LfgMessageTemplate LfgTemplate,
    int? WindowX = null,
    int? WindowY = null,
    RelicsMissionsOrientation Orientation = RelicsMissionsOrientation.Stacked,
    double? SideBySideRelicsWidth = null,
    double RelicCardWidthPx = 300.0,
    RelicViewMode ViewMode = RelicViewMode.Compact)
{
    public static UserSettings Default { get; } = new(
        SourceUrl: new Uri(DropTableSourceDefaults.SourceUrl),
        WindowWidth: 900,
        WindowHeight: 600,
        FontSizePt: 14.0,
        RelicsPanelHeight: null,
        LfgTemplate: LfgMessageTemplate.Default);
}
