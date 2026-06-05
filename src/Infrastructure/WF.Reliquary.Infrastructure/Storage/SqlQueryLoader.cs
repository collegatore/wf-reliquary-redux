using System.Reflection;

namespace WF.Reliquary.Infrastructure.Storage;

internal static class SqlQueryLoader
{
    private static readonly Assembly Assembly = typeof(SqlQueryLoader).Assembly;
    private static readonly string Prefix = $"{Assembly.GetName().Name}.Storage.Queries.";

    internal static string Load(string fileName)
    {
        var resourceName = Prefix + fileName;
        using var stream = Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Embedded SQL resource '{resourceName}' not found. " +
                $"Available: {string.Join(", ", Assembly.GetManifestResourceNames())}");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
