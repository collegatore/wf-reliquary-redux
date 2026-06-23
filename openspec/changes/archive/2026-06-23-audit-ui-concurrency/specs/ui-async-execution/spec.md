## ADDED Requirements

### Requirement: UI thread is never blocked by application work

The Presentation layer SHALL NOT block the UI thread on application work (database access, HTTP, file I/O, HTML parsing, or any other potentially long-running operation). Because the SQLite EF Core provider executes its `…Async` methods synchronously, awaiting a store/reader method from the UI thread SHALL NOT, by itself, be relied upon to keep the UI responsive; the offloading SHALL be performed by the layer that owns the blocking work (see the data-layer and parser requirements below), so that a ViewModel can simply `await` and the UI thread is freed at the first `await`.

#### Scenario: Long-running command keeps the UI responsive
- **WHEN** the user triggers a command that downloads and parses a drop-table source, imports it, reads a local file, exports sources, or reloads a content view from the database
- **THEN** the synchronous/CPU-bound portion of that work executes on a thread-pool thread, not the UI thread, and the window remains responsive (the busy indicator animates) for the duration

#### Scenario: ViewModels do not wrap calls in Task.Run
- **WHEN** a ViewModel invokes a store, reader, or workflow
- **THEN** it awaits the call directly without wrapping it in `Task.Run`, because offloading is the responsibility of the data-access seam and the parser adapter, not the consumer

### Requirement: Database work is offloaded at the data-context seam

All database access SHALL be offloaded to a thread-pool thread at a single seam that owns the `ReliquaryDbContext` lifetime (a unit-of-work executor in Infrastructure), NOT wrapped per consumer method and NOT wrapped per individual EF Core call. Each logical store/reader operation SHALL execute its entire context-bound body within one offloaded unit of work, and the `ReliquaryDbContext` SHALL remain confined to the single thread on which that unit of work runs.

#### Scenario: Store call runs off the calling thread
- **WHEN** any `Sqlite*` store or reader method is invoked
- **THEN** its `ReliquaryDbContext` creation and all EF Core calls within that method execute on a thread-pool thread, so the calling (UI) thread is not blocked for the duration of the query

#### Scenario: One offload per logical operation
- **WHEN** a store method issues several EF Core round-trips against one context
- **THEN** the whole method body runs inside a single offloaded unit of work (one context lifetime), not a separate offload per EF call and not an offload applied by the caller

### Requirement: Synchronous CPU adapters offload their own work

Infrastructure adapters that perform synchronous CPU-bound work — notably HTML drop-table parsing — SHALL offload that work themselves so callers (workflows, ViewModels) await a non-blocking operation.

#### Scenario: HTML parse does not run on the UI thread
- **WHEN** a downloaded drop-table source is parsed during refresh or import
- **THEN** the CPU-bound parse executes on a thread-pool thread, and the consumer awaits it rather than wrapping it in `Task.Run` itself

### Requirement: Asynchronous failures are observed and reported

Every asynchronous operation started from the Presentation layer SHALL have its exceptions observed. No `async` operation may be started with a discarded task (`_ = SomeAsync()`) unless it is routed through a shared safe-fire-and-forget helper that catches exceptions and reports them to the error-reporting surface (`IErrorReporter`, provided by the `error-reporting` capability). Command execution SHALL catch exceptions from the awaited work and report them rather than allowing an unhandled `async void` exception. Progress and success messages SHALL remain in the footer status and SHALL NOT be routed to the error surface.

#### Scenario: Fire-and-forget persistence failure is reported
- **WHEN** a ViewModel persists state in response to a property change (e.g. toggling a wishlisted part or owned gear) and the persistence task throws
- **THEN** the exception is caught by the safe fire-and-forget helper and passed to `IErrorReporter` instead of being silently swallowed

#### Scenario: Command failure does not crash the app
- **WHEN** the work awaited inside a command throws
- **THEN** the command catches the exception, reports it to `IErrorReporter`, restores the not-busy state, and the application continues running

#### Scenario: Background load failure on startup is reported
- **WHEN** a content ViewModel's initial load (started from its constructor) fails
- **THEN** the failure is reported to `IErrorReporter` rather than leaving the view silently empty with no explanation

#### Scenario: Success and progress stay in the footer
- **WHEN** an operation reports progress or completes successfully
- **THEN** the message appears in the footer status text and is not routed to the error surface

### Requirement: ViewModels expose a uniform busy state shown where the activity originates

Each ViewModel that runs awaitable operations in response to user actions or reload triggers SHALL expose a busy state (`IsBusy`), and commands SHALL be disabled while their ViewModel is busy. Busy indication SHALL be placed so the user can identify *what* is busy: per-tab content reloads (Prime Gear / Relics / Missions) SHALL show a local busy indicator within that tab's content area, while global source operations (Drop Tables download / import / read-file / export / clear) SHALL use the shared footer indicator.

#### Scenario: Commands disabled while busy
- **WHEN** an operation is in progress on a ViewModel
- **THEN** that ViewModel's `IsBusy` is true, its commands report `CanExecute` as false, and `IsBusy` returns to false in a `finally` block whether the operation succeeded or threw

#### Scenario: Content reload shows an in-context indicator
- **WHEN** the Prime Gear, Relics, or Missions view reloads after an import or profile/wishlist switch
- **THEN** a busy indicator is shown within that view's content area (not only in the footer), so the user can tell which view is updating

#### Scenario: Global source operation shows the footer indicator
- **WHEN** a Drop Tables source operation (download, import, read-file, export, clear) runs
- **THEN** the shared footer busy indicator is shown for its duration

### Requirement: Overlapping and reentrant operations are guarded

A ViewModel SHALL NOT run two instances of the same mutating operation concurrently. Operations that mutate bound `ObservableCollection`s (content reloads) SHALL be serialized so a second trigger cannot interleave mutations with an in-flight reload. Re-entrant command invocations SHALL be ignored or queued, never run in parallel against shared state.

#### Scenario: Rapid profile switches do not corrupt the view
- **WHEN** the active profile (or wishlist) is switched again before the dependent reloads from the previous switch have completed
- **THEN** reloads are serialized (or the superseded one is cancelled) so the bound collections are never mutated by two reloads at once and the final view reflects the latest selection

#### Scenario: Double-trigger of a busy command is ignored
- **WHEN** a command whose ViewModel is already busy is invoked again
- **THEN** the second invocation returns immediately without starting a parallel operation

### Requirement: UI operations support cancellation

ViewModel operations that perform I/O SHALL accept and honor a `CancellationToken`, consistent with the project-wide convention that all I/O methods are cancellable. When a reload is superseded by a newer trigger, the older reload SHALL be cancelled (latest-wins) so it stops touching shared state once obsolete; intermediate states are not rendered.

#### Scenario: Superseded reload is cancelled
- **WHEN** a reload is triggered while a previous reload for the same view is still running and the previous result is now obsolete
- **THEN** the previous reload's token is cancelled and its continuation does not mutate the bound collections

### Requirement: UI-thread blocking is confined to the startup/shutdown boundary

Blocking on asynchronous work via `.GetAwaiter().GetResult()` SHALL be confined to the application startup (window construction, before the window is shown) and shutdown (window closing) boundaries, where no message loop is being starved, and SHALL be wrapped in `Task.Run` to avoid deadlocking the Avalonia synchronization context. No blocking-on-async call may appear in a command handler, property setter, or any code path that runs while the window is interactive.

#### Scenario: Startup settings load does not deadlock
- **WHEN** the main window is constructed and must load persisted settings synchronously before showing
- **THEN** the load is performed via `Task.Run(...).GetAwaiter().GetResult()` so the UI synchronization context is not deadlocked, and this blocking pattern appears only at the construction/closing boundary

#### Scenario: No blocking-on-async during interaction
- **WHEN** the window is interactive and the user triggers any command or edits any bound property
- **THEN** the resulting work is awaited (never blocked on with `.GetResult()`/`.Result`/`.Wait()`)
