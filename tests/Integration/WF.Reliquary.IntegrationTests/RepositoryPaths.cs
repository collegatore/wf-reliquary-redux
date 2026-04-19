namespace WF.Reliquary.IntegrationTests;

internal static class RepositoryPaths
{
    public static string Root => FindRoot(AppContext.BaseDirectory);

    private static string FindRoot(string startPath)
    {
        var directory = new DirectoryInfo(startPath);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "WF.Reliquary.Redux.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root from test output directory.");
    }
}
