# drop-table-import-clear Specification

## Purpose

TBD — defines the operation and UI for clearing all persisted drop-table import data, supporting a re-import workflow.

## Requirements

### Requirement: Clear Imported Data Operation

The system SHALL provide an application-layer operation that deletes all persisted drop-table import rows — reward items, relics, relic rewards, mission sources, reward rotations, mission rewards, gear vault statuses, and import metadata — in a single atomic transaction. This operation constitutes the clearing stage of a re-import workflow.

#### Scenario: Clear succeeds when import data exists

- **WHEN** the clear operation is invoked and import data rows exist in the database
- **THEN** all import tables are emptied atomically and the operation returns success

#### Scenario: Clear succeeds when no import data exists

- **WHEN** the clear operation is invoked and no import data exists
- **THEN** the operation completes without error (no-op)

#### Scenario: Clear is atomic

- **WHEN** the clear operation encounters a failure mid-way through deletion
- **THEN** no partial deletion is committed and the existing import data remains intact

### Requirement: Clear Imported Data UI Action

The Drop Tables tab Import section SHALL provide a **"Clear imported data"** button. The button SHALL be disabled when no imported data exists and enabled when imported data is present. Activating the button clears all persisted import rows via the application-layer clear operation.

#### Scenario: Button is disabled with no import data

- **WHEN** the Drop Tables tab loads and no import metadata exists
- **THEN** the "Clear imported data" button is disabled

#### Scenario: Button is enabled with existing import data

- **WHEN** the Drop Tables tab loads and import metadata exists
- **THEN** the "Clear imported data" button is enabled

#### Scenario: Button shows confirmation prompt

- **WHEN** the user activates the "Clear imported data" button
- **THEN** a confirmation dialog is displayed before any data is deleted

#### Scenario: User confirms the clear

- **WHEN** the user activates the "Clear imported data" button and confirms in the dialog
- **THEN** all import rows are deleted, the imported version status updates to reflect no import, and the button becomes disabled

#### Scenario: User cancels the clear

- **WHEN** the user activates the "Clear imported data" button but dismisses the confirmation dialog
- **THEN** no import data is deleted and the UI remains unchanged

#### Scenario: Dependent views reload after clear

- **WHEN** the clear operation completes successfully
- **THEN** the Prime Gear, Wishlist Relics, and Wishlist Farming views reload to reflect the now-empty import data
