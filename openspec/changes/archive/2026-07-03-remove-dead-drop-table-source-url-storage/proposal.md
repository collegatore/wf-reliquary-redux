## Why

The `2026-06-07-settings-tab-and-persistence` migration unified URL storage into `user-settings.json` but left behind a dead `LocalDropTableSourceSettingsStore` class, two orphaned tests that exercise it, two unreachable methods on `IDropTableSourceRefreshWorkflow`, and a defensive factory fallback that can never trigger in production. A broader audit of the drop-table download subsystem uncovered two additional issues: the store interface's save/load types are asymmetric representations of the same four-field entity, introducing a pointless `DropTableSourceProvenance` indirection layer; and the `import_metadata.source_file_path` DB column has never been populated and is not read anywhere. All of these add noise without providing value.

## What Changes

- **Delete** `LocalDropTableSourceSettingsStore` (Infrastructure) — the old per-feature settings class that writes to `drop-table-source.settings.json`; `LocalUserSettingsStore` is the production store
- **Remove** `GetSettingsAsync` and `SaveSettingsAsync` from `IDropTableSourceRefreshWorkflow` and their implementation in `DropTableSourceRefreshService` — these methods have zero callers in Presentation since the URL moved to `SettingsViewModel` + `IUserSettingsStore`
- **Remove** the two test cases in `DropTableInfrastructureTests.cs` that instantiate `LocalDropTableSourceSettingsStore` directly (`LocalSettingsStore_SavesUserEditableSourceUrl`, `LocalSettingsStore_UsesDefaultUrlWhenSettingsDoNotExist`)
- **Remove** the unit test for `SaveSettingsAsync` in `DropTableSourceRefreshServiceTests`
- **Remove** the `GetSettingsAsync` mock setup from `MainWindowViewModelTests.CreateWorkflow` (it no longer appears on the interface)
- **Simplify** the factory cast in `ReliquaryServiceFactory.CreateDropTableSourceRefreshService` — remove the `?? new LocalUserSettingsStore()` fallback that was guarding against callers that no longer exist
- **Delete** `StoredDropTableSource` and `DropTableSourceProvenance` — the store's read output carries the same four fields as its write input (`DropTableSourceContent`); change `IDropTableSourceStore.LoadAsync` to return `DropTableSourceContent?` directly and update all consumers (`DropTableSourceRefreshService`, `DropTableImportService`, `HtmlDropTableParser`, `IDropTableParser`, `LocalDropTableSourceStore`) and the six test files that construct these types
- **Remove** the `source_file_path` column from `ImportMetadataEntity` and the SQLite schema — always written as `null`, never read, and mapped to a nullable column that carries no information

## Capabilities

### New Capabilities
<!-- None — this is a dead-code removal, no new capabilities are introduced -->

### Modified Capabilities
<!-- No spec-level requirements change. The removed workflow methods were never part of
     any user-facing scenario and are not referenced in drop-table-source-refresh spec. -->

## Impact

- **Deleted file**: `src/Infrastructure/WF.Reliquary.Infrastructure/DropTables/LocalDropTableSourceSettingsStore.cs`
- **Deleted files**: `LocalDropTableSourceSettingsStore.cs`, `StoredDropTableSource.cs`, `DropTableSourceProvenance.cs`
- **Modified interfaces**: `IDropTableSourceRefreshWorkflow` (shrinks by two methods), `IDropTableSourceStore` (return type change), `IDropTableParser` (parameter type change)
- **Modified implementations**: `DropTableSourceRefreshService`, `ReliquaryServiceFactory`, `LocalDropTableSourceStore`, `HtmlDropTableParser`, `DropTableImportService`, `ImportMetadataEntity`, `ReliquaryDbContext`, `SqliteDropTableImportStore`
- **New EF migration**: drops `source_file_path` column from `import_metadata` table
- **Modified tests**: `DropTableInfrastructureTests`, `DropTableSourceRefreshServiceTests`, `MainWindowViewModelTests`, `DropTableImportServiceTests`, `DropTableImportInfrastructureTests`, `WishlistFarmingMissionsBenchmarkTests`, `WishlistFarmingMissionsDistilledTableBenchmarkTests`, `WishlistFarmingMissionsDedupeTests`
- **No behavior change**: all user-facing functionality is unchanged; this is mechanical type renaming and dead code removal throughout
