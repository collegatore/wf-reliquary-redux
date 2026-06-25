namespace WF.Reliquary.Application.Lfg;

public sealed record LfgOptionGroup(
    string Placeholder,
    IReadOnlyList<LfgOption> Options,
    string DefaultValue);
