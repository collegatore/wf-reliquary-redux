using System.Reflection;

namespace WF.Reliquary.Application;

public static class ApplicationInfo
{
    public const string Name = "WF Reliquary Redux";

    public static string Version { get; } = ReadVersion();

    public static string GetStartupSummary()
    {
        return $"{Name} {Version}";
    }

    private static string ReadVersion()
    {
        var raw = typeof(ApplicationInfo).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ?? "0.0.0";
        var plusIndex = raw.IndexOf('+');
        return plusIndex >= 0 ? raw[..plusIndex] : raw;
    }
}
