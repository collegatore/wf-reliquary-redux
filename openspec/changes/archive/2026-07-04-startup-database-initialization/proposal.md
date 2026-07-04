## Why

The `ef-core-data-layer` spec already requires that migrations are applied "before any data access occurs", but the implementation drifted: `Database.MigrateAsync` is scattered across four write paths, the database file is created lazily on first write, and six read methods guard against a missing file with `File.Exists`. Consequences: reads can observe an unmigrated schema, destructive migrations fire at an unpredictable moment (whichever store writes first), and every new store method must remember the guard/migrate dance. The pending `drop-tables-import-source-selector` change adds a data-deleting deduplication migration and new read paths, which makes fixing this a precondition.

## What Changes

- **Single startup initialization step**: A database initializer creates the database directory and file (if missing) and applies all pending EF Core migrations once, during application startup, before any view model loads data.
- **Lazy creation removed**: The database file always exists while the app is running. Stores no longer create directories or call `MigrateAsync` in write paths.
- **File-existence guards removed**: Read methods no longer check `File.Exists`; they query the (possibly empty) tables directly and rely on the startup invariant that the schema is current.
- **Initialization failure is reported**: If startup migration fails, the error is surfaced through the existing error-reporting mechanism.

## Capabilities

### New Capabilities

_None._

### Modified Capabilities

- `ef-core-data-layer`: The "Schema is managed by EF Core migrations" requirement is strengthened — database creation and migration happen at a single application-startup point; stores are forbidden from applying migrations, creating the database, or probing file existence.

## Impact

- **Application/Ports** — new `IDatabaseInitializer` port with `InitializeAsync(CancellationToken)`
- **Infrastructure** — new `SqliteDatabaseInitializer` implementation (create directory, `Database.MigrateAsync` once); remove `MigrateAsync` + `Directory.CreateDirectory` from `LocalDropTableSourceStore`, `SqliteDropTableImportStore`, `SqliteProfileStore`, `SqliteWishlistStore`; remove `File.Exists` guards from `LocalDropTableSourceStore`, `SqliteImportMetadataReader`, `SqlitePrimeGearReader`, `SqliteWishlistRelicsReader`, `SqliteWishlistFarmingMissionsReader`
- **Composition** — `ReliquaryServiceFactory.CreateDatabaseInitializer()`
- **Presentation** — `MainWindowShellViewModel.InitializeAsync` runs the initializer first and reports failure via `IErrorReporter`
- **Tests** — store/reader tests that relied on "DB absent" behavior switch to initializing a temp database first; new initializer tests (fresh create, pending migrations applied, idempotent re-run)
- **Downstream** — `drop-tables-import-source-selector` simplifies: its read methods no longer need migration awareness (its design/tasks will be updated to reference this change)
