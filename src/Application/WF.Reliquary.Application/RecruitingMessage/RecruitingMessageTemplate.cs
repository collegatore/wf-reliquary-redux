namespace WF.Reliquary.Application.RecruitingMessage;

public sealed record RecruitingMessageTemplate(string Pattern, IReadOnlyList<RecruitingMessageOptionGroup> Groups)
{
    public static RecruitingMessageTemplate Default { get; } = new(
        Pattern: "{participation} [{relic_name}] {refinement}",
        Groups:
        [
            new RecruitingMessageOptionGroup(
                Placeholder: "participation",
                Options:
                [
                    new RecruitingMessageOption("Hosting", "H"),
                    new RecruitingMessageOption("Looking for group", "LF"),
                ],
                DefaultValue: "LF"),
            new RecruitingMessageOptionGroup(
                Placeholder: "refinement",
                Options:
                [
                    new RecruitingMessageOption("Intact", "inta"),
                    new RecruitingMessageOption("Exceptional", "exceptional"),
                    new RecruitingMessageOption("Flawless", "flawless"),
                    new RecruitingMessageOption("Radiant", "rad"),
                ],
                DefaultValue: "rad"),
        ]);
}
