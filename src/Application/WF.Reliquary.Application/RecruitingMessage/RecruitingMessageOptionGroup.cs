namespace WF.Reliquary.Application.RecruitingMessage;

public sealed record RecruitingMessageOptionGroup(
    string Placeholder,
    IReadOnlyList<RecruitingMessageOption> Options,
    string DefaultValue);
