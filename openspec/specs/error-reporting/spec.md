## Purpose

Defines the error-reporting surface: how runtime failures are captured, persisted, surfaced to the user, and made discoverable via persistent controls.

## Requirements

### Requirement: A single error-reporting surface

The application SHALL provide one error-reporting seam (`IErrorReporter`) that consumers call with a short user-facing message and the originating exception. Reporting an error SHALL both persist the full detail to the application log file and present the error to the user as a toast. The surface SHALL be used for errors only; progress and success messages SHALL remain in the footer status and SHALL NOT be raised as error toasts.

#### Scenario: Reporting an error logs and notifies
- **WHEN** a consumer reports an error through `IErrorReporter`
- **THEN** the full exception detail is appended to the log file and a toast is shown with the user-facing message

#### Scenario: Progress and success are not toasts
- **WHEN** an operation reports progress or completes successfully
- **THEN** the message appears in the footer status and no error toast is raised

### Requirement: Errors are recorded to a log file

The application SHALL append error entries to a log file at a stable, discoverable location alongside the application database (`…/WF Reliquary Redux/logs/app.log`). Each entry SHALL include a timestamp and the exception detail (message and stack trace). The log file SHALL be created on first write if it does not exist, and appends SHALL be safe under concurrent reports. The error-log port SHALL expose the absolute log-file path.

#### Scenario: Entry appended on report
- **WHEN** an error is reported
- **THEN** a timestamped entry containing the exception detail is appended to the log file, creating the file if necessary

#### Scenario: Log path is discoverable
- **WHEN** the UI needs to copy or open the log
- **THEN** the error-log port supplies the absolute path of the log file

### Requirement: Error toasts expose a log-access control

An error toast SHALL provide a control to open the log file in the operating system's default handler. The control failing SHALL NOT crash the application. Copying the log path to the clipboard is available from the Settings / Maintenance section rather than from individual toasts.

#### Scenario: Open log from toast
- **WHEN** the user activates "Open log" on an error toast
- **THEN** the log file is opened in the operating system's default handler

#### Scenario: Open log fails gracefully
- **WHEN** no default handler is available or the launch fails
- **THEN** the failure is caught (no crash)

### Requirement: Opening files works on Windows and Linux

The file launcher SHALL open a path using shell execution on Windows (`ProcessStartInfo` with `UseShellExecute = true`) and `xdg-open` on Linux, selected at runtime by the operating system. macOS is not a target. A launch failure SHALL be caught and reported without crashing.

#### Scenario: Open on Windows
- **WHEN** "Open log" is activated on Windows
- **THEN** the file is opened via shell execution in the registered default handler

#### Scenario: Open on Linux
- **WHEN** "Open log" is activated on Linux
- **THEN** the file is opened via `xdg-open` (the desktop default handler)

### Requirement: Errors are acknowledged in the footer

When an error is reported, the application footer SHALL display a terse, operation-named acknowledgment of the form `"{userMessage}. Refer to logs for the error details."` The footer message SHALL remain visible until the next operation updates the footer status (it is not time-limited). This provides a persistent, low-noise acknowledgment for users who missed or dismissed the toast.

#### Scenario: Footer shows error acknowledgment
- **WHEN** an error is reported through `IErrorReporter`
- **THEN** the footer status text is set to `"{userMessage}. Refer to logs for the error details."` and remains until the next operation clears or replaces it

#### Scenario: Footer error message names the operation
- **WHEN** a specific operation fails (e.g. drop-table parsing)
- **THEN** the footer text identifies the operation (e.g. "Drop tables parsing failed. Refer to logs for the error details.") rather than showing a generic message

### Requirement: Settings tab exposes log-file access

The Settings tab SHALL include a Maintenance section with two always-enabled controls: "Open log file" and "Copy log path". "Open log file" SHALL open the log file in the OS default handler; if no log file exists yet it SHALL open the log directory instead (creating it if necessary) so the user can see where logs will appear. "Copy log path" SHALL place the absolute log-file path on the system clipboard. Both controls SHALL always be enabled regardless of whether a log file currently exists.

#### Scenario: Open log file from Settings
- **WHEN** the user activates "Open log file" in the Settings / Maintenance section and a log file exists
- **THEN** the log file is opened via `IFileLauncher` in the OS default handler

#### Scenario: Open log file before any error has occurred
- **WHEN** the user activates "Open log file" and no log file has been written yet
- **THEN** the log directory is created if needed and opened in the OS file manager, showing the user where logs will appear

#### Scenario: Copy log path from Settings
- **WHEN** the user activates "Copy log path" in the Settings / Maintenance section
- **THEN** the absolute path of the log file is placed on the system clipboard
