## MODIFIED Requirements

### Requirement: Drop Table Source URL UI Configuration
The Avalonia UI SHALL show the configured drop-table source URL and SHALL allow the user to edit and save that URL as application-level user settings. The Save URL control SHALL be disabled when the current field value is identical to the already-saved URL.

#### Scenario: User views configured URL
- **WHEN** the UI loads
- **THEN** the source URL field displays the saved application setting or the default source URL when no setting exists

#### Scenario: User saves configured URL
- **WHEN** the user enters a valid absolute URL and saves it
- **THEN** the application persists the URL for future refresh operations

#### Scenario: Save URL is disabled when value is unchanged
- **WHEN** the source URL field contains the same value as the currently saved URL
- **THEN** the Save URL control is disabled

#### Scenario: Save URL is enabled when value differs
- **WHEN** the user edits the source URL field to a value that differs from the saved URL
- **THEN** the Save URL control is enabled

---

### Requirement: Drop Table Status Field
The Avalonia UI SHALL show a status field for the stored drop-table file and SHALL display the downloaded file's version (the source page Last Update value) labelled **"Downloaded file version"** when available.

#### Scenario: Stored file exists with Last Update
- **WHEN** the UI loads and a stored drop-table file exists with a Last Update value
- **THEN** the status field displays that Last Update value under the label "Downloaded file version"

#### Scenario: Stored file does not exist
- **WHEN** the UI loads and no stored drop-table file exists
- **THEN** the status field displays an empty-state message instead of a Last Update value

#### Scenario: Stored file exists without Last Update
- **WHEN** the UI loads and a stored drop-table file exists but Last Update is unavailable
- **THEN** the status field indicates that the file exists but Last Update is unavailable

---

### Requirement: Drop Tables tab layout
The Drop Tables tab SHALL present its content in three clearly labelled sections separated by horizontal dividers: **Drop-table source URL**, **File download**, and **File import**. The app title heading SHALL NOT appear within the tab content.

#### Scenario: Tab shows three sections
- **WHEN** the user opens the Drop Tables tab
- **THEN** three sections are visible: "Drop-table source URL", "File download", and "File import", each preceded by its label and a separator line

#### Scenario: App title is not shown in tab content
- **WHEN** the user opens the Drop Tables tab
- **THEN** no heading showing the application name appears within the tab content area
