## ADDED Requirements

### Requirement: Fatal startup failures are logged and surfaced

Any exception that unwinds the application entry point (`Program.Main`) SHALL be caught by a top-level handler that first appends the full exception detail to the error log and then presents a fatal-error dialog, instead of letting the process die silently. The log append SHALL complete (flush) before the dialog is shown and before the process exits. The handler SHALL NOT depend on the main window or the toast/footer reporting surface having been constructed.

#### Scenario: Startup failure is logged
- **WHEN** an exception (e.g. a database migration failure) unwinds `Program.Main`
- **THEN** a timestamped entry with the application version and full exception detail is appended to `app.log` before the process exits

#### Scenario: Failure before error-reporter wiring
- **WHEN** the failure occurs before the toast/footer error reporter has been wired up
- **THEN** the handler still logs and shows the fatal-error dialog using its own error-log instance

#### Scenario: Log append failure does not mask the dialog
- **WHEN** appending to the error log itself fails (e.g. disk full)
- **THEN** the fatal-error dialog is still shown and the process still exits non-zero

### Requirement: Fatal-error dialog is native and self-contained

The fatal-error dialog SHALL state that the application encountered a fatal error and must close, and SHALL include the exception summary and the absolute log-file path. On Windows it SHALL be shown as a native message box (independent of Avalonia); on Linux the same message SHALL be written to standard error. A failure to show the dialog SHALL NOT prevent the process from exiting.

#### Scenario: Dialog on Windows
- **WHEN** the top-level handler runs on Windows
- **THEN** a native message box shows the fatal-error message, exception summary, and log-file path

#### Scenario: Message on Linux
- **WHEN** the top-level handler runs on Linux
- **THEN** the fatal-error message, exception summary, and log-file path are written to standard error

### Requirement: Fatal startup failure exits non-zero

After logging and surfacing a fatal startup failure, the process SHALL exit with a non-zero exit code. A normal shutdown SHALL continue to exit with code zero.

#### Scenario: Crash exit code
- **WHEN** the top-level handler completes after a fatal failure
- **THEN** the process exit code is non-zero

#### Scenario: Normal exit unchanged
- **WHEN** the application shuts down normally
- **THEN** the process exit code is zero
