## Context

The app is Avalonia 12 + MVVM. ViewModels obtain Application-layer workflows/stores through `ReliquaryServiceFactory` and call them from commands, property setters, and constructors. The Application/Infrastructure ports are uniformly `async` and accept `CancellationToken`.

**Key correction discovered during the audit:** the ports being `async` does *not* mean they yield the UI thread. The storage layer is EF Core over `Microsoft.Data.Sqlite`, whose ADO.NET provider **executes its async methods synchronously** — there is no async I/O for SQLite. So `await context.X.ToListAsync(ct)`, `SaveChangesAsync`, `ExecuteDeleteAsync`, and `SqlQueryRaw<T>(...).ToListAsync` all run to completion on the calling thread before the `await` ever suspends. Every `await store.XAsync()` from a ViewModel therefore blocks the UI thread for the full query. `HttpClient` *is* genuinely async (the download yields), but the subsequent `HtmlDropTableParser.Parse(...)` is a fully synchronous, regex-heavy CPU method. The concurrency *risk* is thus both in the Presentation layer (patterns) **and** at the data-access layer (sync-over-async), and the fix must address the latter at its source.

Each store/reader follows the same shape: a `DbContextOptions` field, and each public method does `await using var context = new ReliquaryDbContext(options);` then several `await context.…Async(ct)` calls. That uniform shape — one context lifetime per public method — is the natural seam to offload at.

Current state, from the audit:

- **Offloading is inconsistent.** `MainWindowViewModel.ReadFromFileAsync` wraps the local-file workflow in `Task.Run`; `RefreshAsync` (HTTP download **and** synchronous HtmlAgilityPack parse) and `ImportAsync` (bulk DB import with post-insert compute) await directly on the UI thread. The download+parse path is the most likely real freeze and is *not* offloaded.
- **`await Task.Yield()` is used as a pseudo-offload.** `MainWindowViewModel` calls `await Task.Yield()` after setting `IsBusy` so the busy state paints before the work starts. It does not move work off the UI thread; it only lets one render pass through.
- **Fire-and-forget everywhere.** Constructors of `PrimeGearViewModel`, `WishlistRelicsViewModel`, `WishlistFarmingMissionsViewModel`, `WishlistViewModel`, `ProfileViewModel` call `_ = ReloadAsync()`. Property setters `PrimePartViewModel.IsChecked` and `PrimeGearCardViewModel.IsOwned` call `_ = PersistAsync(...)`. Any throw is unobserved — no log, no UI feedback.
- **`AsyncRelayCommand.Execute` is `async void`** with no try/catch; a throw becomes a process-level unhandled exception.
- **Busy state is non-uniform.** Only `MainWindowViewModel` has `IsBusy`. Content ViewModels have none, so their reloads have no progress indication and no guard against overlap.
- **Reload fan-out is unserialized.** `MainWindowShellViewModel` wires `OnImportSucceeded`, `OnActiveProfileChanged`, `OnActiveWishlistChanged`, `OnWishlistChanged` to `await` several `ReloadAsync()` calls. A second trigger before the first finishes can run two reloads that both `Clear()`/`Add()` the same `ObservableCollection`.
- **No `CancellationToken` in ViewModel signatures**, contrary to the project convention, so superseded reloads run to completion.
- **Blocking on async on the UI thread.** `MainWindow`'s constructor and `OnWindowClosing` use `Task.Run(() => store.XAsync()).GetAwaiter().GetResult()`. This is a deliberate deadlock-avoidance hack and is acceptable *only* at the startup/shutdown boundary.

Constraint: pre-V1, so no backward-compat shims are required and the existing test suite can be adjusted freely. No persistence/schema impact.

## Goals / Non-Goals

**Goals:**
- Guarantee that DB work (sync-over-async SQLite) never runs on the UI thread, via a single offload seam close to the `ReliquaryDbContext`, with no per-consumer `Task.Run`.
- One documented rule for *where* offloading lives (the layer that owns the blocking work), applied uniformly so ViewModels never call `Task.Run`.
- Reuse `CommunityToolkit.Mvvm` + `AsyncAwaitBestPractices` for command/INPC/fire-and-forget plumbing so no asynchronous failure is silently swallowed and commands aren't `async void`.
- `async void` command execution that catches, reports, and recovers (restores not-busy).
- A uniform busy/reentrancy primitive reused by every ViewModel that runs awaitable work.
- Serialized (and ideally cancellable) content reloads so overlapping triggers cannot corrupt bound collections.
- `CancellationToken` threaded through ViewModel operations.
- Blocking-on-async confined to, and documented at, the startup/shutdown boundary only.
- Failures routed to a single error-reporting seam (`IErrorReporter`) so none are silently swallowed; the seam's implementation (toast + log) is delivered by the `error-reporting-surface` change.

**Non-Goals:**
- Introducing a DI container or a reactive framework (ReactiveUI). Keep the hand-rolled `ReliquaryServiceFactory` wiring; `CommunityToolkit.Mvvm` is a library, not a framework, and does not change the composition model.
- Building the error surface itself — the toast, the log file, the log-access controls, and the cross-platform launcher are owned by the `error-reporting-surface` change. This change only consumes `IErrorReporter`.
- Switching the database provider or making SQLite genuinely async — out of scope; the executor seam (Decision 3) is the accepted mitigation for sync-over-async.
- Changing the *semantics* of Application/Infrastructure ports. (Turning `IDropTableParser.Parse` into `ParseAsync` is a mechanical signature change for offloading, not a behavioral one.)
- Reworking the spinner visuals or the splitter/grid layout logic.

## Decisions

### Decision 1: Adopt CommunityToolkit.Mvvm + AsyncAwaitBestPractices, keep two small local helpers
Take `CommunityToolkit.Mvvm` for the command/INPC surface and `AsyncAwaitBestPractices` for safe fire-and-forget; hand-roll only the two genuinely project-specific pieces (the Infrastructure offload seam — Decision 3 — and the per-ViewModel reload serialization — Decision 5).
- **Why:** the command-side gaps (`async void`, busy state, reentrancy) are exactly what `CommunityToolkit.Mvvm.Input.AsyncRelayCommand` solves (`IsRunning`, `CanExecute` false while running, no `async void` at the call site), and its `[ObservableProperty]`/`[NotifyCanExecuteChangedFor]` source generators delete most of the repeated `SetProperty`/`PropertyChanged` plumbing across all eight ViewModels. `AsyncAwaitBestPractices.SafeFireAndForget(onException:)` plus `SetDefaultExceptionHandling` covers the *non-command* fire-and-forget (constructor loads, property-setter persistence) that the Toolkit does not. Both are small, widely used, actively maintained.
- **Why keep two helpers:** neither library decides *where* CPU/DB work runs (offloading), nor serializes callback-driven reloads that aren't commands. Those stay local.
- **Alternative considered — zero dependencies (previous plan):** hand-roll all three helpers. Rejected: it re-implements `AsyncRelayCommand`/`IsRunning`/`SafeFireAndForget` that these libraries already get right, and forgoes the source-generated INPC cleanup, for no real gain beyond dependency count.
- **Alternative considered — ReactiveUI:** an Rx-based framework layered on Avalonia; `ReactiveCommand` would cover the command side, but it imposes the whole observable-streams paradigm and scheduler discipline — far heavier than these findings justify. Rejected.
- **Test impact:** existing tests await `ExecuteTaskAsync()`; the Toolkit's `AsyncRelayCommand` exposes `ExecuteAsync(...)`, so tests update to that awaitable path (which still surfaces exceptions for deterministic assertions).

### Decision 2: Offloading is an Infrastructure responsibility, not a consumer concern
Rule: *the layer that owns the blocking work offloads it; consumers (ViewModels, workflows) just `await`.* Concretely — DB work offloads at the `ReliquaryDbContext` seam (Decision 3); the synchronous `HtmlDropTableParser` offloads its own parse (wrap the regex work in `Task.Run` inside the adapter, exposing an async port). ViewModels therefore **never** call `Task.Run`; they await stores/workflows that are now genuinely non-blocking. The `await Task.Yield()` pseudo-offload in `MainWindowViewModel` is removed (kept only if a paint-before-work beat is still wanted for the busy indicator).
- **Why:** the previous "offload CPU-bound work in the ViewModel" rule put the decision at the wrong altitude — every consumer had to know which calls were secretly synchronous, which is exactly how the inconsistency (and the SQLite blind spot) arose. Pushing offload down to where the blocking actually happens makes it correct-by-construction and uniform: there is one place per concern, close to the work.
- **Alternative considered — wrap each store *method* in `Task.Run` (over the final method):** rejected per the explicit constraint; it scatters `Task.Run` across dozens of public methods and still leaks the concern outward. The unit-of-work seam (Decision 3) achieves the same off-thread guarantee in one place.
- **Alternative considered — keep offloading in ViewModels:** rejected — needs every VM author to know SQLite is sync-over-async forever.

### Decision 3: A unit-of-work execution seam owns the context and offloads it
Add an internal `ReliquaryDbExecutor` (constructed from `DbContextOptions<ReliquaryDbContext>`) in `Infrastructure/Storage/` exposing:

```csharp
Task<T> RunAsync<T>(Func<ReliquaryDbContext, CancellationToken, Task<T>> work, CancellationToken ct = default)
    => Task.Run(async () =>
    {
        await using var context = new ReliquaryDbContext(options);
        return await work(context, ct);
    }, ct);
// + a non-generic Task RunAsync(Func<…, Task> work, …) overload
```

Every store/reader holds a `ReliquaryDbExecutor` instead of a bare `DbContextOptions`, and each public method becomes `=> executor.RunAsync(async (context, ct) => { …existing body… }, ct);`. The method's whole body (context creation + however many EF round-trips it makes) runs inside **one** `Task.Run` on a thread-pool thread.
- **Why this seam:** it is the single point that already owns the `ReliquaryDbContext` lifetime — "meaningfully close to the DB context." Granularity is one offload per logical operation (per context lifetime), not per EF call (too many thread hops) and not per consumer/ViewModel method (the "over the final method" altitude the constraint rejects). DbContext is not thread-safe, but a context is created and used entirely inside one `Task.Run`, so confinement is preserved.
- **Result:** `await store.XAsync()` from any consumer now genuinely yields the caller's thread on the first `await` (the `Task.Run`'s task), so the UI thread is free even though SQLite's ADO calls are synchronous.
- **Alternative considered — a custom `DbCommandInterceptor` / execution strategy:** EF interceptors can't relocate the work to another thread; they run on the calling thread. Rejected.
- **Alternative considered — wrap inside each EF call:** too fine-grained; multiplies thread hops within one method.

### Decision 4: Busy + reentrancy as a single disposable scope
Provide a `BusyScope` acquired at the top of each guarded operation: it sets `IsBusy = true`, returns `null`/no-op if already busy (so the operation early-returns), and resets `IsBusy = false` on dispose. Commands' `CanExecute` keys off `IsBusy`. (Where a command's *only* work is awaiting a store/workflow, the Toolkit `AsyncRelayCommand`'s built-in `IsRunning`/reentrancy may suffice; `BusyScope` covers the non-command paths and multi-step operations.)
- **Why:** collapses the repeated `if (IsBusy) return; IsBusy = true; try { … } finally { IsBusy = false; }` boilerplate (already in `MainWindowViewModel`) into one reusable, correct-by-construction primitive, and extends it to content ViewModels.

### Decision 5: Serialize content reloads by cancelling superseded ones (latest-wins) — DECIDED
Each content ViewModel holds a single in-flight reload. A new reload cancels the previous via a `CancellationTokenSource` swap (the `ReloadGate`, Decision 4's companion) and lets the stale continuation no-op after a cancellation check, so only the latest reload mutates the bound collections, always on the UI thread. **Decided: cancel-and-replace (latest-wins), not queue-in-order** — no view needs to render intermediate states; only the result for the current selection matters.
- **Why:** the fan-out triggers (import/profile/wishlist switch) are the concrete corruption risk, and the user only ever cares about the latest selection. Cancellation also satisfies the `CancellationToken` convention with a real consumer.
- **Alternatives rejected:** queue-in-order (renders stale intermediate states the user has already navigated past); a global app-wide reload lock (over-serializes independent views, complicates the shell wiring).

### Decision 6: UI-thread marshaling stays implicit via captured SynchronizationContext
Continuations after `await` resume on the UI thread because no `ConfigureAwait(false)` is used in ViewModels. The `Task.Run` now lives in Infrastructure (Decision 3), so a ViewModel that does `var x = await store.XAsync(); ActiveCards.Add(...)` resumes its continuation on the captured UI context and mutates `ObservableCollection`s safely. Correspondingly, Infrastructure code inside the executor delegate **may** use `ConfigureAwait(false)` freely (it has no UI context), while ViewModels **must not**. Keep `Dispatcher.UIThread.Post` only for the existing view-layer layout callbacks.
- **Why:** avoids sprinkling `Dispatcher` calls through ViewModels; documents the one invariant (ViewModels must not use `ConfigureAwait(false)`; the offload boundary is Infrastructure).

### Decision 7: Confine and document startup/shutdown blocking
Keep the `Task.Run(...).GetAwaiter().GetResult()` calls in `MainWindow`'s constructor and `OnWindowClosing`, add an explanatory note, and assert via the spec that no such blocking appears in interactive code paths.
- **Why:** the constructor must produce a fully-initialized window synchronously before it is shown; converting to async window construction is a larger change with little payoff. The hazard (deadlock / interactive freeze) is real only outside the boundary.

### Decision 8: Errors route to `IErrorReporter`; progress/success stays in the footer
Two distinct channels:
- **Progress/success** (e.g. "Importing stored source…", "Import completed.") stays in the footer `OperationStatusText` as today.
- **Errors** (caught command failures, `SafeFireAndForget` exceptions, failed background loads) are passed to `IErrorReporter.Report(userMessage, exception)`. The global `SafeFireAndForget` default handler and the command catch both call the reporter.

`IErrorReporter` — and everything behind it (the toast presentation, the file-backed log, the copy-log-path / open-log controls, and the cross-platform file launcher) — is **owned by the separate `error-reporting-surface` change** (capability `error-reporting`). This change depends only on the interface.
- **Why split:** the toast + logging surface is real, reusable surface area that stands on its own and is not specific to concurrency; isolating it keeps this change focused on the UI-thread/observability contract.
- **Parallel build:** code against `IErrorReporter`. If `error-reporting-surface` lands first, wire straight to it; otherwise route to an interim no-op (or footer) reporter and swap when it lands.
- **Why not footer-only:** errors are transient and easily missed in a status line and need actionable affordances — but that rationale, and the toast/log design, live in the other change.

### Decision 9: Busy is shown where the user can identify what is busy — DECIDED
Busy indication is placed so it is unambiguous *what* is loading:
- **Per-tab content reloads** (Prime Gear / Relics / Missions reloading after an import or profile/wishlist switch) show a **local inline busy indicator inside that tab's content area**, bound to that ViewModel's `IsBusy`. The user sees the spinner over the exact view that is updating.
- **Global source operations** (Drop Tables download / import / read-file / export / clear) keep the existing **footer spinner**, since they are app-wide and already presented there.

Decided this way because a single shared footer spinner cannot tell the user *which* view is refreshing; an in-context indicator is self-identifying. Each ViewModel still exposes `IsBusy` uniformly (Decision 4); only the *placement* differs by scope (per-tab vs. footer).
- **Alternative rejected — route every `IsBusy` to one shared footer:** ambiguous about the source of activity, especially when a tab reloads while the footer is otherwise idle.

## Risks / Trade-offs

- **`Task.Run` doesn't make a running SQLite query cancellable mid-flight** → once a synchronous ADO call starts, the `CancellationToken` is not honored until it returns. Mitigation: accepted — the goal of Decision 3 is to keep the work *off the UI thread*; cancellation gives latest-wins at the operation boundary (the superseded continuation no-ops), not mid-query interruption. SQLite queries here are small.
- **Thread-pool pressure / context-affinity** → every DB op now hops to the thread pool. Mitigation: ops are short and infrequent (user-driven); one `Task.Run` per logical operation, not per EF call. DbContext stays confined to a single thread inside each delegate, so no thread-safety violation.
- **Behavioral test churn** → migrating to `CommunityToolkit.Mvvm` changes the command type and `[ObservableProperty]` generates members; existing tests that `new AsyncRelayCommand(...)` or call `ExecuteTaskAsync()` must move to `AsyncRelayCommand`/`ExecuteAsync`. Mitigation: mechanical, done in the migration step; the awaitable execute path still surfaces exceptions for assertions.
- **Parser port change** → `IDropTableParser.Parse` becomes `ParseAsync` (the adapter offloads its regex work via `Task.Run`). Decided: the port should always have been async, since it is the heaviest CPU step in the import path. Mitigation: pre-V1 allows the signature change; it is mechanical — callers in the import/refresh workflows and their tests `await` instead of calling synchronously.
- **Continuation marshaling** → ViewModel continuations after `await` must land on the UI thread to mutate `ObservableCollection`s. Mitigation: the no-`ConfigureAwait(false)`-in-ViewModels invariant (Decision 6) and a test that mutations occur on the UI thread.
- **Cancellation introduces partial-reload states** → a cancelled reload that already `Clear()`ed a collection could leave it empty. Mitigation: build new content into a local list first, then swap into the bound collection in one final, post-cancellation-check step.

## Migration Plan

Pre-V1, no runtime migration needed. Sequence: (1) add `ReliquaryDbExecutor` + a test proving store calls run off the calling thread; (2) retrofit every store/reader to route through it; (3) make `HtmlDropTableParser` offload its parse (async port); (4) add the two packages and migrate the command/INPC surface to `CommunityToolkit.Mvvm`; (5) route constructor/property-setter fire-and-forget through `SafeFireAndForget` + global handler; (6) add `BusyScope` + reload serialization/cancellation to content ViewModels and the shell fan-out; (7) document/confine startup-shutdown blocking; (8) adjust existing tests. Each step builds and tests green before the next. Rollback is per-commit revert.

## Open Questions

None — all resolved. Reloads use cancel-and-replace/latest-wins (Decision 5); busy is shown per-tab for content reloads and in the footer for global source operations (Decision 9).
