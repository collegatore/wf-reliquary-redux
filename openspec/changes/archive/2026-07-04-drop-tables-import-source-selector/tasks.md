## 1. Application Layer — Data Types

- [x] 1.1 Add `DropTableSourceSummary` record (`Id: long`, `SourceUrl: string`, `DownloadedAt: DateTimeOffset`, `LastUpdate: string`) to Application layer
- [x] 1.2 Add `DuplicateVersion` factory/state to `DropTableRefreshResult` carrying `DropTableSourceContent Pending` and `long ExistingId`; the state has `Succeeded = false`, `ErrorMessage = null`, and an `IsDuplicateVersion` property (true when `Pending` is non-null)
- [x] 1.3 Add `DuplicateVersion` factory/state to `DropTableFileRegistrationResult` carrying `DropTableSourceContent Pending` and `long ExistingId`; same semantics as 1.2 (`Succeeded = false`, `ErrorMessage = null`, `IsDuplicateVersion` discriminator)

## 2. Application Layer — Store Interface

- [x] 2.1 Add `ListSummariesAsync(CancellationToken) -> Task<IReadOnlyList<DropTableSourceSummary>>` to `IDropTableSourceStore`
- [x] 2.2 Add `FindSummaryByLastUpdateAsync(string lastUpdate, CancellationToken) -> Task<DropTableSourceSummary?>` to `IDropTableSourceStore`
- [x] 2.3 Add `LoadByIdAsync(long id, CancellationToken) -> Task<DropTableSourceContent?>` to `IDropTableSourceStore`
- [x] 2.4 Add `ReplaceAsync(long existingId, DropTableSourceContent newContent, CancellationToken) -> Task` to `IDropTableSourceStore`

## 3. Application Layer — Workflow Contracts

- [x] 3.1 Change `IDropTableImportWorkflow.ImportAsync` signature to `ImportAsync(long sourceId, CancellationToken cancellationToken = default)`
- [x] 3.2 Add `ConfirmSaveAsync(DropTableSourceContent pending, long existingId, CancellationToken) -> Task<DropTableRefreshResult>` to `IDropTableSourceRefreshWorkflow`
- [x] 3.3 Add `ConfirmSaveAsync(DropTableSourceContent pending, long existingId, CancellationToken) -> Task<DropTableFileRegistrationResult>` to `IDropTableLocalFileSourceWorkflow`

## 4. Application Layer — Service Implementations

- [x] 4.1 Update `DropTableSourceRefreshService.RefreshAsync`: after extracting `lastUpdate`, call `FindSummaryByLastUpdateAsync`; if duplicate, return `DuplicateVersion` result without calling `SaveAsync`
- [x] 4.2 Implement `DropTableSourceRefreshService.ConfirmSaveAsync`: call `sourceStore.ReplaceAsync(existingId, pending)`, then `GetStatusAsync`, return success result; wrap in the same try/catch translation as `RefreshAsync` so a not-found `existingId` (store throws) becomes a failure result
- [x] 4.3 Update `DropTableLocalFileSourceWorkflow.RegisterFromFileAsync`: after extracting `lastUpdate`, call `FindSummaryByLastUpdateAsync`; if duplicate, return `DuplicateVersion` result without calling `SaveAsync`
- [x] 4.4 Implement `DropTableLocalFileSourceWorkflow.ConfirmSaveAsync`: call `sourceStore.ReplaceAsync(existingId, pending)`, return success result; catch store failures (not-found `existingId`) and return a failure result
- [x] 4.5 Update `DropTableImportService.ImportAsync`: call `sourceStore.LoadByIdAsync(sourceId)` instead of `LoadAsync()`; return failure when `null`

## 5. Infrastructure Layer — Store Implementation

> Precondition: the `startup-database-initialization` change is implemented first — the database exists and is fully migrated before any store method runs, so the new methods are plain queries.

- [x] 5.1 Implement `LocalDropTableSourceStore.ListSummariesAsync`: query `source_files` ordered by `id DESC`, project to `DropTableSourceSummary` (no content column); empty database yields an empty list
- [x] 5.2 Implement `LocalDropTableSourceStore.FindSummaryByLastUpdateAsync`: query by `source_last_update`, project to `DropTableSourceSummary?`; return `null` when no match
- [x] 5.3 Implement `LocalDropTableSourceStore.LoadByIdAsync`: query by `id`, map to `DropTableSourceContent?`; return `null` when not found
- [x] 5.4 Implement `LocalDropTableSourceStore.ReplaceAsync`: within a single transaction, delete the row with `existingId` and insert the new `DropTableSourceContent`; throw if `existingId` not found (caller translates to a failure result)

## 6. Infrastructure Layer — Migration

- [x] 6.1 Add EF Core migration `AddSourceFilesUniqueVersionIndex`
- [x] 6.2 Migration SQL step: `DELETE FROM source_files WHERE id NOT IN (SELECT MAX(id) FROM source_files GROUP BY source_last_update)`
- [x] 6.3 Migration index step: `CreateIndex("IX_source_files_source_last_update", "source_files", "source_last_update", unique: true)`
- [x] 6.4 Update `ReliquaryDbContext` / `ModelSnapshot` to reflect the new unique index on `SourceFileEntity`

## 7. Presentation Layer — View Model

- [x] 7.1 Add `AvailableSources` (`ObservableCollection<DropTableSourceSummary>`) and `SelectedSource` (`DropTableSourceSummary?`) observable properties to `MainWindowViewModel`
- [x] 7.2 Add `RefreshSourceListAsync` helper: calls `sourceStore.ListSummariesAsync`, repopulates `AvailableSources`, sets `SelectedSource` to first item or `null`
- [x] 7.3 Call `RefreshSourceListAsync` inside `LoadStatusAsync`
- [x] 7.4 Change `ImportCommand` can-execute to `!IsBusy && SelectedSource != null`; add `OnSelectedSourceChanged` partial to call `ImportCommand.NotifyCanExecuteChanged()`
- [x] 7.5 Update `ImportAsync` to pass `SelectedSource!.Id` to `importWorkflow.ImportAsync(sourceId)`
- [x] 7.6 In `RefreshAsync` result handling, branch on `IsDuplicateVersion` **before** the failure branch (a duplicate must not reach the error reporter): call `ConfirmOverwrite(existingSummary)` delegate; if confirmed call `refreshWorkflow.ConfirmSaveAsync(pending, existingId)` and report its failure (if any) as an error; after any successful save (new or confirmed overwrite) call `RefreshSourceListAsync`; on cancel, do not refresh
- [x] 7.7 In `ReadFromFileAsync` result handling, apply the same `IsDuplicateVersion`-before-failure branching via `localFileWorkflow.ConfirmSaveAsync`; call `RefreshSourceListAsync` after any successful save
- [x] 7.8 Add `Func<DropTableSourceSummary, Task<bool>>? ConfirmOverwrite` delegate property (wired by `MainWindow`)
- [x] 7.9 Add `OpenSourceBrowserCommand` (`IAsyncRelayCommand`) that invokes `OpenSourceBrowserAsync` delegate with current `AvailableSources`
- [x] 7.10 Add `Func<IReadOnlyList<DropTableSourceSummary>, Task>? OpenSourceBrowserAsync` delegate property (wired by `MainWindow`)
- [x] 7.11 Inject `IDropTableSourceStore` into `MainWindowViewModel` (for `RefreshSourceListAsync`); update constructor and `ReliquaryServiceFactory` wiring

## 8. Presentation Layer — Dialogs and Windows

- [x] 8.1 Create `ConfirmSourceOverwriteDialog` Avalonia Window: display existing record's version and `DownloadedAt`; OK/Cancel returns `bool` via `ShowDialog<bool>(owner)`; add static `ShowAsync(Window owner, DropTableSourceSummary existing)` helper
- [x] 8.2 Wire `ConfirmOverwrite` delegate in `MainWindow` to call `ConfirmSourceOverwriteDialog.ShowAsync`
- [x] 8.3 Create `SourceBrowserWindow` Avalonia Window: DataGrid with columns Version, Source (URL or filename), Downloaded; read-only; add static `ShowAsync(Window owner, IReadOnlyList<DropTableSourceSummary> sources)` helper
- [x] 8.4 Wire `OpenSourceBrowserAsync` delegate in `MainWindow` to call `SourceBrowserWindow.ShowAsync`

## 9. Presentation Layer — AXAML

- [x] 9.1 In `DropTablesView.axaml` Import section: add a horizontal row containing a `ComboBox` (bound to `DropTables.AvailableSources` / `DropTables.SelectedSource`) and a pictogram `Button` (bound to `DropTables.OpenSourceBrowserCommand`) next to it
- [x] 9.2 Set ComboBox `DisplayMemberBinding` or `ItemTemplate` to show `LastUpdate` only; use `"(version unknown)"` fallback for empty values
- [x] 9.3 Disable ComboBox and pictogram button when `AvailableSources` is empty (bind `IsEnabled`)
- [x] 9.4 Move the Import and Clear buttons below the selector row (layout unchanged except for the new selector row above)

## 10. Tests

- [x] 10.1 Unit tests for `LocalDropTableSourceStore.ListSummariesAsync`: empty database, multiple rows ordered newest-first, no HTML in summaries
- [x] 10.2 Unit tests for `LocalDropTableSourceStore.FindSummaryByLastUpdateAsync`: found, not found
- [x] 10.3 Unit tests for `LocalDropTableSourceStore.LoadByIdAsync`: known ID, unknown ID
- [x] 10.4 Unit tests for `LocalDropTableSourceStore.ReplaceAsync`: success replaces row atomically, fails on unknown `existingId`
- [x] 10.5 Update `DropTableSourceRefreshService` tests: new-version path saves; duplicate-version path returns `DuplicateVersion` without saving (no error message, `IsDuplicateVersion` true); `ConfirmSaveAsync` replaces and returns success; `ConfirmSaveAsync` returns failure when the store throws (not-found `existingId`)
- [x] 10.6 Update `DropTableLocalFileSourceWorkflow` tests: new-version path saves; duplicate-version path returns `DuplicateVersion`; `ConfirmSaveAsync` replaces and returns success; `ConfirmSaveAsync` returns failure when the store throws (not-found `existingId`)
- [x] 10.7 Update `DropTableImportService` tests to pass a `sourceId`; add test for not-found ID
- [x] 10.8 Update `MainWindowViewModel` tests: supply `SelectedSource` before invoking `ImportCommand`; assert `ImportCommand.CanExecute` is false when `SelectedSource` is null; assert source list refreshed and newest pre-selected after download/file-read; assert `ConfirmOverwrite` delegate called on duplicate-version result and no error reported for the detection itself; assert cancel path leaves the source list unrefreshed
- [x] 10.9 Integration test for migration: insert duplicate-version rows at the pre-index schema, run the startup initializer, verify only latest per version survives and the unique index is present
