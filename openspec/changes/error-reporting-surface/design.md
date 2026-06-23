## Context

The app is Avalonia 12 + MVVM with a strict four-layer architecture (Presentation → Application ← Infrastructure, wired by `ReliquaryServiceFactory` in Composition; no DI container). Today there is **no logging of any kind** (a grep for `ILogger`/Serilog/`.log`/`Console.Error` finds only `.gitignore`) and **no user-visible error channel** — failures either vanish (fire-and-forget) or, in the Drop Tables tab, get written as a transient footer string. The database lives at `…/WF Reliquary Redux/DropTables/drop-table-import.sqlite`; the log belongs in a sibling `logs/` directory resolved the same way.

This change is split out of `audit-ui-concurrency`, which requires that every asynchronous failure be observed and surfaced. That change needs a sink to route failures to; this change builds it. The two are independent units that meet at the `IErrorReporter` interface.

## Goals / Non-Goals

**Goals:**
- One seam (`IErrorReporter`) any layer can call to report an error.
- Full error detail persisted to an append-only log file at a discoverable, stable path.
- A dismissible, stacking toast for errors, with Copy-log-path and Open-log affordances.
- Cross-platform "open log" (Windows + Linux), with copy-path as the guaranteed fallback.
- Minimal footprint consistent with the project's hand-wired, low-dependency style.

**Non-Goals:**
- A general-purpose / structured-logging stack (`Microsoft.Extensions.Logging`, Serilog). This is error capture, not diagnostic tracing.
- Log rotation/retention policy, remote/telemetry sinks (can be added later if needed).
- OS-native notification center integration; macOS support.
- Changing how progress/success is shown (stays in the footer).

## Decisions

### Decision 1: A minimal `IErrorLog` port + file implementation, not a logging framework
`IErrorLog` (Application) exposes `Task AppendAsync(string message, Exception? ex, CancellationToken)` (or a sync append — see Open Questions) and `string LogFilePath { get; }`. Infrastructure implements it by appending timestamped lines to `…/WF Reliquary Redux/logs/app.log`, creating the directory/file on first write, with appends serialized (lock or single-writer) so concurrent reports don't interleave.
- **Why:** "copy/open the log" requires a log to exist; a tiny append-only file is enough and matches the project's no-framework stance.
- **Alternative — `Microsoft.Extensions.Logging` + file provider:** heavier, pulls in a config/DI idiom the project avoids. Rejected.

### Decision 2: Toast via Avalonia `WindowNotificationManager`
Errors present as in-window toasts using `Avalonia.Controls.Notifications.WindowNotificationManager`, hosted once by `MainWindow`. The toast shows the short user-facing message; detail is in the log.
- **Why:** built into Avalonia (no dependency), uniform across Windows/Linux, dismissible and stacking, and can host action controls.
- **Alternative — OS-native notifications:** not uniformly available across target OSes; harder to attach actions. Rejected.
- **Alternative — footer-only:** transient and easily missed; can't host log affordances. Rejected (footer keeps progress/success only).

### Decision 3: `IErrorReporter` is the consumer-facing seam
Consumers depend on `IErrorReporter.Report(string userMessage, Exception ex)` rather than on the log or toast directly. The implementation (Presentation) writes to `IErrorLog` then raises the toast on the UI thread. This is the single integration point `audit-ui-concurrency` targets.
- **Why:** decouples "something failed" from "how it's shown/stored"; lets the concurrency change code against a stable interface and keeps testing simple (assert the reporter was called / the log grew).
- **Marshaling:** `Report` may be called from a background continuation; the toast is posted to the UI thread (`Dispatcher.UIThread`) inside the reporter, so callers need not marshal.

### Decision 4: Cross-platform `IFileLauncher`
A small `IFileLauncher.Open(string path)` selected at runtime: Windows → `Process.Start(new ProcessStartInfo(path){ UseShellExecute = true })`; Linux → `Process.Start("xdg-open", path)`. Launch failures are caught and reported (never thrown to the UI).
- **Why:** `UseShellExecute = true` with a bare path does not reliably open files on Linux; `xdg-open` is the portable convention. Copy-path remains the fallback when no handler exists (headless/minimal Linux).

## Risks / Trade-offs

- **`xdg-open` absent on barebones Linux** → "Open log" no-ops/fails → Mitigation: caught; Copy-log-path always works.
- **Toast hosting requires a `TopLevel`/window** → reporting before the window exists (early startup) has nowhere to show → Mitigation: still log to file; queue or drop the toast if no window yet (acceptable — startup errors are rare and logged).
- **Reporting an error must never itself throw** → a failing log write or toast must not cascade → Mitigation: the reporter wraps both steps defensively; a log-write failure still attempts the toast and vice versa.
- **Unbounded log growth** → no rotation in this change → Mitigation: accepted for now; append-only, small entries; rotation is a later enhancement.

## Migration Plan

Pre-V1, no runtime migration. Sequence: (1) `IErrorLog` port + file impl + test; (2) `IFileLauncher` (Win/Linux) + test; (3) `IErrorReporter` impl (log + toast) and `WindowNotificationManager` hosting in `MainWindow`; (4) toast controls (copy/open); (5) wire all in `ReliquaryServiceFactory`; (6) `audit-ui-concurrency` points its `SafeFireAndForget` handler and command catch at `IErrorReporter`. Rollback is per-commit revert. A log file is created at first error (note to developer per the pre-V1 persistence rule — new stored format, no migration).

## Open Questions

- **`IErrorLog.AppendAsync` vs. synchronous append:** async fits the project's "all I/O is async" convention, but error reporting from a catch block is simpler if fire-and-forget; likely async with the reporter awaiting/observing it.
- **Toast auto-dismiss vs. sticky for errors:** errors may warrant a longer or manual-dismiss duration so the log controls stay reachable. (Leaning: longer, manually dismissible.)
- **Should `IErrorReporter` live in Application (port) or Presentation only?** Application placement lets non-UI layers report; Presentation-only keeps Avalonia types out of Application. (Leaning: `IErrorReporter` in Application as a port, toast impl in Presentation.)
