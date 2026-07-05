## MODIFIED Requirements

### Requirement: On-Demand Drop Table Download

The system SHALL provide an application-layer operation that downloads the raw drop-table HTML from the configured source URL when requested. Before saving, the operation SHALL check whether a source with the same Last Update version already exists in `source_files`. If a duplicate version is found, the operation SHALL return a `DuplicateVersion` result carrying the pending `DropTableSourceContent` and the existing row's `Id` without saving. A `DuplicateVersion` result is distinguishable from both success and failure: it SHALL carry no error message, and consumers SHALL treat it as a pending confirmation rather than reporting it as an error. If no duplicate is found, the operation SHALL save the downloaded content as before and its success result SHALL carry the id of the newly saved `source_files` row, so callers can act on that source (for example, importing it) without a separate lookup.

#### Scenario: Download succeeds with new version

- **WHEN** the user requests a drop-table download, the configured URL returns successful HTML content with a parseable Last Update value, and no existing `source_files` row has the same Last Update value
- **THEN** the system stores the downloaded HTML content and provenance in the `source_files` DB table and returns a success result carrying the id of the newly inserted row

#### Scenario: Download produces a duplicate version

- **WHEN** the user requests a drop-table download, the downloaded HTML has a Last Update value that matches an existing `source_files` row
- **THEN** the system returns a `DuplicateVersion` result carrying the pending content and the existing row's ID without inserting a new row, and the result carries no error message

#### Scenario: Downloaded file lacks Last Update

- **WHEN** the user requests a drop-table download and the downloaded file does not contain a parseable Last Update value
- **THEN** the system reports a failed operation and does not insert a row into `source_files`

#### Scenario: Download fails

- **WHEN** the user requests a drop-table download and the configured URL cannot be retrieved successfully
- **THEN** the system reports a failed operation without modifying any existing `source_files` records
