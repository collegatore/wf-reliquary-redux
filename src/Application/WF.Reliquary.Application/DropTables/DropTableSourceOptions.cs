namespace WF.Reliquary.Application.DropTables;

public sealed record DropTableSourceOptions(Uri SourceUrl, string FileName)
{
    public static DropTableSourceOptions CreateDefault()
    {
        return DropTableSourceDefaults.CreateOptions();
    }
}
