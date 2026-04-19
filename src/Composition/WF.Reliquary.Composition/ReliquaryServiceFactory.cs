using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Infrastructure.DropTables;

namespace WF.Reliquary.Composition;

public static class ReliquaryServiceFactory
{
    public static IDropTableSourceRefreshWorkflow CreateDropTableSourceRefreshService()
    {
        var options = DropTableSourceOptions.CreateDefault();

        return new DropTableSourceRefreshService(
            options,
            new HttpDropTableSourceClient(new HttpClient()),
            new LocalDropTableSourceStore(),
            new HtmlDropTableMetadataReader(),
            new LocalDropTableSourceSettingsStore());
    }
}
