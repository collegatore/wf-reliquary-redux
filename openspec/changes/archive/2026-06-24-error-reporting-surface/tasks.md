## 1. Error log (port + file implementation)

- [x] 1.1 Add an `IErrorLog` port (Application) with an append method (timestamp + message + exception detail) and a `LogFilePath` property.
- [x] 1.2 Implement a file-writing `IErrorLog` in Infrastructure under `…/WF Reliquary Redux/logs/app.log` (resolve the directory like the DB path; create on first write; serialize appends so concurrent reports don't interleave).
- [x] 1.3 Test: appending creates the file when absent, writes a timestamped entry with the exception detail, and `LogFilePath` returns the absolute path.

## 2. Cross-platform file launcher

- [x] 2.1 Add an `IFileLauncher` with `Open(string path)`: Windows → `Process.Start(new ProcessStartInfo(path){ UseShellExecute = true })`; Linux → `Process.Start("xdg-open", path)`, selected by `OperatingSystem.IsWindows()`/`IsLinux()`.
- [x] 2.2 Catch and report launch failures (never throw to the caller).
- [x] 2.3 Test the OS-selection logic (e.g. via an injectable process-start delegate) for the Windows and Linux branches.

## 3. Error reporter + toast surface

- [x] 3.1 Implement `IErrorReporter` (`ErrorReporter` in Presentation) to append via `IErrorLog`, set the footer status text to `"{userMessage}. Refer to logs for the error details."` via an injected `Action<string>` delegate, then raise a toast — all posted to `Dispatcher.UIThread` so background callers need not marshal.
- [x] 3.2 Make `Report` defensive: a failing log write still attempts the toast and footer update and vice versa; reporting never throws.
- [x] 3.3 Host toast overlay panel in `MainWindow`; gracefully skipped if `SetToastPanel` not called (still logs + sets footer). Custom `ErrorToastCard` control used instead of `WindowNotificationManager` to support two action buttons.
- [x] 3.4 Give the error toast two controls — "Copy log path" (`TopLevel.Clipboard.SetTextAsync(IErrorLog.LogFilePath)`) and "Open log" (`IFileLauncher` on the log path) — with copy-path as the always-available fallback.
- [x] 3.5 Configure the toast with no expiry (manual dismiss only) — close button removes the card from the panel.

## 4. Settings — Maintenance section

- [x] 4.1 Add a Maintenance section to `SettingsView.axaml` with an "Open log file" button (always enabled).
- [x] 4.2 Add `OpenLogFileCommand` to `SettingsViewModel`; inject via `Action? OpenLogFile` delegate (set by MainWindow) so SettingsViewModel has no Infrastructure dependency.

## 5. Composition wiring

- [x] 5.1 Wire `IErrorLog`, `IFileLauncher`, and `IErrorReporter` in `ReliquaryServiceFactory`; construct `ErrorReporter` in `MainWindow` (after `InitializeComponent`); pass footer-update delegate; wire `settingsVm.OpenLogFile` delegate.

## 6. Verification

- [x] 6.1 Test: reporting an error appends a log entry; footer format is `"{userMessage}. Refer to logs for the error details."`; `LogFilePath` is absolute (clipboard-actionable); "Open log" failure is swallowed. Toast insertion requires Avalonia UIThread and is verified manually.
- [x] 6.2 Run `dotnet build` and `dotnet test`; confirm green (do not launch the app unannounced, per project rules).
- [x] 6.3 Note in the change/PR that a new log file (`logs/app.log`) is created at first error — new stored format under the pre-V1 persistence rule (no migration).
- [x] 6.4 Update `docs/technical-reference.md` with the error-reporting surface (log location, toast, footer behavior, controls, Settings button) so future code routes failures through `IErrorReporter`.
