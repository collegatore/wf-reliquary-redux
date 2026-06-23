## 1. Error log (port + file implementation)

- [ ] 1.1 Add an `IErrorLog` port (Application) with an append method (timestamp + message + exception detail) and a `LogFilePath` property.
- [ ] 1.2 Implement a file-writing `IErrorLog` in Infrastructure under `…/WF Reliquary Redux/logs/app.log` (resolve the directory like the DB path; create on first write; serialize appends so concurrent reports don't interleave).
- [ ] 1.3 Test: appending creates the file when absent, writes a timestamped entry with the exception detail, and `LogFilePath` returns the absolute path.

## 2. Cross-platform file launcher

- [ ] 2.1 Add an `IFileLauncher` with `Open(string path)`: Windows → `Process.Start(new ProcessStartInfo(path){ UseShellExecute = true })`; Linux → `Process.Start("xdg-open", path)`, selected by `OperatingSystem.IsWindows()`/`IsLinux()`.
- [ ] 2.2 Catch and report launch failures (never throw to the caller).
- [ ] 2.3 Test the OS-selection logic (e.g. via an injectable process-start delegate) for the Windows and Linux branches.

## 3. Error reporter + toast surface

- [ ] 3.1 Add an `IErrorReporter` seam (`Report(string userMessage, Exception ex)`); implement it to append via `IErrorLog` then raise a toast, posting the toast to the UI thread (`Dispatcher.UIThread`) so background callers need not marshal.
- [ ] 3.2 Make `Report` defensive: a failing log write still attempts the toast and vice versa; reporting never throws.
- [ ] 3.3 Host a `WindowNotificationManager` (`Avalonia.Controls.Notifications`) in `MainWindow`; queue/drop the toast gracefully if reported before the window exists (still log).
- [ ] 3.4 Give the error toast two controls — "Copy log path" (`TopLevel.Clipboard.SetTextAsync(IErrorLog.LogFilePath)`) and "Open log" (`IFileLauncher` on the log path) — with copy-path as the always-available fallback.
- [ ] 3.5 Choose the error-toast dismissal behavior (longer/manual-dismiss per the resolved open question) so the controls stay reachable.

## 4. Composition wiring

- [ ] 4.1 Wire `IErrorLog`, `IFileLauncher`, and `IErrorReporter` in `ReliquaryServiceFactory` and expose them to Presentation.

## 5. Verification

- [ ] 5.1 Test: reporting an error appends a log entry and raises a toast; "Copy log path" yields the correct absolute path; "Open log" failure is swallowed.
- [ ] 5.2 Run `dotnet build` and `dotnet test`; confirm green (do not launch the app unannounced, per project rules).
- [ ] 5.3 Note in the change/PR that a new log file (`logs/app.log`) is created at first error — new stored format under the pre-V1 persistence rule (no migration).
- [ ] 5.4 Update `docs/technical-reference.md` with the error-reporting surface (log location, toast, controls) so future code routes failures through `IErrorReporter`.
