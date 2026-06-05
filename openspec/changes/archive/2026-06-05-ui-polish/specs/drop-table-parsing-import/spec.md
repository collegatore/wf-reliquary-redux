## ADDED Requirements

### Requirement: Imported file version status display
The Drop Tables tab SHALL display an **"Imported file version"** status line in the File import section that reflects the last successfully imported drop-table version. When no import has been completed, the line SHALL read "No drop-table file imported yet". When an import exists, it SHALL show the source Last Update value from import metadata.

#### Scenario: No import has been performed
- **WHEN** the UI loads and no import metadata exists in the database
- **THEN** the File import section displays "No drop-table file imported yet"

#### Scenario: An import exists with a Last Update value
- **WHEN** the UI loads and import metadata exists with a source_last_update value
- **THEN** the File import section displays "Imported file version: <value>"

#### Scenario: Import status refreshes after successful import
- **WHEN** a drop-table import completes successfully
- **THEN** the imported file version status line updates to reflect the newly imported version without requiring an app restart
