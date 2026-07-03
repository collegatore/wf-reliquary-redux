using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Application.RecruitingMessage;

namespace WF.Reliquary.Application.Settings;

public sealed record UserSettings(
    Uri SourceUrl,
    double WindowWidth,
    double WindowHeight,
    double FontSizePt,
    double? RelicsPanelHeight,
    RecruitingMessageTemplate RecruitingMessageTemplate,
    int? WindowX = null,
    int? WindowY = null,
    RelicsMissionsOrientation Orientation = RelicsMissionsOrientation.SideBySide,
    double? SideBySideRelicsWidth = null,
    double RelicCardWidthPx = 200.0,
    RelicViewMode ViewMode = RelicViewMode.Compact,
    RelicSortMode SortMode = RelicSortMode.Era,
    bool SortDescending = false)
{
    public static UserSettings Default { get; } = new(
        SourceUrl: new Uri(DropTableSourceDefaults.SourceUrl),
        WindowWidth: 1280,
        WindowHeight: 800,
        FontSizePt: 14.0,
        RelicsPanelHeight: null,
        RecruitingMessageTemplate: RecruitingMessageTemplate.Default);
}
