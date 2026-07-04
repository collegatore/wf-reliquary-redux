## 1. Port and Implementation

- [x] 1.1 Add `IDatabaseInitializer` port to Application (`InitializeAsync(CancellationToken cancellationToken = default) -> Task`)
- [x] 1.2 Implement `SqliteDatabaseInitializer` in Infrastructure: create database directory if missing, run `Database.MigrateAsync` via `ReliquaryDbExecutor`; constructor takes optional `databasePath` (defaults like the stores)
- [x] 1.3 Add `ReliquaryServiceFactory.CreateDatabaseInitializer()` to Composition

## 2. Startup Wiring

- [x] 2.1 Inject `IDatabaseInitializer` into `MainWindowShellViewModel`; update construction call sites
- [x] 2.2 Await `databaseInitializer.InitializeAsync()` at the top of `MainWindowShellViewModel.InitializeAsync`, before `DropTables.LoadStatusAsync`
- [x] 2.3 Wrap the initializer call in try/catch: report failure via `IErrorReporter` and continue startup

## 3. Remove Scattered Schema Management

- [x] 3.1 `LocalDropTableSourceStore`: remove `Database.MigrateAsync` + `Directory.CreateDirectory` from `SaveAsync`; remove `File.Exists` guards from `LoadAsync` and `HasAnyAsync`
- [x] 3.2 `SqliteDropTableImportStore`: remove both `Database.MigrateAsync` + `Directory.CreateDirectory` occurrences
- [x] 3.3 `SqliteProfileStore`: remove `Database.MigrateAsync` + `Directory.CreateDirectory`
- [x] 3.4 `SqliteWishlistStore`: remove `Database.MigrateAsync` + `Directory.CreateDirectory`
- [x] 3.5 Remove `File.Exists` guards from `SqliteImportMetadataReader`, `SqlitePrimeGearReader`, `SqliteWishlistRelicsReader`, `SqliteWishlistFarmingMissionsReader`
- [x] 3.6 Sweep Infrastructure for any remaining `MigrateAsync` / `File.Exists(databasePath)` occurrences outside `SqliteDatabaseInitializer`

## 4. Tests

- [x] 4.1 Unit tests for `SqliteDatabaseInitializer`: fresh path creates directory + file with full schema; outdated database gets pending migrations applied; re-run on up-to-date database is a no-op (idempotent)
- [x] 4.2 Update store/reader tests: replace "DB absent returns null/empty/false" cases with "empty database returns null/empty/false" cases, initializing a temp database in setup via `SqliteDatabaseInitializer`
- [x] 4.3 Update `MainWindowShellViewModel` tests: initializer runs before data loads; initializer failure is reported via `IErrorReporter` and startup continues
- [x] 4.4 Run full test suite and build to confirm no store still depends on lazy creation
