using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Application.Lfg;

namespace WF.Reliquary.Application.Settings;

public sealed record UserSettings(
    Uri SourceUrl,
    double WindowWidth,
    double WindowHeight,
    UserFontSize FontSize,
    double? RelicsPanelHeight,
    LfgMessageTemplate LfgTemplate,
    int? WindowX = null,
    int? WindowY = null)
{
    public static UserSettings Default { get; } = new(
        SourceUrl: new Uri(DropTableSourceDefaults.SourceUrl),
        WindowWidth: 900,
        WindowHeight: 600,
        FontSize: UserFontSize.Medium,
        RelicsPanelHeight: null,
        LfgTemplate: LfgMessageTemplate.Default);
}
