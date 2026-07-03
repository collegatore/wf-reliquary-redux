## Context

The `2026-06-07-settings-tab-and-persistence` change migrated the drop-table source URL from a per-feature JSON file (`drop-table-source.settings.json` via `LocalDropTableSourceSettingsStore`) into the unified `user-settings.json` file via `LocalUserSettingsStore`. The migration was declared complete but left three categories of dead code:

1. **Dead class**: `LocalDropTableSourceSettingsStore` — a full `IDropTableSourceSettingsStore` implementation that still compiles and is tested, but is never registered in `ReliquaryServiceFactory` and writes to a file no other code reads.
2. **Dead workflow methods**: `GetSettingsAsync` / `SaveSettingsAsync` on `IDropTableSourceRefreshWorkflow` — these existed when the Drop Tables tab owned URL editing. After the URL moved to `SettingsViewModel` + `IUserSettingsStore`, no Presentation code calls them.
3. **Dead factory guard**: the `?? new LocalUserSettingsStore()` fallback in `ReliquaryServiceFactory.CreateDropTableSourceRefreshService` — this was defensive code for callers that no longer exist; `LocalUserSettingsStore` always implements `IDropTableSourceSettingsStore`.

`IDropTableSourceSettingsStore` and `DropTableSourceSettings` are **not** dead: they remain the narrow Application-layer interface that `DropTableSourceRefreshService` uses to read the configured URL at refresh time. The adapter implementation (`LocalUserSettingsStore : IUserSettingsStore, IDropTableSourceSettingsStore`) is correct and stays.

## Goals / Non-Goals

**Goals:**
- Delete `LocalDropTableSourceSettingsStore` and its dedicated file
- Remove `GetSettingsAsync` and `SaveSettingsAsync` from `IDropTableSourceRefreshWorkflow` and `DropTableSourceRefreshService`
- Remove the dead factory fallback
- Remove or migrate tests that only exercised the deleted code
- Leave zero compilation errors and all remaining tests green

**Non-Goals:**
- Changing any user-visible behaviour
- Modifying `IDropTableSourceSettingsStore`, `DropTableSourceSettings`, or the adapter logic in `LocalUserSettingsStore`
- Touching the settings-tab UI or `SettingsViewModel`
- Merging `DropTableDownload` with `DropTableSourceContent` — their distinction is correct and intentional

## Decisions

### 1. Delete `LocalDropTableSourceSettingsStore` outright

**Decision**: Hard-delete the file. No deprecation wrapper.

**Rationale**: The class is unreachable from production composition. Its only callers are two test cases that test it in isolation. Those tests add no value once the class is gone. A deprecation shim would just defer the inevitable and keep the dead path visible.

**Alternative considered**: Move the class to a test helper. Rejected — the tests themselves are redundant; `LocalUserSettingsStore` has equivalent coverage in `UserSettingsStoreTests`.

### 2. Remove `GetSettingsAsync` / `SaveSettingsAsync` from the workflow interface

**Decision**: Remove both methods from `IDropTableSourceRefreshWorkflow` and the concrete service.

**Rationale**: Zero production callers exist. The URL edit path now flows through `IUserSettingsStore` in `SettingsViewModel`. Keeping interface methods that nothing calls makes the interface misleading and inflates mock setup in unrelated tests.

**Alternative considered**: Mark as `[Obsolete]`. Rejected — we control all callers; there is no external consumer to protect.

### 3. Delete the two `DropTableInfrastructureTests` test cases

**Decision**: Delete `LocalSettingsStore_SavesUserEditableSourceUrl` and `LocalSettingsStore_UsesDefaultUrlWhenSettingsDoNotExist`.

**Rationale**: They test `LocalDropTableSourceSettingsStore` directly. Once the class is gone the tests cannot compile. Porting them to test `LocalUserSettingsStore` would duplicate the coverage already in `UserSettingsStoreTests.AsDropTableSourceSettingsStore_*`.

### 4. Simplify the factory cast

**Decision**: Replace the guard expression with a direct `(LocalUserSettingsStore)` cast — or, since the factory method already creates the store, pass it through without a conditional.

**Rationale**: The `?? new LocalUserSettingsStore()` fallback creates a second instance only when the supplied `userSettingsStore` is not a `LocalUserSettingsStore`. In production, `CreateUserSettingsStore()` always returns a `LocalUserSettingsStore`, so the fallback never fires. In tests that pass a mock, the cast returns null and a new instance is silently created — which is the wrong behaviour anyway (the mock's URL is ignored). Removing the fallback makes the contract explicit.

### 5. Unify `StoredDropTableSource` with `DropTableSourceContent`

**Decision**: Delete `StoredDropTableSource` and `DropTableSourceProvenance`. Change `IDropTableSourceStore.LoadAsync` to return `DropTableSourceContent?` and update all consumers.

**Rationale**: The store interface currently uses two different types for the same four-field entity — `DropTableSourceContent(SourceUrl, Html, DownloadedAt, LastUpdate)` on write and `StoredDropTableSource(Html, Provenance(SourceUrl, DownloadedAt, LastUpdate))` on read. The `DropTableSourceProvenance` wrapper is never passed around independently; every caller immediately drills through it (`stored.Provenance.LastUpdate`, `source.Provenance.DownloadedAt`). Flattening removes one level of indirection and makes the store interface symmetric. All consumer changes are mechanical: `stored.Provenance.X` → `stored.X` everywhere.

**Scope**: Production changes in `IDropTableSourceStore`, `LocalDropTableSourceStore`, `IDropTableParser`, `HtmlDropTableParser`, `DropTableImportService`, `DropTableSourceRefreshService`. Test changes in `DropTableSourceRefreshServiceTests`, `DropTableImportServiceTests`, `DropTableImportInfrastructureTests`, `WishlistFarmingMissionsBenchmarkTests`, `WishlistFarmingMissionsDistilledTableBenchmarkTests`, `WishlistFarmingMissionsDedupeTests`.

**What stays**: `DropTableDownload` remains separate from `DropTableSourceContent` — the download step legitimately lacks `LastUpdate` because metadata reading is a distinct application-layer step.

**Alternative considered**: Flatten `StoredDropTableSource` in place (same fields, remove `Provenance` nesting) while keeping the type name. Rejected — the type would then be structurally identical to `DropTableSourceContent`, making two names for one concept. Deleting one is cleaner.

### 6. Remove the `source_file_path` dead DB column

**Decision**: Remove `ImportMetadataEntity.SourceFilePath`, its `ReliquaryDbContext` mapping, and add an EF migration that drops the `source_file_path` column from the `import_metadata` table.

**Rationale**: The column was always written as `null` in `SqliteDropTableImportStore.ReplaceAsync` and is never read back anywhere. It was likely added as a placeholder for tracking the origin of local-file imports, but that path stores the file URI in the `source_url` column instead (`DropTableLocalFileSourceWorkflow` uses `new Uri(filePath)` as the `SourceUrl`). Retaining a nullable column that is always null adds schema noise and maintenance surface.

**Pre-release note**: The app is at `1.0.0-alpha` with no installed user base. The migration is safe to ship.

## Risks / Trade-offs

- **Existing `drop-table-source.settings.json` files** on developer machines are orphaned. They were already ignored by `LocalUserSettingsStore` (documented in the migration design doc as a known trade-off). No migration or cleanup needed.
- **`MainWindowViewModelTests.CreateWorkflow`** sets up `GetSettingsAsync` on the mock. Once the method is removed from the interface, Moq will throw at test setup. This is a compile-time break that will be caught immediately — not a silent regression.
- **Breadth of the `StoredDropTableSource` change**: six test files construct these types. All changes are mechanical find-and-replace (`new StoredDropTableSource(html, new DropTableSourceProvenance(url, at, ver))` → `new DropTableSourceContent(url, html, at, ver)`), but must be applied consistently or the build will fail. Do all six in one pass before verifying.
- **EF migration on existing developer databases**: the `source_file_path` column is always null, so the `DROP COLUMN` migration will never lose data. SQLite supports `DROP COLUMN` from version 3.35.0 (2021); the EF provider for SQLite uses the recreate-table strategy for schema changes, which is safe regardless.
