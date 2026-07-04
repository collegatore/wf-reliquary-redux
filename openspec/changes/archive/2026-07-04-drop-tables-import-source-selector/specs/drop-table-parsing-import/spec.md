## MODIFIED Requirements

### Requirement: Stored Drop Table Source Import

The system SHALL provide an application-layer import workflow that loads a specific, caller-selected Warframe drop-table HTML from local source storage by its `source_files` row ID and imports parsed PI1 data from that stored file.

#### Scenario: Selected source is imported

- **WHEN** a stored drop-table HTML file exists for the provided `sourceId`
- **THEN** the import workflow parses the stored HTML and returns an import result that includes source path, source Last Update when available, import timestamp, and imported row counts

#### Scenario: Selected source ID does not exist

- **WHEN** the provided `sourceId` does not match any row in `source_files`
- **THEN** the import workflow fails without creating parsed data and reports that the requested source file was not found

#### Scenario: Stored source is missing (no rows at all)

- **WHEN** no stored drop-table HTML file exists
- **THEN** the import workflow fails without creating parsed data and reports that the source file must be downloaded first

#### Scenario: Stored source is malformed

- **WHEN** the stored drop-table HTML for the selected source ID cannot be parsed into the required Relics and Missions sections
- **THEN** the import workflow fails without replacing the previous parsed data set

### Requirement: Manual Import Testing Action

The system SHALL provide a presentation-layer import action that invokes the application import workflow with the ID of the currently selected source. The Import button SHALL only be enabled when a source is selected in the source selector ComboBox.

#### Scenario: Import action starts with selected source

- **WHEN** a user activates the import button and a source is selected
- **THEN** the presentation invokes the composed application import workflow with the selected source's ID, without referencing infrastructure directly

#### Scenario: Import action does not download source

- **WHEN** a user activates the import button
- **THEN** the presentation does not invoke source refresh, live network download, or any workflow that replaces the stored drop-table source file

#### Scenario: Import is running

- **WHEN** the manual import is in progress
- **THEN** the presentation indicates that import is running and prevents a duplicate import from being started by the same action

#### Scenario: Import succeeds

- **WHEN** the manual import completes successfully
- **THEN** the presentation displays success status, source Last Update when available, and imported row counts

#### Scenario: Import fails

- **WHEN** the manual import fails because the source is missing, malformed, or persistence fails
- **THEN** the presentation displays the failure message returned by the application import workflow
