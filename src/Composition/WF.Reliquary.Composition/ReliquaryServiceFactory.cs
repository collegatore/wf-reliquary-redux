using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Infrastructure.DropTables;
using WF.Reliquary.Infrastructure.Logging;
using WF.Reliquary.Infrastructure.Settings;
using WF.Reliquary.Infrastructure.Shell;
using WF.Reliquary.Infrastructure.Storage;

namespace WF.Reliquary.Composition;

public static class ReliquaryServiceFactory
{
    public static IUserSettingsStore CreateUserSettingsStore()
    {
        return new LocalUserSettingsStore();
    }

    public static IDropTableSourceRefreshWorkflow CreateDropTableSourceRefreshService(IUserSettingsStore? userSettingsStore = null)
    {
        var settingsStore = (IDropTableSourceSettingsStore)(userSettingsStore ?? new LocalUserSettingsStore());

        return new DropTableSourceRefreshService(
            new HttpDropTableSourceClient(new HttpClient()),
            new LocalDropTableSourceStore(),
            new HtmlDropTableMetadataReader(),
            settingsStore);
    }

    public static IDropTableImportWorkflow CreateDropTableImportService()
    {
        return new DropTableImportService(
            new LocalDropTableSourceStore(),
            new HtmlDropTableParser(),
            new SqliteDropTableImportStore());
    }

    public static IDropTableImportStore CreateDropTableImportStore()
    {
        return new SqliteDropTableImportStore();
    }

    public static IDropTableLocalFileSourceWorkflow CreateDropTableLocalFileSourceWorkflow()
    {
        return new DropTableLocalFileSourceWorkflow(
            new LocalDropTableSourceStore(),
            new HtmlDropTableMetadataReader());
    }

    public static IDropTableSourceStore CreateDropTableSourceStore()
    {
        return new LocalDropTableSourceStore();
    }

    public static IDropTableSourceExporter CreateDropTableSourceExporter()
    {
        return new LocalDropTableSourceExporter();
    }

    public static IPrimeGearReader CreatePrimeGearReader()
    {
        return new SqlitePrimeGearReader();
    }

    public static IProfileStore CreateProfileStore()
    {
        return new SqliteProfileStore();
    }

    public static IWishlistStore CreateWishlistStore()
    {
        return new SqliteWishlistStore();
    }

    public static IOwnedGearStore CreateOwnedGearStore()
    {
        return new SqliteOwnedGearStore();
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

    public static IDatabaseInitializer CreateDatabaseInitializer()
    {
        return new SqliteDatabaseInitializer();
    }

    public static IErrorLog CreateErrorLog()
    {
        return new FileErrorLog();
    }

    public static IFileLauncher CreateFileLauncher()
    {
        return new ShellFileLauncher();
    }
}
