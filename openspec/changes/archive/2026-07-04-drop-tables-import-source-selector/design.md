## Context

`source_files` stores every download/file-read without deduplication. `IDropTableSourceStore.LoadAsync()` always resolves to the highest `id` row; `IDropTableImportWorkflow.ImportAsync()` takes no arguments. There is no unique constraint on `source_last_update`, so repeated downloads of the same version silently accumulate. The Import section shows only a button with no source visibility.

## Goals / Non-Goals

**Goals:**
- Enforce version uniqueness in `source_files` via a UNIQUE index with a safe deduplication migration
- Surface a duplicate-version detection step in both acquisition paths and require explicit user confirmation before overwriting
- Replace the static import button with a version-only ComboBox selector that drives import
- Add a pictogram button that opens a modal source browser (version, source, download time)

**Non-Goals:**
- Bulk deletion of stored sources
- Showing HTML content in any UI surface
- Changing when or how the source is downloaded (URL/timeout/redirect behaviour unchanged)

## Decisions

### 1. Unique index on `source_last_update`; deduplication migration runs first

A `UNIQUE` index is added to `source_files.source_last_update`. The EF migration runs a `DELETE ... WHERE id NOT IN (SELECT MAX(id) FROM source_files GROUP BY source_last_update)` statement before creating the index so existing duplicate rows are cleaned up safely. The `MAX(id)` rule keeps the most-recently-inserted row per version, consistent with the existing "latest wins" resolution used everywhere.

**Rationale:** The DB constraint makes uniqueness a hard guarantee regardless of application-layer checks, protecting against race conditions and future call-site drift.

**Migration timing:** This change depends on the `startup-database-initialization` change (precondition — implement first), which applies all pending migrations at a single application-startup point before any data access. The deduplication migration therefore runs before the source list is first read, and the new read methods are plain queries with no migration or file-existence awareness.

### 2. Two-phase acquisition: detect → confirm → replace

Neither the download workflow (`DropTableSourceRefreshService`) nor the file-read workflow (`DropTableLocalFileSourceWorkflow`) saves directly when a duplicate version is detected. Instead they:

1. Extract `lastUpdate` from the HTML
2. Call `sourceStore.FindSummaryByLastUpdateAsync(lastUpdate)` — returns `DropTableSourceSummary?`
3. If a duplicate exists: return a `DuplicateVersion` result carrying the in-memory `DropTableSourceContent` (pending) and the existing row's `Id` — **without saving**
4. VM receives the `DuplicateVersion` result, shows `ConfirmSourceOverwriteDialog`, and if confirmed calls `workflow.ConfirmSaveAsync(pending, existingId)`
5. `ConfirmSaveAsync` is a new method on both workflow interfaces; it calls `sourceStore.ReplaceAsync(existingId, newContent)` which atomically deletes the old row and inserts the new one

**`DuplicateVersion` result semantics:** the `DuplicateVersion` state carries `Succeeded = false`, `ErrorMessage = null`, and non-null `Pending`/`ExistingId`; an `IsDuplicateVersion` property (true when `Pending` is non-null) discriminates it from a plain failure. The VM checks `IsDuplicateVersion` **before** the generic failure branch, so a duplicate detection never reaches the error reporter.

**`ConfirmSaveAsync` failure path:** `ReplaceAsync` throws when `existingId` no longer exists in `source_files`. Both `ConfirmSaveAsync` implementations wrap the call in the same try/catch translation `RefreshAsync` already uses and return a failure result; no row is inserted.

**Rationale:** Keeping the save decision in the workflow (not the VM) maintains layer separation. Holding the pending `DropTableSourceContent` in memory between detect and confirm is safe for a desktop app with a short user-interaction delay.

**Alternative considered:** Let the store do an `INSERT OR REPLACE` under the UNIQUE index — this silently replaces without user interaction, violating the requirement for an explicit confirmation step.

### 3. `IDropTableSourceStore` gains three new methods

```
FindSummaryByLastUpdateAsync(string lastUpdate) -> Task<DropTableSourceSummary?>
ReplaceAsync(long existingId, DropTableSourceContent newContent) -> Task
ListSummariesAsync() -> Task<IReadOnlyList<DropTableSourceSummary>>
LoadByIdAsync(long id) -> Task<DropTableSourceContent?>
```

All are added to the existing `IDropTableSourceStore` interface (not a new port), consistent with `LoadAsync` / `HasAnyAsync` / `SaveAsync` already there.

### 4. `DropTableImportWorkflow` takes an explicit `sourceId`

`IDropTableImportWorkflow.ImportAsync(long sourceId, CancellationToken)` replaces the no-argument form. `DropTableImportService` calls `LoadByIdAsync(sourceId)` instead of `LoadAsync()`, and returns a failure when the ID is not found. All call sites (VM + tests) are updated.

### 5. `DropTableSourceSummary` record lives in Application layer

`DropTableSourceSummary(long Id, string SourceUrl, DateTimeOffset DownloadedAt, string LastUpdate)` is defined in Application and referenced directly by Presentation. No separate Presentation DTO needed at this scale.

### 6. ComboBox label: version string only

Each ComboBox item's label is the `LastUpdate` string. When empty, the label reads `"(version unknown)"`. The download timestamp and source URL are visible in the source browser modal, not in the inline selector.

**Rationale:** Keeps the selector compact. Users who need provenance details open the browser modal.

**Note:** the `"(version unknown)"` fallback is defensive only — both acquisition workflows fail without a parseable Last Update, so no new row can have an empty version. It guards against rows from older database states.

### 7. Source browser modal: `SourceBrowserWindow` with DataGrid

A new Avalonia `Window` opened via `ShowDialog(owner)` from the MainWindow. Columns: **Version** (`LastUpdate`), **Source** (URL for downloads; filename extracted from the file URI for local files), **Downloaded** (`DownloadedAt` in local short date/time). The window is read-only. It receives `IReadOnlyList<DropTableSourceSummary>` from the VM's current `AvailableSources`.

The VM exposes `Func<IReadOnlyList<DropTableSourceSummary>, Task>? OpenSourceBrowserAsync` wired by `MainWindow`. An `OpenSourceBrowserCommand` on the VM calls this delegate. The pictogram button in `DropTablesView.axaml` binds to this command.

**Rationale:** Follows the existing `RewardsModalWindow` / `ConfirmClearImportDialog` pattern: a static `ShowAsync` helper on the window class, wired at the presentation boundary by `MainWindow`. No new architectural patterns needed.

### 8. Auto-select newest source after acquisition

After each successful download or file-read (including confirmed overwrites), `RefreshSourceListAsync()` repopulates `AvailableSources` (newest-first) and sets `SelectedSource` to the first item. On initial load (`LoadStatusAsync`), the same refresh runs.

### 9. Overwrite confirmation dialog: `ConfirmSourceOverwriteDialog`

A new `Window` (like `ConfirmClearImportDialog`) that shows the version label and download timestamp of the existing record and asks "Replace it?". Returns `bool` via `ShowDialog<bool>(owner)`. The VM holds a `Func<DropTableSourceSummary, Task<bool>>? ConfirmOverwrite` delegate wired by `MainWindow`.

## Risks / Trade-offs

- **In-memory pending content between detect and confirm**: If the user takes a very long time to confirm, the content is stale only in the sense that the server may have updated — but the version string would then differ, so no duplicate would have been detected in the first place. No practical risk.
- **`IDropTableSourceStore` interface growth**: Four new methods on one interface is on the edge of healthy. If this continues to grow, splitting into read/write ports is a reasonable next step.
- **Interface break on `IDropTableImportWorkflow`**: All existing tests calling `ImportAsync()` must supply a `sourceId`. Mitigated by updating all tests in this change.

## Migration Plan

1. EF migration `AddSourceFilesUniqueVersionIndex`:
   - SQL: `DELETE FROM source_files WHERE id NOT IN (SELECT MAX(id) FROM source_files GROUP BY source_last_update)`
   - `CreateIndex("IX_source_files_source_last_update", "source_files", "source_last_update", unique: true)`
2. The migration is applied by the startup database initializer (see the `startup-database-initialization` precondition change), so deduplication takes effect before the first source list is displayed.
3. No rollback needed for alpha (schema is owned end-to-end by the app).
