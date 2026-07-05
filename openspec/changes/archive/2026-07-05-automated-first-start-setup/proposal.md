## Why

Right now, a user who launches the app for the first time is dropped onto an empty Drop Tables tab and has to manually click Download, then manually click Import, before the rest of the app (Prime Gear, Wishlist, etc.) has any data to show. This is not discoverable and adds friction to the very first thing a new user has to do. An automated setup routine — download the drop-table source from the configured URL and import it in one step — removes that friction, but only if it happens transparently and never runs without the user's explicit consent, since it touches the network on the user's behalf.

## What Changes

- Add a consent modal that appears when the app starts with no downloaded/read-in drop-table sources at all (stricter than the existing "no imported data" tab-redirect condition, and self-suppressing forever once any source exists).
- The modal explains that a file will be downloaded from the web, displays the effective source URL (as currently configured in Settings, defaulting to `https://www.warframe.com/droptables`), and notes the user can download the file themselves in a browser instead.
- The modal offers only Approve / Deny. The routine never runs without an explicit Approve click. Deny is not persisted — the modal reappears on every subsequent launch until a source exists (via this routine or the existing manual Download/Read-from-file paths).
- On Approve, the same modal window transitions in place (no second window) to a live two-step progress view — Download, then Import — each showing pending/running/succeeded/failed status, and finally to a Done view with a Close button once both steps have settled.
- The modal's native close (X) button is disabled for the modal's entire lifetime; Close is the only way to dismiss it once both steps settle.
- Failures are shown inline in the modal's step list and also reported through the existing app-wide error-reporting channel, matching how the manual Download/Import buttons already report errors today.
- **BREAKING** (internal contract only, no external API): `IDropTableSourceStore.SaveAsync` changes from `Task` to `Task<long>`, returning the newly saved source's id; `DropTableRefreshResult.Success` gains an `Id` so callers can chain straight into import without a redundant lookup. All existing in-repo callers are updated as part of this change.

## Capabilities

### New Capabilities
- `first-start-setup`: the consent modal, its progress/done states, the gating condition, and the download-then-import auto-chain it triggers.

### Modified Capabilities
- `drop-table-source-refresh`: `RefreshAsync`'s success result now carries the id of the newly saved source, in addition to its existing status fields.

## Impact

- `src/Presentation/WF.Reliquary.Presentation/MainWindowShellViewModel.cs` — new gate check and modal-invocation hook alongside the existing `InitializeAsync` tab-redirect.
- `src/Presentation/WF.Reliquary.Presentation/MainWindowViewModel.cs` — exposes `HasStoredSources`-based gate reuse; no change to its existing Download/Import commands beyond the result shape.
- `src/Presentation/WF.Reliquary.Presentation/MainWindow.axaml.cs` — wires the new modal delegate, following the existing `ConfirmClearImportDialog`/`ConfirmSourceOverwriteDialog` pattern.
- New AXAML window + code-behind + view model for the consent/progress/done modal.
- `src/Application/WF.Reliquary.Application/DropTables/DropTableRefreshResult.cs`, `DropTableSourceRefreshService.cs` — add `Id` to the success result.
- `src/Application/WF.Reliquary.Application/Ports/IDropTableSourceStore.cs`, `src/Infrastructure/WF.Reliquary.Infrastructure/DropTables/LocalDropTableSourceStore.cs` — `SaveAsync` returns the new row id.
- No database schema changes; no new external dependencies.
