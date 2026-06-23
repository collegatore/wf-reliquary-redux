## Why

The application has no logging and no user-visible error channel today: when something fails, the detail is lost and the user sees nothing (or, at best, a transient footer string). The companion `audit-ui-concurrency` change makes "every asynchronous failure is observed" a requirement, which needs a real place to send those failures. This change provides that place: a single error-reporting surface — a toast notification backed by an append-only application log file, with controls to copy the log path or open the log — so any layer can report an error and the user gets an actionable, dismissible notification while full detail is persisted for diagnosis.

## What Changes

- Add an `IErrorReporter` seam that any consumer calls with a user-facing message + the exception; it both logs and shows a toast. This is the abstraction the concurrency contract routes failures to.
- Add an `IErrorLog` port (Application) and a file-writing implementation (Infrastructure) that appends timestamped error entries (message + stack trace) to `…/WF Reliquary Redux/logs/app.log` (path resolved like the database path), creating it on first write. The port exposes the absolute log-file path.
- Add an in-window **toast** surface using Avalonia's `WindowNotificationManager` (`Avalonia.Controls.Notifications`) hosted by `MainWindow`, shown for errors (not for progress/success, which remain in the footer).
- Each error toast carries **Copy log path** (clipboard) and **Open log** controls.
- Add a cross-platform `IFileLauncher` that opens a path via shell-execute on Windows and `xdg-open` on Linux (macOS not targeted), catching launch failures; **Copy log path** is the always-available fallback.
- Wire `IErrorLog` / `IFileLauncher` / `IErrorReporter` in `ReliquaryServiceFactory`.

## Capabilities

### New Capabilities
- `error-reporting`: The application's error-reporting surface — the `IErrorReporter` seam, the file-backed error log (`IErrorLog`), the toast presentation, the copy-log-path / open-log controls, and the cross-platform file launcher.

### Modified Capabilities
<!-- None. This is a new cross-cutting capability; consumers (e.g. the UI concurrency contract) reference it without changing their own spec-level requirements here. -->

## Impact

- **Application:** new `IErrorLog` port (append entry; expose `LogFilePath`). Optionally `IErrorReporter` if defined at this layer; otherwise the reporter lives in Presentation over the port.
- **Infrastructure:** file-writing `IErrorLog` implementation (thread-safe append) under `…/WF Reliquary Redux/logs/app.log`; `IFileLauncher` (Windows shell-execute / Linux `xdg-open`).
- **Composition:** `ReliquaryServiceFactory` wires `IErrorLog`, `IFileLauncher`, and the reporter.
- **Presentation:** a `WindowNotificationManager` hosted by `MainWindow`; an `IErrorReporter` implementation that logs + raises a toast with the two controls.
- **Dependencies:** none new — toasts use Avalonia's built-in `Avalonia.Controls.Notifications`; log + launcher use `System.IO` / `System.Diagnostics.Process`.
- **Relationship to `audit-ui-concurrency`:** that change consumes `IErrorReporter` as its failure sink. The two can be built in parallel against the interface; if this change lands first, the concurrency change wires straight to it, otherwise the concurrency change targets the interface and this change supplies the implementation.
- **Persistence:** a new log file is created at first error; no database/schema impact.
