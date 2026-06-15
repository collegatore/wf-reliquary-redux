## MODIFIED Requirements

### Requirement: Downloaded Source Content DB Record

The system SHALL insert one row into the `source_files` table for each successful drop-table download. The row SHALL store the full raw HTML content, source URL, download timestamp, and the source Drop Tables version value. Rows are never deleted by the import operation.

#### Scenario: Download succeeds and record is inserted

- **WHEN** a drop-table HTML file is downloaded successfully with a parseable Drop Tables version value
- **THEN** a row is inserted into `source_files` with the full HTML content, source URL, ISO 8601 download timestamp, and source Drop Tables version value

#### Scenario: Source file records are preserved across imports

- **WHEN** a drop-table import runs
- **THEN** all rows in `source_files` remain unchanged

#### Scenario: Download file missing Drop Tables version value

- **WHEN** a drop-table HTML file is downloaded but does not contain a parseable Drop Tables version value
- **THEN** the system raises a validation error with the message "Downloaded drop-table file does not contain a Drop Tables version value."

#### Scenario: Local file import missing Drop Tables version value

- **WHEN** a local drop-table HTML file is imported but does not contain a parseable Drop Tables version value
- **THEN** the system reports failure with the message "File does not contain a Drop Tables version value."
