using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Infrastructure.DropTables;
using WF.Reliquary.Infrastructure.Settings;


namespace WF.Reliquary.Composition;

public static class ReliquaryServiceFactory
{
    public static IUserSettingsStore CreateUserSettingsStore()
    {
        return new LocalUserSettingsStore();
    }

    public static IDropTableSourceRefreshWorkflow CreateDropTableSourceRefreshService(IUserSettingsStore? userSettingsStore = null)
    {
        var options = DropTableSourceOptions.CreateDefault();
        var settingsStore = (userSettingsStore as IDropTableSourceSettingsStore)
            ?? new LocalUserSettingsStore();

        return new DropTableSourceRefreshService(
            options,
            new HttpDropTableSourceClient(new HttpClient()),
            new LocalDropTableSourceStore(),
            new HtmlDropTableMetadataReader(),
            settingsStore);
    }

    public static IDropTableImportWorkflow CreateDropTableImportService()
    {
        var options = DropTableSourceOptions.CreateDefault();

        return new DropTableImportService(
            options,
            new LocalDropTableSourceStore(),
            new HtmlDropTableParser(),
            new SqliteDropTableImportStore());
    }

    public static IPrimeGearReader CreatePrimeGearReader()
    {
        return new SqlitePrimeGearReader();
    }

    public static IWishlistStore CreateWishlistStore()
    {
        return new SqliteWishlistStore();
    }

    public static IWishlistRelicsReader CreateWishlistRelicsReader()
    {
        return new SqliteWishlistRelicsReader();
    }

    public static IImportMetadataReader CreateImportMetadataReader()
    {
        return new SqliteImportMetadataReader();
    }

    public static IWishlistFarmingMissionsReader CreateWishlistFarmingMissionsReader()
    {
        return new SqliteWishlistFarmingMissionsReader();
    }
}
