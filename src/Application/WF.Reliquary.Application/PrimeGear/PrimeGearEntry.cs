namespace WF.Reliquary.Application.PrimeGear;

public sealed record PrimeGearEntry(string GearName, IReadOnlyList<string> Parts, bool IsVaulted);
