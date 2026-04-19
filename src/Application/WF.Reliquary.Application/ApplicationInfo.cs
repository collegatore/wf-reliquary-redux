namespace WF.Reliquary.Application;

public static class ApplicationInfo
{
    public const string Name = "WF Reliquary Redux";
    public const string Version = "0.1.0-boilerplate";

    public static string GetStartupSummary()
    {
        return $"{Name} {Version}";
    }
}
