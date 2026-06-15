## Why

When no drop-table data has been imported yet, the app opens on the Prime Gear tab, which is empty and confusing — the user has no data and no obvious path to get some. Surfacing Drop Tables as the landing tab when there is no imported data, and adding a way to clear that data for a fresh re-import, closes the onboarding gap and gives users a clean path for refreshing their data.

## What Changes

- On startup, the tab selection logic checks whether any imported drop-table data exists; if none exists, the app opens on the **Drop Tables** tab instead of Prime Gear.
- The Drop Tables **Import** section gains a **"Clear imported data"** button that removes all persisted parsed import rows (executing the clearing stage of the re-import workflow).
- The "Clear imported data" button is disabled when no imported data exists and enabled when imported data is present.

## Capabilities

### New Capabilities

- `drop-table-import-clear`: Clear action that deletes all persisted import data as the first stage of a re-import workflow, with a button in the Drop Tables Import section that enables only when import data is present.

### Modified Capabilities

- `main-window-navigation`: Default startup tab changes from unconditionally Prime Gear to conditionally Drop Tables when no imported data exists.

## Impact

- **Presentation**: `DropTablesViewModel` / Drop Tables view gains a clear-data command and enabled state; startup tab-selection logic in main window reads import state.
- **Application**: New application-layer operation to delete all import rows; startup query to check whether any import data exists.
- **Infrastructure**: New SQL delete operation behind the application-owned contract.
- **No schema changes**: clearing deletes rows but does not alter table structure.
