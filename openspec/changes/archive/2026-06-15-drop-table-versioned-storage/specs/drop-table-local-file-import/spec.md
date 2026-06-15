## ADDED Requirements

### Requirement: Local File Source Registration

The system SHALL provide an application-layer operation that accepts a local HTML file path, extracts the Last Update value from its content, and inserts the content and provenance into `source_files`. If no Last Update value can be extracted, the operation SHALL fail without inserting a record.

#### Scenario: Local file is registered successfully

- **WHEN** a valid drop-table HTML file with a parseable Last Update value is submitted
- **THEN** a `source_files` row is inserted with the full HTML content, a `file://` URI as source URL, the current timestamp as `downloaded_at`, and the extracted Last Update value

#### Scenario: Local file lacks Last Update

- **WHEN** the submitted HTML file does not contain a parseable Last Update value
- **THEN** the operation fails and no `source_files` row is inserted

#### Scenario: Local file cannot be read

- **WHEN** the submitted file path does not exist or cannot be opened
- **THEN** the operation fails and no `source_files` row is inserted

### Requirement: Import from File UI Action

The Drop Tables tab SHALL provide an **"Read from file"** button that opens a file picker for the user to select a local HTML file, then registers it in `source_files`. This button only acquires and stores the source; it does not trigger parsing or import.

#### Scenario: User selects a file and registration succeeds

- **WHEN** the user activates "Read from file", selects a valid drop-table HTML file, and the registration succeeds
- **THEN** the UI updates the downloaded version status from the newly stored `source_files` record

#### Scenario: User cancels the file picker

- **WHEN** the user activates "Read from file" but dismisses the file picker without selecting a file
- **THEN** no operation is invoked and the UI remains unchanged

#### Scenario: Registration fails

- **WHEN** the user selects a file but the registration fails (unreadable file or missing Last Update)
- **THEN** the UI displays the failure message and the existing `source_files` records are unchanged
