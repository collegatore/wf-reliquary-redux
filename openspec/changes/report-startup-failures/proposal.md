## Why

A failure during synchronous startup (settings load, database migration, seeding in the `MainWindow` constructor) escapes `Program.Main` unhandled and kills the process with no dialog and no log entry — the user sees the app "not run" with zero diagnostics. This happened in practice: the 1.0.0 build dies silently on machines whose database carries a pre-squash EF migration history, and the only trace is the absence of anything after the version line in `app.log`.

## What Changes

- Wrap the top-level startup path in `Program.Main` with a catch-all that:
  - appends the full exception detail to the existing error log (`FileErrorLog`), synchronously, so the entry is flushed before the process exits;
  - surfaces a startup-failure dialog telling the user the app could not start, showing the error summary and the log-file path.
- The dialog must work even though the main window failed to construct (the toast/footer reporting surface is unavailable at this point).
- Process exits with a non-zero exit code after the dialog is dismissed.
- No change to the existing toast/footer error reporting for post-startup errors.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `error-reporting`: add a requirement that unhandled startup failures are logged to the error log and surfaced via a standalone startup-failure dialog (not a toast), and that the process then exits non-zero instead of dying silently.

## Impact

- `src/Presentation/WF.Reliquary.Presentation/Program.cs` — top-level catch around `StartWithClassicDesktopLifetime`.
- `src/Presentation/WF.Reliquary.Presentation/MainWindow.axaml.cs` — startup boundary is the code being protected; may need a marker/wrapper so failures carry a user-facing phase description.
- New minimal startup-failure dialog (Avalonia window or platform fallback) in the Presentation project.
- `FileErrorLog` — needs a synchronous append path (or a safely awaited call) usable during process teardown.
- Tests: integration coverage for "startup failure is logged"; dialog behavior verified manually.
