## 1. Data-layer offload seam (close to the DbContext)

- [x] 1.1 Add an internal `ReliquaryDbExecutor` in `Infrastructure/Storage/` constructed from `DbContextOptions<ReliquaryDbContext>`, exposing `Task<T> RunAsync<T>(Func<ReliquaryDbContext, CancellationToken, Task<T>>, CancellationToken)` and a non-generic `RunAsync(...)` overload, each creating the context and running the delegate inside one `Task.Run`.
- [x] 1.2 Add a test proving a store call's body executes off the calling thread (capture `Thread.CurrentThread.ManagedThreadId` / assert it differs from the caller's) and that the context stays single-threaded within the delegate.
- [x] 1.3 Retrofit `SqliteWishlistStore`, `SqliteProfileStore`, and `SqliteOwnedGearStore` to hold a `ReliquaryDbExecutor` and route every public method's body through `RunAsync` (one offload per method, preserving existing logic, FK-violation handling, and `cancellationToken` flow).
- [x] 1.4 Retrofit the readers `SqlitePrimeGearReader`, `SqliteWishlistRelicsReader`, `SqliteWishlistFarmingMissionsReader`, `SqliteImportMetadataReader` (preserving the `File.Exists`/`SqliteException` guards inside the delegate).
- [x] 1.5 Retrofit `SqliteDropTableImportStore` and any remaining `Sqlite*`/seed paths (`DefaultProfileSeeder` callers, `SeedDefaultAsync`, `ClearAsync`), keeping the single explicit import transaction intact inside one offloaded unit of work. (Also retrofitted the DB-backed `LocalDropTableSourceStore` / `LocalDropTableSourceExporter`.)
- [x] 1.6 Confirm Infrastructure delegates may use `ConfigureAwait(false)` (no UI context); apply where natural.

## 2. Synchronous CPU adapter offload

- [x] 2.1 Turn `IDropTableParser.Parse` into `ParseAsync(...)` (accepting a `CancellationToken`); have `HtmlDropTableParser` run its regex parse via `Task.Run`. Update the import/refresh workflow callers to `await` it, and update parser/workflow tests to the async signature. (Kept the public synchronous `Parse` core so the parser's own integration tests are unchanged; the port exposes only `ParseAsync`.)
- [x] 2.2 Remove the `Task.Run` wrapper from `MainWindowViewModel.ReadFromFileAsync` now that offloading lives below it. (Also removed the redundant `Task.Run` around parse + `ReplaceAsync` in `DropTableImportService`.)

## 3. Adopt CommunityToolkit.Mvvm + AsyncAwaitBestPractices

- [x] 3.1 Add `CommunityToolkit.Mvvm` and `AsyncAwaitBestPractices` package references to the Presentation project.
- [x] 3.2 Replace the hand-rolled `AsyncRelayCommand`/`RelayCommand` with the Toolkit equivalents; migrate `INotifyPropertyChanged`/`SetProperty` boilerplate to `ObservableObject` + `[ObservableProperty]`/`[NotifyCanExecuteChangedFor]` across the ViewModels.
- [x] 3.3 Configure a global default exception handler via `SafeFireAndForgetExtensions.SetDefaultExceptionHandling(...)` (route to `IErrorReporter`) plus an Avalonia/`TaskScheduler.UnobservedTaskException` last-resort hook in `App`/`Program`.
- [x] 3.4 Update existing command tests to the Toolkit surface (`AsyncRelayCommand.ExecuteAsync`) and assert exception propagation on the awaitable path.

> **Cross-change dependency:** failures route to `IErrorReporter`, delivered by the `error-reporting-surface` change. Build against the interface; until it lands, inject an interim no-op/footer reporter and swap when available.

## 4. Local helpers — busy & reload serialization

- [x] 4.1 Add a reusable `BusyScope` primitive: acquiring it sets `IsBusy = true` and returns a disposable that resets it on dispose; a second acquire while busy returns a sentinel so the caller early-returns (reentrancy guard for multi-step / non-command operations).
- [x] 4.2 Add a `ReloadGate` primitive that cancels the previous in-flight `CancellationTokenSource` and hands the new operation a fresh token (latest-wins), so superseded reloads stop mutating shared state.
- [x] 4.3 Unit-test both primitives (reentrancy early-return; previous token cancelled).
- [x] 4.4 Document the ViewModel invariants in one place: no `ConfigureAwait(false)` in ViewModels; ViewModels never call `Task.Run` (offload lives in Infrastructure); blocking-on-async only at startup/shutdown.

## 5. Content ViewModels — busy, fire-and-forget, cancellation

- [x] 5.1 Add `IsBusy` to `PrimeGearViewModel`, `WishlistRelicsViewModel`, `WishlistFarmingMissionsViewModel` and gate their reloads with `BusyScope`.
- [x] 5.2 Convert the constructor `_ = ReloadAsync()` calls in those three VMs plus `WishlistViewModel` and `ProfileViewModel` to `SafeFireAndForget(onException:)` so initial-load failures are reported to `IErrorReporter`.
- [x] 5.3 Build reload content into a local list and swap into the bound `ObservableCollection` in a single post-cancellation-check step, so a cancelled reload never leaves a half-cleared collection.
- [x] 5.4 Thread `CancellationToken` through `ReloadAsync`/`LoadAsync` via the `ReloadGate` so superseded reloads are cancelled (latest-wins; no intermediate state rendered).
- [x] 5.5 Route `PrimePartViewModel.IsChecked` and `PrimeGearCardViewModel.IsOwned` persistence (`_ = PersistAsync(...)`) through `SafeFireAndForget`, preserving the existing fade/overlap behavior but reporting failures.

## 6. MainWindowViewModel & shell fan-out

- [x] 6.1 Migrate `MainWindowViewModel` to the Toolkit command surface; remove the `await Task.Yield()` pseudo-offload; use `BusyScope`; keep progress/success in `OperationStatusText`/footer and route failures to `IErrorReporter` instead of writing `"… failed: …"` into the footer.
- [x] 6.2 Serialize the `OnImportSucceeded`, `OnImportCleared`, `OnActiveProfileChanged`, `OnActiveWishlistChanged`, `OnWishlistChanged` fan-out in `MainWindowShellViewModel` so a new trigger supersedes in-flight reloads rather than running them concurrently.
- [x] 6.3 Add a local in-content busy indicator to the Prime Gear / Relics / Missions views bound to each ViewModel's `IsBusy` (so the user sees which view is reloading); keep the footer spinner for global Drop Tables source operations.

## 7. Startup/shutdown blocking

- [x] 7.1 Add explanatory comments at the `MainWindow` constructor and `OnWindowClosing` `.GetAwaiter().GetResult()` sites documenting why blocking is bounded to this boundary and wrapped in `Task.Run`.
- [x] 7.2 Verify (by inspection + a guard test) that no `.GetResult()`/`.Result`/`.Wait()` on async work exists in any interactive code path (commands, property setters, reloads).

## 8. Verification

- [x] 8.1 Add unit tests asserting: commands disabled while busy; double-trigger of a busy command is a no-op; a superseded reload's continuation does not mutate bound collections; a thrown command/persist is reported to `IErrorReporter` (assert via a fake reporter).
- [x] 8.2 Add a test (or analyzer/inspection check) asserting ViewModels do not use `ConfigureAwait(false)`, do not call `Task.Run`, and that collection mutations occur on the UI thread.
- [x] 8.3 Run `dotnet build` and `dotnet test`; confirm green. Verify responsiveness via tests/build (do not launch the app unannounced, per project rules).
- [x] 8.4 Update `docs/technical-reference.md` (and CLAUDE-referenced docs) with the UI concurrency contract — including the SQLite sync-over-async note and the Infrastructure offload seam — so future code follows it.
