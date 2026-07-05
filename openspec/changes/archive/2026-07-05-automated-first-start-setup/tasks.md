## 1. Widen the source-save contract to return the new row id

- [x] 1.1 Change `IDropTableSourceStore.SaveAsync` signature from `Task` to `Task<long>` in `src/Application/WF.Reliquary.Application/Ports/IDropTableSourceStore.cs`.
- [x] 1.2 Update `LocalDropTableSourceStore.SaveAsync` (`src/Infrastructure/WF.Reliquary.Infrastructure/DropTables/LocalDropTableSourceStore.cs`) to return the EF-assigned `SourceFileEntity.Id` after `SaveChangesAsync`.
- [x] 1.3 Add an `Id` (nullable `long`) to `DropTableRefreshResult` and thread it through the `Success` factory in `src/Application/WF.Reliquary.Application/DropTables/DropTableRefreshResult.cs`.
- [x] 1.4 Update `DropTableSourceRefreshService.RefreshAsync` (`src/Application/WF.Reliquary.Application/DropTables/DropTableSourceRefreshService.cs`) to pass the new id from `SaveAsync`'s return value into `DropTableRefreshResult.Success(status, id)`.
- [x] 1.5 Update `DropTableLocalFileSourceWorkflow` (`src/Application/WF.Reliquary.Application/DropTables/DropTableLocalFileSourceWorkflow.cs`), the other `SaveAsync` caller, to compile against the new `Task<long>` signature (it may ignore the returned id if not needed there).
- [x] 1.6 Update/add unit tests covering `DropTableSourceRefreshService.RefreshAsync` to assert the success result carries the new source's id (see existing tests in `tests/Unit/WF.Reliquary.UnitTests/` for this service).

## 2. First-start setup gate and shell wiring

- [x] 2.1 Add a delegate property (e.g. `Func<Task>? OfferAutomatedSetup`) to `MainWindowShellViewModel`, invoked from `InitializeAsync` after `DropTables.LoadStatusAsync()` when `!DropTables.HasStoredSources` — added alongside, not replacing, the existing `!DropTables.HasImportedData` tab-redirect check.
- [x] 2.2 Confirm/expose `HasStoredSources` on `MainWindowViewModel` as a bindable property populated in `LoadStatusAsync` (reuse the existing value already computed via `refreshWorkflow.HasStoredSourcesAsync()`).
- [x] 2.3 Wire `OfferAutomatedSetup` in `MainWindow.axaml.cs`'s constructor to open the new setup window, following the existing `ConfirmClearImportDialog`/`ConfirmSourceOverwriteDialog` wiring pattern.

## 3. First-start setup modal — view model

- [x] 3.1 Create `FirstStartSetupViewModel` with a state enum (`Consent`, `Running`, `Done`) and bindable properties for: effective source URL, download step status (pending/running/succeeded/failed + message), import step status (same shape), and whether Close is enabled.
- [x] 3.2 On construction/show, load the effective URL via `IDropTableSourceSettingsStore.LoadAsync().SourceUrl` for display in the Consent state.
- [x] 3.3 Implement the Approve command: transition to `Running`, call `refreshWorkflow.RefreshAsync()`, update the download step from the result; if succeeded, call `importWorkflow.ImportAsync(result.Id!.Value)` and update the import step from that result; if download failed, mark the import step as skipped/not-run. Transition to `Done` once both steps have reached a terminal state (or the routine short-circuits after a download failure).
- [x] 3.4 Implement the Deny command: no workflow calls, modal is dismissible immediately (no persisted state).
- [x] 3.5 Report Download/Import failures through the existing `AppErrors.Reporter` / `errorReporter` channel in addition to the inline step status, matching `MainWindowViewModel.RefreshAsync`/`ImportAsync`'s existing error-reporting convention.
- [x] 3.6 Implement the Close command, enabled only once both steps have reached a terminal state.
- [x] 3.7 Add `OnDownloadSucceeded`/`OnImportSucceeded` callbacks so a successful step triggers the same reload behavior the manual Download/Import buttons already produce (Drop Tables tab's own status, and Prime Gear/Wishlist views via `MainWindowViewModel.OnImportSucceeded`) — wired in `MainWindow.axaml.cs`, not invoked on failure or skip.

## 4. First-start setup modal — window (AXAML)

- [x] 4.1 Create `FirstStartSetupWindow.axaml` / `.axaml.cs` with three in-place panels bound to the view model's state (Consent: explanation + effective URL + "you can download this yourself" note + Approve/Deny buttons; Running: two-step status list, no dismiss controls; Done: two-step final status + Close button).
- [x] 4.2 Handle the window's `Closing` event to cancel (`e.Cancel = true`) unless the close was explicitly initiated via the in-window Close command — i.e., the native X control never closes the window, in any state, including Done.
- [x] 4.3 Add a static `ShowAsync(Window owner, ...)` helper mirroring the pattern used by `ConfirmClearImportDialog`/`SourceBrowserWindow`, for use by the `OfferAutomatedSetup` wiring in `MainWindow.axaml.cs`.

## 5. Tests

- [x] 5.1 Add/extend integration tests (see `tests/Integration/WF.Reliquary.IntegrationTests/MainWindowShellViewModelTests.cs`) covering: modal offered when no source exists; modal not offered when a source exists (with or without imported data); modal not offered on subsequent launches after a manual download/read-from-file, without ever approving.
- [x] 5.2 Add tests for the new setup view model (in `tests/Integration/WF.Reliquary.IntegrationTests/`, since the view model lives in the Presentation project which the Unit test project does not reference) covering: Approve chains download then import using the returned id; Deny performs no workflow calls; download failure skips import and surfaces an error; import failure after a successful download leaves the source saved and surfaces an error; Close only becomes available once both steps are terminal.
- [x] 5.3 Run the full test suite (`dotnet test`) to confirm no regressions from the widened `IDropTableSourceStore.SaveAsync` signature.

## 6. Manual verification

- [x] 6.1 Verify against a fresh local database: consent modal appears, effective URL matches Settings, Approve runs download then import with visible step progress, Close appears only after both steps settle, native X is inert throughout.
- [x] 6.2 Verify Deny: modal closes, reappears on next launch.
- [x] 6.3 Verify a manual "Download from URL" or "Read from file" performed instead of approving the modal permanently suppresses future prompts.
