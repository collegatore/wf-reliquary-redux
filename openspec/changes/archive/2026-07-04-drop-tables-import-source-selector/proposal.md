## Why

The `source_files` table accumulates every downloaded drop-table HTML (rows are never deleted), but the import workflow always silently uses the most-recently-inserted row. Users have no way to choose which stored source to import, and nothing prevents duplicate downloads of the same version. This change introduces source selection, enforces version uniqueness, and provides a clear UI for inspecting stored sources.

## What Changes

- **Source version uniqueness**: A unique index is added to `source_files.source_last_update`. A migration deduplicates existing rows before the index is created (latest download per version survives).
- **Duplicate detection on acquisition**: Before saving a newly downloaded or file-read source, both acquisition workflows check whether a source with the same Last Update version already exists. If so, a confirmation is required before the existing record is replaced.
- **Source selector ComboBox**: The static "Import stored source" button is replaced by a ComboBox (showing the version label) followed by the Import button. The most-recently-acquired source is pre-selected.
- **Source browser modal**: A pictogram button next to the ComboBox opens a modal window with a table showing all stored sources (version, source URL or filename, download timestamp), for inspection without leaving the tab.
- **Import uses selected source**: `IDropTableImportWorkflow` takes an explicit source ID so the import operates on the user's selection rather than always using the latest.

## Capabilities

### New Capabilities

- `drop-table-import-source-selector`: Source listing query, view-model selector state, import gated on selection, source browser modal window, and overwrite confirmation UI.

### Modified Capabilities

- `drop-table-source-files`: Unique index on `source_last_update` with deduplication migration; new overwrite store operation.
- `drop-table-source-refresh`: Download workflow detects a duplicate version before saving and returns a pending result requiring user confirmation to replace.
- `drop-table-local-file-import`: File-read workflow detects a duplicate version before saving and returns a pending result requiring user confirmation to replace.
- `drop-table-parsing-import`: Import workflow accepts an explicit source file ID; import button is gated on a valid selection.

## Impact

- **Precondition** — depends on the `startup-database-initialization` change (implement first): the database is created and migrated at application startup, so the new store queries and the deduplication migration need no lazy-migration handling
- **Application/Ports** — `IDropTableSourceStore`: new `ListSummariesAsync`, `FindSummaryByLastUpdateAsync`, `ReplaceAsync` methods
- **Application** — `IDropTableImportWorkflow`: signature change to accept `sourceId`; `IDropTableSourceRefreshWorkflow` / `IDropTableLocalFileSourceWorkflow`: new `ConfirmSaveAsync` for confirmed overwrites
- **Application** — `DropTableRefreshResult`, `DropTableFileRegistrationResult`: new `DuplicateVersion` factory/state
- **Infrastructure** — `LocalDropTableSourceStore`: implement new store methods; EF migration for unique index with deduplication
- **Presentation** — `MainWindowViewModel`: `AvailableSources`, `SelectedSource`, overwrite-confirm delegate, source-browser-open delegate; updated `ImportCommand`, `RefreshAsync`, `ReadFromFileAsync`
- **Presentation** — `DropTablesView.axaml`: ComboBox + pictogram button replacing the static import button layout
- **Presentation** — New `SourceBrowserWindow` modal with DataGrid
- **Presentation** — New `ConfirmSourceOverwriteDialog`
- Tests for new store queries, duplicate detection, overwrite flow, and VM selector state
