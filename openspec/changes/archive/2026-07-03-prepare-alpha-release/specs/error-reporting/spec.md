## MODIFIED Requirements

### Requirement: Errors are recorded to a log file

The application SHALL append error entries to a log file at a stable, discoverable location alongside the application database (`…/WF Reliquary Redux/logs/app.log`). Each entry SHALL include a timestamp, the application version, and the exception detail (message and stack trace). The log file SHALL be created on first write if it does not exist, and appends SHALL be safe under concurrent reports. The error-log port SHALL expose the absolute log-file path.

#### Scenario: Entry appended on report

- **WHEN** an error is reported
- **THEN** a timestamped entry containing the application version and the exception detail is appended to the log file, creating the file if necessary

#### Scenario: Log path is discoverable

- **WHEN** the UI needs to copy or open the log
- **THEN** the error-log port supplies the absolute path of the log file
