## Context

All persistence lives in one SQLite file (`drop-table-import.sqlite`) accessed through per-store `ReliquaryDbExecutor` instances. Schema management drifted into the stores: `Database.MigrateAsync` runs inside four write paths (`LocalDropTableSourceStore.SaveAsync`, `SqliteDropTableImportStore` twice, `SqliteProfileStore`, `SqliteWishlistStore`), each write path also creates the database directory, and six read methods guard with `File.Exists(databasePath)` because the file may not exist yet. The `ef-core-data-layer` spec already states migrations apply "before any data access occurs" — the implementation violates this today.

The async startup seam is `MainWindowShellViewModel.InitializeAsync`, invoked once from `MainWindow`; it currently starts with `DropTables.LoadStatusAsync` (a read).

## Goals / Non-Goals

**Goals:**
- One initialization point: create directory + database file and apply all pending migrations at application startup, before any data access
- The database file always exists while the app runs; stores never create, migrate, or probe for it
- Startup initialization failures surface through the existing error-reporting path

**Non-Goals:**
- Schema changes (no new migration is introduced by this change)
- Changing the database path, file layout, or the per-store `ReliquaryDbExecutor` pattern
- Multi-instance/concurrency hardening beyond what SQLite already provides

## Decisions

### 1. `IDatabaseInitializer` port + `SqliteDatabaseInitializer` implementation

A new Application port `IDatabaseInitializer` with a single `InitializeAsync(CancellationToken)` method, implemented in Infrastructure by `SqliteDatabaseInitializer`: create the database directory if missing, then run `Database.MigrateAsync()` through a `ReliquaryDbExecutor` (which also creates the file on first connection). Like the stores, the constructor accepts an optional `databasePath` so tests can target temp files. Exposed via `ReliquaryServiceFactory.CreateDatabaseInitializer()`.

**Rationale:** Presentation must trigger initialization without referencing Infrastructure; a port matches how every other Infrastructure service is consumed.

**Alternative considered:** a static helper called from Composition before constructing view models — works, but breaks the established port/factory pattern and is harder to fake in VM tests.

### 2. Initialization runs first in `MainWindowShellViewModel.InitializeAsync`

`InitializeAsync` awaits `databaseInitializer.InitializeAsync()` before `DropTables.LoadStatusAsync` and everything else. This is the existing single async startup entry point; no new lifecycle hook is needed.

**Alternative considered:** `App.OnFrameworkInitializationCompleted` — runs earlier but is synchronous and precedes error-reporting wiring; blocking the UI thread on migration there is worse than a few unrendered milliseconds in the shell's async init.

### 3. Stores assume an initialized database (invariant, not defensive checks)

All `File.Exists(databasePath)` guards, `Directory.CreateDirectory` calls, and `Database.MigrateAsync` calls are removed from stores and readers. After startup the schema is guaranteed current; a query against a fresh database returns empty results, which is behaviorally identical to today's "file absent → null/empty" short-circuits. Deleting the file mid-session was never supported and now fails loudly via SQLite errors routed to the error reporter.

**Rationale:** One invariant beats seven scattered defenses; every future store method gets correctness for free.

### 4. Initialization failure: report and continue

If `InitializeAsync` throws, the shell reports it via `IErrorReporter` and continues starting up. Subsequent store calls will fail and be reported through the same paths that already handle store exceptions. No retry logic, no blocking dialog.

**Rationale:** A migration failure on a single-user desktop app is unrecoverable without user action (e.g., corrupted file); a clear error message beats a crash or a silent hang. Matches the existing "report, don't die" convention.

### 5. Tests initialize a temp database explicitly

Store/reader tests that previously exercised "DB absent" branches switch to running `SqliteDatabaseInitializer` (or an equivalent helper) against a temp path in setup. The "absent database" behaviors are no longer requirements, so those test cases are removed rather than rewritten; "empty database" cases replace them where the distinction mattered.

## Risks / Trade-offs

- **Startup cost**: `MigrateAsync` on every launch → negligible; after the first run it is a single `__EFMigrationsHistory` check on a local file, and it runs on a thread-pool thread via the executor.
- **File deleted while running** → store calls now throw instead of silently returning empty; errors surface via `IErrorReporter`. Acceptable: this was never a supported scenario.
- **A future destructive migration runs unconditionally at startup** → that is the point: predictable timing, before any read. Destructive migrations still need their own care (as specced in `drop-tables-import-source-selector`).
- **Downstream change overlap**: `drop-tables-import-source-selector` currently specs read-path migration awareness → its design/tasks are updated to depend on this change instead (its read methods become plain queries).

## Migration Plan

1. Add port, implementation, factory method, and shell wiring (initializer runs; stores unchanged — double migration is harmless because `MigrateAsync` is idempotent).
2. Strip `MigrateAsync` / `Directory.CreateDirectory` / `File.Exists` from stores and readers; update tests.
3. `drop-tables-import-source-selector` is rebased on this change (implement this one first).

No schema migration and no rollback concerns: behavior-only refactor.
