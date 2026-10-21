## Context

`Program.Main` starts Avalonia via `StartWithClassicDesktopLifetime`, which synchronously constructs `MainWindow`. The window constructor is the designated startup boundary: it blocks on settings load, database migration (`SqliteDatabaseInitializer.InitializeAsync`), and profile/wishlist seeding. Any exception on this path unwinds through `Main` with no handler — the process dies with no log entry and no user-visible message. The existing error-reporting surface (`IErrorReporter` → toast + footer + log) cannot help here: it is wired up mid-constructor and its UI hosts (toast panel, footer) may not exist yet when the failure happens.

The concrete motivating failure: a database whose EF migration history predates the 1.0.0 squash makes `MigrateAsync` throw, and the app appears to "not run" with zero diagnostics.

## Goals / Non-Goals

**Goals:**
- Any exception that unwinds `Main` is appended to `app.log` before the process exits.
- The user sees a dialog stating the app could not start, with the exception summary and the log-file path.
- The process exits with a non-zero exit code after the failure is reported.
- The reporting path depends on as little of the app as possible (it must work when the app failed to construct).

**Non-Goals:**
- Recovering from or retrying the failed startup (e.g. offering a database reset) — separate change.
- Changing toast/footer reporting for post-startup errors.
- Catching failures on background threads that do not unwind `Main` (already covered by `SafeFireAndForget` and `TaskScheduler.UnobservedTaskException`).

## Decisions

**Catch in `Main`, around `StartWithClassicDesktopLifetime`.** This is the single choke point every synchronous startup failure unwinds through, so one handler covers settings load, migration, seeding, and any future constructor work. It also incidentally catches post-startup UI-thread crashes that unwind the message loop — acceptable and strictly better than today's silent death. Alternative considered: try/catch inside the `MainWindow` constructor — rejected because it fragments coverage (each blocking call would need wrapping) and the constructor cannot cleanly abort the lifetime.

**Log synchronously by blocking on `FileErrorLog.AppendAsync`.** In the catch block the Avalonia loop is dead and there is no `SynchronizationContext`, so `GetAwaiter().GetResult()` cannot deadlock — same rationale as the existing startup boundary. No new sync API on `IErrorLog`. The handler constructs its own `FileErrorLog` (default path) rather than relying on `AppErrors.Reporter`, which may still be the no-op reporter if the failure occurred before wiring.

**Native dialog, not Avalonia.** The failure may have occurred before or during Avalonia/window initialization, and `AppBuilder.Configure` cannot reliably run a second lifetime in the same process. On Windows, P/Invoke `MessageBoxW` (error icon, app title, message + log path). On Linux, write the same message to stderr (a desktop-toolkit-free dialog is not worth the dependency; stderr is visible when launched from a terminal, and the log file carries the detail). Wrapped in its own try/catch — dialog failure must never mask the exit path.

**Non-zero exit code.** `Main` returns `int`; the catch path returns `1` after logging and showing the dialog. Callers/scripts can distinguish crash-on-start from clean exit.

**Structure: `StartupCrashHandler` static helper** in the Presentation project with `Handle(Exception ex)` doing log-then-dialog. The log step takes an `IErrorLog` (injectable) so an integration test can assert the entry; the dialog step is a thin OS wrapper excluded from coverage.

## Risks / Trade-offs

- [Logging itself throws (disk full, permissions)] → the handler wraps the log step in try/catch and still shows the dialog; the dialog text does not depend on the log write succeeding.
- [Message box blocks a headless/CI run] → the dialog is only invoked from the interactive `Main` path, never from tests; tests target the log step directly.
- [Catch-all also fires for post-startup UI-thread crashes, showing a "could not start" message late in a session] → message wording is generic enough ("encountered a fatal error and must close"); refining per-phase wording is not worth extra state.
- [Blocking on `AppendAsync` in a crashed process] → no message loop is alive; same justified pattern as the existing startup boundary comment.
