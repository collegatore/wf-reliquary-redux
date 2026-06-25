namespace WF.Reliquary.Application.Lfg;

public sealed record LfgMessageTemplate(string Pattern, IReadOnlyList<LfgOptionGroup> Groups)
{
    public static LfgMessageTemplate Default { get; } = new(
        Pattern: "{participation} [{relic_name}] {refinement}",
        Groups:
        [
            new LfgOptionGroup(
                Placeholder: "participation",
                Options:
                [
                    new LfgOption("Hosting", "H"),
                    new LfgOption("Looking for group", "LF"),
                ],
                DefaultValue: "LF"),
            new LfgOptionGroup(
                Placeholder: "refinement",
                Options:
                [
                    new LfgOption("Intact", "inta"),
                    new LfgOption("Exceptional", "exceptional"),
                    new LfgOption("Flawless", "flawless"),
                    new LfgOption("Radiant", "rad"),
                ],
                DefaultValue: "rad"),
        ]);
}
