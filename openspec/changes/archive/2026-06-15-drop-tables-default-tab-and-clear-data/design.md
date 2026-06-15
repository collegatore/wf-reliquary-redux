## Context

`MainWindowShellViewModel` initialises with `selectedTabIndex = 0` (Prime Gear) unconditionally. The `MainWindowViewModel` (Drop Tables) loads import status asynchronously after the window is shown via a fire-and-forget `_ = shell.DropTables.LoadStatusAsync()`. There is no post-load hook that could redirect the initial tab.

`IDropTableImportStore` has only `ReplaceAsync`. Clearing import data today requires a full replace — there is no first-class delete-only path. `IImportMetadataReader.GetImportedVersionAsync` returns `null` when no import exists and a version string when one does, giving a cheap way to detect import presence.

## Goals / Non-Goals

**Goals:**
- Open on the Drop Tables tab when no import data exists at startup (including first launch).
- Add a "Clear imported data" button to the Drop Tables Import section that deletes all persisted import rows.
- Keep the button disabled when no import data exists; enable it when import data is present.
- After clearing, reload dependent views (Prime Gear, Wishlist) to reflect empty state.

**Non-Goals:**
- Any automatic re-import after clearing.
- Changes to the DB schema.

## Decisions

### 1 — Post-load tab redirect via `InitializeAsync` on the shell

The startup tab check must happen after `LoadStatusAsync` resolves, not at construction time (import DB may be empty on first run). `MainWindow.axaml.cs` currently fires `LoadStatusAsync` as a detached task.

**Decision**: Add `InitializeAsync()` to `MainWindowShellViewModel` that awaits `DropTables.LoadStatusAsync()` and then sets `SelectedTabIndex = DropTablesTabIndex` when `DropTables.HasImportedData` is `false`. `MainWindow` calls `_ = shell.InitializeAsync()` instead of `_ = shell.DropTables.LoadStatusAsync()`.

**Alternative considered**: post-load callback delegate on `MainWindowViewModel`. Rejected — the tab-selection policy belongs with the shell, not the Drop Tables VM.

### 2 — `HasImportedData` property on `MainWindowViewModel`

`LoadStatusAsync` already calls `importMetadataReader.GetImportedVersionAsync`. We capture whether the result was non-null as a `bool HasImportedData` property set during that call. This property gates both the clear button's enabled state and the startup tab decision.

### 3 — `ClearAsync` on `IDropTableImportStore`

The clear operation is the deletion half of `ReplaceAsync` (same FK-order truncation, no inserts). Extending the existing store port with `ClearAsync()` keeps the operation co-located with `ReplaceAsync` and requires no new interface.

**Alternative considered**: new `IDropTableImportClearWorkflow` in Application. Rejected — clearing is a single store operation with no orchestration; a dedicated workflow interface is unnecessary indirection.

### 4 — `OnImportCleared` callback mirrors `OnImportSucceeded`

After a clear, Prime Gear, Wishlist Relics, and Wishlist Farming views must reload to show empty state, just as they do after a successful import. `MainWindowShellViewModel` already wires `OnImportSucceeded` for this. A parallel `OnImportCleared` delegate on `MainWindowViewModel` keeps the pattern consistent and avoids coupling the VM to the shell.

### 5 — Confirmation dialog via `Func<Task<bool>>` callback on the VM

The clear is destructive and irreversible, so a confirmation prompt is required. The existing pattern (`ConfirmDiscard`, `ConfirmOwnedTracking`) uses a `Func<Task<bool>>` delegate assigned in `MainWindow.axaml.cs`, keeping Avalonia/window-opening code out of the VM. A new `ConfirmClearImportDialog` (AXAML + code-behind) with "Confirm" and "Cancel" buttons follows the same shape as `ConfirmDiscardDialog`. The VM checks the delegate before proceeding; if unset or returning `false`, the command aborts without clearing.

**Alternative considered**: reuse `ConfirmDiscardDialog`. Rejected — its text is hardcoded to "discard changes" and it would mislead the user.

## Risks / Trade-offs

- [Startup async race] `InitializeAsync` runs off-thread before `Dispatcher` is fully warm. Mitigated: same pattern as current `LoadStatusAsync` fire-and-forget; tab index mutation must be dispatched to the UI thread.
- [GearVaultStatus rows] The clear path must delete `GearVaultStatuses` in FK order, identical to the replace path. Handled by re-using the same delete sequence in `ClearAsync`.
