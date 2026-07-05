# drop-table-source-refresh Specification

## Purpose
TBD - created by archiving change add-drop-table-download. Update Purpose after archive.
## Requirements
### Requirement: Configured Drop Table Source URL

The system SHALL use a user-editable application-level Warframe drop-table
source URL and SHALL default to `https://www.warframe.com/droptables`
when no user setting is saved. The download operation SHALL follow HTTP
redirects, since the canonical address redirects to DE's current CDN location
for the drop-table HTML.

#### Scenario: Default source URL is used
- **WHEN** the application starts without a configured source URL override
- **THEN** the drop-table refresh workflow uses `https://www.warframe.com/droptables`
  as the source URL

#### Scenario: Default source redirect is followed
- **WHEN** a download is requested against the default source URL and the server
  responds with an HTTP redirect to the current drop-table asset location
- **THEN** the download follows the redirect and stores the drop-table HTML it
  resolves to

#### Scenario: Source URL is edited by user
- **WHEN** the user saves a different absolute source URL in the application UI
- **THEN** the drop-table refresh workflow uses the configured URL for the next
  download operation

#### Scenario: Invalid source URL is entered
- **WHEN** the user attempts to save a source URL that is not absolute
- **THEN** the application rejects the value and keeps the previously configured
  URL

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

### Requirement: Confirmed Download Overwrite

The system SHALL provide an application-layer `ConfirmSaveAsync` operation on `IDropTableSourceRefreshWorkflow` that accepts a pending `DropTableSourceContent` and an `existingId`, atomically replaces the existing `source_files` row via `IDropTableSourceStore.ReplaceAsync`, and returns a success result. This operation is only called after the user has confirmed the overwrite in the UI.

#### Scenario: Overwrite confirmed by user

- **WHEN** `ConfirmSaveAsync` is called with a valid pending content and an existing row ID
- **THEN** the existing `source_files` row is replaced atomically and the operation returns a success result with the current source status

#### Scenario: Overwrite with invalid ID

- **WHEN** `ConfirmSaveAsync` is called with an ID that no longer exists in `source_files`
- **THEN** the operation returns a failure result and no row is inserted

### Requirement: DB-Backed Source Content Storage

The system SHALL store each successfully downloaded raw drop-table HTML in the `source_files` database table alongside its provenance. No HTML file or JSON sidecar SHALL be written to the local filesystem as part of the download operation.

#### Scenario: Content is stored in DB
- **WHEN** a download succeeds with a parseable Last Update value
- **THEN** a `source_files` row is inserted containing the full HTML content, source URL, download timestamp, and Last Update value; no file is written to disk

#### Scenario: No source has been downloaded
- **WHEN** no download has been performed and the `source_files` table has no rows
- **THEN** the source status reports that no drop-table source is available

### Requirement: Stored File Last Update Status

The system SHALL expose the source page's Last Update value from the most recent `source_files` database record when a downloaded source exists.

#### Scenario: Last Update provenance exists
- **WHEN** at least one row exists in `source_files`
- **THEN** the source status includes the Last Update value from the most recent row for display

#### Scenario: No source_files record exists
- **WHEN** the `source_files` table has no rows
- **THEN** the source status reports that no drop-table source is available

### Requirement: Drop Table Download UI Action

The Drop Tables tab SHALL provide a **"Download from URL"** button that downloads the raw drop-table HTML from the configured source URL and stores it in `source_files`. This button only acquires and stores the source; it does not trigger parsing or import.

#### Scenario: User starts download from URL
- **WHEN** the user activates the "Download from URL" button
- **THEN** the UI invokes the application-layer drop-table download operation and prevents duplicate concurrent requests

#### Scenario: Download from URL completes
- **WHEN** a UI-started download completes successfully
- **THEN** the UI updates the downloaded version status from the newly stored `source_files` record

### Requirement: Drop Table Source URL UI Configuration

The Avalonia UI SHALL show the configured drop-table source URL and SHALL allow the user to edit and save that URL as part of the unified application-level user settings via the Settings tab. The Settings tab's Save button persists the URL along with all other settings. There is no separate Save URL button on the Drop Tables tab.

#### Scenario: User views configured URL

- **WHEN** the user opens the Settings tab
- **THEN** the source URL field displays the saved application setting or the default source URL when no setting exists

#### Scenario: User saves configured URL

- **WHEN** the user enters a valid absolute URL in the Settings tab and clicks Save
- **THEN** the application persists the URL for future refresh operations

#### Scenario: Invalid URL is rejected on save

- **WHEN** the user enters a value that is not an absolute URL in the Settings tab and clicks Save
- **THEN** the application rejects the value, keeps the previously configured URL, and notifies the user of the validation error

### Requirement: Drop Table Status Field

The Avalonia UI SHALL show a status field for the stored drop-table file and
SHALL display the downloaded file's version (the source page Last Update value) labelled **"Downloaded file version"** when available.

#### Scenario: Stored file exists with Last Update
- **WHEN** the UI loads and a stored drop-table file exists with a Last Update
  value
- **THEN** the status field displays that Last Update value under the label "Downloaded file version"

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

