## Context

`MainWindowShellViewModel.InitializeAsync` already gates a startup behavior on Drop Tables state: it awaits `DropTables.LoadStatusAsync()` and switches to the Drop Tables tab when `!DropTables.HasImportedData`. `MainWindowViewModel.LoadStatusAsync` separately computes `HasStoredSources` via `refreshWorkflow.HasStoredSourcesAsync()` (backed by `IDropTableSourceStore.HasAnyAsync`). `IDropTableSourceStore` has no delete operation — sources are only ever added (`SaveAsync`) or replaced in place (`ReplaceAsync`), never removed — so `HasStoredSources` can only go false→true, never back to false, over the lifetime of a database.

The manual UI already has two independent, working pieces this change reuses as-is: `IDropTableSourceRefreshWorkflow.RefreshAsync()` (download from the configured URL, parse Last Update, save a `source_files` row) and `IDropTableImportWorkflow.ImportAsync(sourceId)` (parse and import a stored source's rows into the DB). The consent modal introduced here is purely a new orchestration + UI layer on top of these two existing workflows.

Modal-style confirmation dialogs already follow one consistent shape in this codebase (`ConfirmClearImportDialog`, `ConfirmSourceOverwriteDialog`): a `Func<Task<...>>` delegate property assigned in `MainWindow.axaml.cs`'s constructor, awaited from VM code, opening an AXAML window with `ShowDialog`.

## Goals / Non-Goals

**Goals:**
- Gate the new consent modal on `!HasStoredSources` (stricter than the existing `!HasImportedData` tab-redirect condition, which is unchanged) so it never appears once any source — auto-downloaded, manually downloaded, or read from a local file — has ever been saved.
- Let the user explicitly Approve or Deny a one-time (per app launch) offer to download-and-import automatically.
- Run the download-then-import chain inside the same modal window, showing live per-step status, without a way to abandon it mid-run via the window chrome.
- Reuse `RefreshAsync` and `ImportAsync` unchanged in behavior; only widen their result types so the id can flow through without an extra lookup.

**Non-Goals:**
- Persisting a "don't ask again" preference. Deny is intentionally not remembered; the modal reappears on every launch until a source exists by any means.
- Any new parsing, schema, or import logic — this is purely orchestration and UI around existing workflows.
- Handling the `DuplicateVersion` / overwrite-confirmation path from `RefreshAsync` inside the automated routine — provably unreachable here, since the modal's own gate (`!HasStoredSources`) guarantees zero `source_files` rows exist at the point Approve calls `RefreshAsync` (see Decisions/Open Questions).
- Retrying a failed import automatically, or rolling back a successfully saved source if import subsequently fails.

## Decisions

### 1 — Gate on `HasStoredSources`, checked in `MainWindowShellViewModel.InitializeAsync`

The existing tab-redirect check (`!DropTables.HasImportedData`) stays exactly as-is. A second, independent check is added right after it (or alongside it) using `DropTables.HasStoredSources`, which `LoadStatusAsync` already populates by the time `InitializeAsync` reaches this point — no new data-loading path needed. When `!HasStoredSources`, the shell invokes a new delegate (e.g. `OfferAutomatedSetup: Func<Task>`) that opens the modal and awaits its full run (consent → running → done → closed by the user).

**Alternative considered**: a persisted `UserSettings` flag (e.g. `AutoSetupDeclined`) to suppress repeat prompts after a Deny. Rejected per explicit product decision — Deny is not persisted, and the self-suppression once a source exists is considered sufficient.

### 2 — Single modal window with three in-place view states, not three windows

One AXAML window (new, e.g. `FirstStartSetupWindow`) backed by one view model with a state enum (`Consent`, `Running`, `Done`). Avalonia data templates / `IsVisible` bindings switch which panel is shown as the state changes; the window itself never closes or reopens between states. This matches the requirement that the window stay open with live status rather than closing and reopening, and keeps a single owner for the disabled-X-button behavior.

**Alternative considered**: separate consent dialog that closes and hands off to a separate progress window. Rejected — two windows means two lifetimes to manage the disabled-close-button rule for, and a jarring window swap for no functional benefit.

### 3 — Disabling the native close (X) button

Avalonia's `Window` does not expose a first-class "disable the X button" property; the accepted approach is handling `Closing` and setting `e.Cancel = true` while a "closable" flag on the view model is false. The flag starts `false` (Consent), stays `false` through `Running`, and — per explicit product decision — **stays `false` even in `Done`**; only the in-window "Close" button (which calls `window.Close()` directly, bypassing the guard, or flips the flag first) can dismiss it. This is implemented once in the window's code-behind and is not something the view model needs to know about beyond exposing the flag.

**Alternative considered**: re-enable X once both steps settle, since at that point closing is "safe." Rejected per explicit product decision — a single, unambiguous exit control (Close) was preferred over a mixed control scheme.

### 4 — Chaining via a widened `DropTableRefreshResult`

`IDropTableSourceStore.SaveAsync` changes from `Task` to `Task<long>` — EF Core already assigns `SourceFileEntity.Id` after `SaveChangesAsync`, so this is a same-transaction, zero-extra-query change. `DropTableRefreshResult.Success` gains an `Id` (the same type of nullable-friendly shape the record already uses for `ExistingId` on the duplicate path). `DropTableSourceRefreshService.RefreshAsync` passes the new id from `SaveAsync`'s return value straight into `DropTableRefreshResult.Success(status, id)`.

The new modal's view model then runs:
```
var refreshResult = await refreshWorkflow.RefreshAsync();
// update Download step UI from refreshResult
if (refreshResult.Succeeded)
{
    var importResult = await importWorkflow.ImportAsync(refreshResult.Id!.Value);
    // update Import step UI from importResult
}
```
No new Application-layer service is introduced for the chain itself — it lives in the new modal's view model, which is presentation-layer orchestration, consistent with how `MainWindowViewModel` already sequences workflow calls today.

**Alternative considered**: call `sourceStore.FindSummaryByLastUpdateAsync(status.LastUpdate)` after a successful refresh to look up the id. Rejected — an avoidable extra DB round-trip when the id is already known at the point of insert.

### 5 — Failure reporting: modal-local plus the existing app-wide channel

Both the Download and Import steps report their outcome two ways: (a) inline in the modal's step list (status glyph + short message), and (b) through `AppErrors.Reporter` / the `errorReporter` used elsewhere in `MainWindowViewModel` (`RefreshAsync`, `ImportAsync`), so the failure also reaches the toast/log surface exactly as a manual Download/Import failure would today. This keeps one error-reporting convention for the whole app rather than introducing a modal-only channel.

If Download fails, Import never runs; the Import step is shown as skipped/not-run rather than failed. If Download succeeds but Import fails, the source row remains saved (no rollback) — `HasStoredSources` is now permanently true, so the modal will not reappear, and the user resolves the failed import manually later from the Drop Tables tab (Source Browser → select the source → Import).

### 6 — Effective URL is read fresh, not hardcoded

The modal's Consent view reads `IDropTableSourceSettingsStore.LoadAsync().SourceUrl` when it is about to show, and displays that value (which may already differ from `DropTableSourceDefaults.SourceUrl` if the user edited Settings before ever downloading anything) rather than the compile-time default. `RefreshAsync` itself already reads the same settings store internally, so the URL shown to the user is guaranteed to match the URL that will actually be requested.

### 7 — Reload cascade parity with the manual Download/Import buttons

Because the modal's view model drives `refreshWorkflow`/`importWorkflow` directly rather than through `MainWindowViewModel`, it does not automatically get the reload behavior the manual "Download from URL" and "Import" buttons already trigger (`MainWindowViewModel.LoadStatusAsync` refreshing the Drop Tables tab's own acquisition/import display, and `OnImportSucceeded` reloading Prime Gear, Wishlist Relics, and Wishlist Farming Missions). `FirstStartSetupViewModel` exposes two optional callbacks — `OnDownloadSucceeded` (invoked once, only after a successful download, before the import step starts) and `OnImportSucceeded` (invoked once, only after a successful import) — which `MainWindow.axaml.cs` wires to `dropTablesVm.LoadStatusAsync()` and `dropTablesVm.OnImportSucceeded` respectively, so the automated path produces the same visible refresh as the manual one.

**Alternative considered**: have the modal call `MainWindowViewModel.RefreshAsync()`/`ImportAsync()` directly instead of the raw workflows, inheriting the reload behavior for free. Rejected — those methods don't surface a success/failure result the modal can use to drive its own step states and messages (`RefreshAsync` swallows the error message on failure, `ImportAsync` requires `SelectedSource` to already be set), so the modal would still need parallel bookkeeping; the two-callback approach keeps the modal's own state machine simple while still reusing the shell's existing reload wiring.

## Risks / Trade-offs

- [Modal blocks app startup interaction] Since `InitializeAsync` awaits the modal's full lifecycle, the rest of the shell is technically initialized but the user cannot interact with other tabs until they Approve-and-wait or Deny. Mitigation: this mirrors the existing behavior of being redirected to the Drop Tables tab on a fresh DB — the app was already effectively unusable until a source existed; this just makes that fact explicit and actionable in one step. `ShowDialog` (modal-to-owner) already blocks input to `MainWindow` the same way other confirmation dialogs in this app do.
- [Deny-forever nagging] A user who repeatedly denies sees the modal on every launch. Accepted trade-off per explicit product decision; the manual Download/Read-from-file buttons remain the escape hatch (either one flips `HasStoredSources` and permanently stops the prompt).
- [Widening `SaveAsync`'s return type touches an existing interface] `IDropTableSourceStore.SaveAsync` is implemented only by `LocalDropTableSourceStore` and consumed only by `DropTableSourceRefreshService` and `DropTableLocalFileSourceWorkflow` (read-from-file path) in this codebase; both call sites are updated in this change, so there is no external/plugin surface at risk.

## Open Questions

None. (Previously this section flagged `RefreshAsync`'s `DuplicateVersion` result as an unhandled edge case reachable via "Read-from-file, then Deny, then Approve on a later launch." That path is not actually reachable: the modal's gate is `!HasStoredSources`, and `RefreshAsync`'s duplicate check can only find a match if a `source_files` row already exists. Any prior Read-from-file or manual Download would already have flipped `HasStoredSources` to `true`, which means the modal would never have been offered in the first place. So at the moment Approve triggers `RefreshAsync` inside this flow, zero rows are guaranteed to exist, and `DuplicateVersion` cannot occur. The automated routine correctly does not handle that branch.)
