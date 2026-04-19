# drop-table-source-refresh Specification

## Purpose
TBD - created by archiving change add-drop-table-download. Update Purpose after archive.
## Requirements
### Requirement: Configured Drop Table Source URL

The system SHALL use a user-editable application-level Warframe drop-table
source URL and SHALL default to
`https://warframe-web-assets.nyc3.cdn.digitaloceanspaces.com/uploads/cms/hnfvc0o3jnfvc873njb03enrf56.html`
when no user setting is saved.

#### Scenario: Default source URL is used
- **WHEN** the application starts without a configured source URL override
- **THEN** the drop-table refresh workflow uses the preparation-phase URL as
  the source URL

#### Scenario: Source URL is edited by user
- **WHEN** the user saves a different absolute source URL in the application UI
- **THEN** the drop-table refresh workflow uses the configured URL for the next
  download operation

#### Scenario: Invalid source URL is entered
- **WHEN** the user attempts to save a source URL that is not absolute
- **THEN** the application rejects the value and keeps the previously configured
  URL

### Requirement: On-Demand Drop Table Download

The system SHALL provide an application-layer operation that downloads the raw
drop-table HTML from the configured source URL when requested by the user.

#### Scenario: Download succeeds
- **WHEN** the user requests a drop-table download and the configured URL
  returns successful HTML content with a parseable Last Update value
- **THEN** the system stores the downloaded content as the current raw
  drop-table file and records Last Update in provenance

#### Scenario: Downloaded file lacks Last Update
- **WHEN** the user requests a drop-table download and the downloaded file does
  not contain a parseable Last Update value
- **THEN** the system reports a failed operation and does not replace the
  stored raw drop-table file

#### Scenario: Download fails
- **WHEN** the user requests a drop-table download and the configured URL cannot
  be retrieved successfully
- **THEN** the system reports a failed operation without deleting the previously
  stored raw drop-table file

### Requirement: Local Raw File Storage

The system SHALL store the downloaded raw drop-table HTML in a deterministic
local application data location and SHALL preserve lightweight provenance for
the stored file.

#### Scenario: File is stored
- **WHEN** a download succeeds
- **THEN** the raw HTML file exists at the configured local storage path and
  provenance records the source URL, download timestamp, and parsed Last Update
  value

#### Scenario: Stored file is absent
- **WHEN** no raw drop-table file has been downloaded yet
- **THEN** the source status reports that no local file exists

### Requirement: Stored File Last Update Status

The system SHALL expose the source page's Last Update value from stored-file
provenance when the stored file exists.

#### Scenario: Last Update provenance exists
- **WHEN** a stored raw drop-table file has provenance
- **THEN** the source status includes the provenance Last Update value for
  display

### Requirement: Drop Table Download UI Action

The Avalonia UI SHALL provide a visible control that allows the user to start
the drop-table download operation.

#### Scenario: User starts download
- **WHEN** the user activates the download control
- **THEN** the UI invokes the application-layer drop-table refresh operation
  and prevents duplicate concurrent refresh requests

#### Scenario: Download completes from UI
- **WHEN** a UI-started download completes
- **THEN** the UI updates its status from the newly stored file status

### Requirement: Drop Table Source URL UI Configuration

The Avalonia UI SHALL show the configured drop-table source URL and SHALL allow
the user to edit and save that URL as application-level user settings.

#### Scenario: User views configured URL
- **WHEN** the UI loads
- **THEN** the source URL field displays the saved application setting or the
  default source URL when no setting exists

#### Scenario: User saves configured URL
- **WHEN** the user enters a valid absolute URL and saves it
- **THEN** the application persists the URL for future refresh operations

### Requirement: Drop Table Status Field

The Avalonia UI SHALL show a status field for the stored drop-table file and
SHALL display the Last Update value from the stored file when available.

#### Scenario: Stored file exists with Last Update
- **WHEN** the UI loads and a stored drop-table file exists with a Last Update
  value
- **THEN** the status field displays that Last Update value

#### Scenario: Stored file does not exist
- **WHEN** the UI loads and no stored drop-table file exists
- **THEN** the status field displays an empty-state message instead of a Last
  Update value

#### Scenario: Stored file exists without Last Update
- **WHEN** the UI loads and a stored drop-table file exists but Last Update is
  unavailable
- **THEN** the status field indicates that the file exists but Last Update is
  unavailable

### Requirement: Layered Refresh Architecture

The implementation SHALL preserve layer separation by keeping refresh
orchestration contracts in Application, HTTP/file-system details in
Infrastructure, UI interaction in Presentation, and concrete service wiring in
a composition boundary.

#### Scenario: Application layer remains independent
- **WHEN** project references are inspected
- **THEN** the Application project has no project reference to Presentation,
  Infrastructure, or Composition

#### Scenario: Presentation avoids direct infrastructure dependency
- **WHEN** project references are inspected
- **THEN** the Presentation project does not directly reference Infrastructure

### Requirement: Refresh Workflow Validation

The system SHALL include automated tests for URL defaults, successful and
failed refresh orchestration, stored-file metadata extraction, UI-facing status
state, and project dependency direction.

#### Scenario: Validation suite runs
- **WHEN** `dotnet test` is run from the repository root
- **THEN** tests validate the drop-table source refresh workflow and existing
  boilerplate checks without requiring live network access

### Requirement: Refresh Scope Boundary

The drop-table source refresh feature SHALL NOT parse reward tables, create or
modify SQLite schema, persist normalized reward rows, perform item lookup, or
rank mission acquisition paths.

#### Scenario: Downloaded file is available
- **WHEN** a raw drop-table file has been downloaded successfully
- **THEN** the feature stops at raw file storage and status reporting until a
  later approved parser/import change consumes the file

