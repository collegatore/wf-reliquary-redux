## 1. Crash handler

- [ ] 1.1 Create `StartupCrashHandler` in the Presentation project: `Handle(Exception ex, IErrorLog errorLog)` that appends the full exception detail (blocking on `AppendAsync`; no SynchronizationContext is alive) inside its own try/catch, then shows the fatal-error dialog
- [ ] 1.2 Implement the fatal-error dialog: P/Invoke `MessageBoxW` on Windows (error icon, app title, message with exception summary and log-file path); write the same message to stderr on Linux; wrap in try/catch so a dialog failure never blocks exit

## 2. Entry point wiring

- [ ] 2.1 Change `Program.Main` to return `int`; wrap `StartWithClassicDesktopLifetime` in try/catch that calls `StartupCrashHandler.Handle` with a fresh `FileErrorLog` and returns `1`; return `0` on normal shutdown

## 3. Tests

- [ ] 3.1 Integration test: `StartupCrashHandler.Handle` with a throwing scenario appends a timestamped entry containing the exception detail to the log (use a temp-path `FileErrorLog`)
- [ ] 3.2 Integration test: log append failure (unwritable path) does not throw out of `Handle`

## 4. Verification

- [ ] 4.1 Build and run full test suite
- [ ] 4.2 Manual check (ask user): point the app at a database with an incompatible migration history and confirm the dialog appears, the log entry is written, and the exit code is non-zero
