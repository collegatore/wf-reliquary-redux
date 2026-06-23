## ADDED Requirements

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

### Requirement: Error toasts expose log-access controls

An error toast SHALL provide a control to copy the absolute log-file path to the clipboard and a control to open the log file in the operating system's default handler. Either control failing SHALL NOT crash the application, and the copy-path control SHALL remain available as a fallback when opening the log is not possible.

#### Scenario: Copy log path
- **WHEN** the user activates the toast's "Copy log path" control
- **THEN** the absolute path of the application log file is placed on the system clipboard

#### Scenario: Open log
- **WHEN** the user activates "Open log"
- **THEN** the log file is opened in the operating system's default handler

#### Scenario: Open log fails gracefully
- **WHEN** no default handler is available or the launch fails
- **THEN** the failure is caught (no crash) and the user can still use "Copy log path" to locate the file manually

### Requirement: Opening files works on Windows and Linux

The file launcher SHALL open a path using shell execution on Windows (`ProcessStartInfo` with `UseShellExecute = true`) and `xdg-open` on Linux, selected at runtime by the operating system. macOS is not a target. A launch failure SHALL be caught and reported without crashing.

#### Scenario: Open on Windows
- **WHEN** "Open log" is activated on Windows
- **THEN** the file is opened via shell execution in the registered default handler

#### Scenario: Open on Linux
- **WHEN** "Open log" is activated on Linux
- **THEN** the file is opened via `xdg-open` (the desktop default handler)
