## MODIFIED Requirements

### Requirement: On-Demand Drop Table Download

The system SHALL provide an application-layer operation that downloads the raw
drop-table HTML from the configured source URL when requested by the user.

#### Scenario: Download succeeds

- **WHEN** the user requests a drop-table download and the configured URL
  returns successful HTML content with a parseable Last Update value
- **THEN** the system stores the downloaded HTML content and provenance in the `source_files` DB table

#### Scenario: Downloaded file lacks Last Update

- **WHEN** the user requests a drop-table download and the downloaded file does
  not contain a parseable Last Update value
- **THEN** the system reports a failed operation and does not insert a row into `source_files`

#### Scenario: Download fails

- **WHEN** the user requests a drop-table download and the configured URL cannot
  be retrieved successfully
- **THEN** the system reports a failed operation without modifying any existing `source_files` records

### Requirement: Drop Table Download UI Action

The Drop Tables tab SHALL provide a **"Download from URL"** button that downloads the raw drop-table HTML from the configured source URL and stores it in `source_files`. This button only acquires and stores the source; it does not trigger parsing or import.

#### Scenario: User starts download from URL

- **WHEN** the user activates the "Download from URL" button
- **THEN** the UI invokes the application-layer drop-table download operation and prevents duplicate concurrent requests

#### Scenario: Download from URL completes

- **WHEN** a UI-started download completes successfully
- **THEN** the UI updates the downloaded version status from the newly stored `source_files` record

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
