# drop-table-source-export Specification

## Purpose

Allows a user to export all stored drop-table source files from the `source_files` DB table to the local filesystem, so that previously downloaded HTML content can be recovered or archived outside the application.

## Requirements

### Requirement: Export All Stored Source Files

The system SHALL provide an application-layer operation that writes every row in `source_files` to the filesystem as individual HTML files in a caller-supplied destination folder. Each exported file SHALL be named `warframe-drop-tables-{source_last_update}.html`. If a row has no `source_last_update` value, the file SHALL be named using the row's `downloaded_at` timestamp instead.

#### Scenario: Export succeeds

- **WHEN** the export operation is invoked with a valid destination folder and `source_files` contains one or more rows
- **THEN** one HTML file per row is written to the destination folder, each named by its Last Update value (or downloaded-at timestamp as fallback), and the operation reports the number of files written

#### Scenario: No source files to export

- **WHEN** the export operation is invoked and `source_files` has no rows
- **THEN** the operation completes without writing any files and reports that nothing was available to export

#### Scenario: Export destination is not writable

- **WHEN** the export operation is invoked with a destination folder that cannot be written to
- **THEN** the operation reports a failure without partially writing files

### Requirement: Export UI Action on Drop Tables Tab

The Drop Tables tab SHALL include an Export button that opens a folder picker, then invokes the export operation for the user-selected folder. The button SHALL be disabled while no rows exist in `source_files`. The UI SHALL display the result (files written, nothing to export, or failure message) after the operation completes.

#### Scenario: User exports stored sources

- **WHEN** the user clicks Export, selects a folder, and the export operation succeeds
- **THEN** the UI reports how many files were written to the selected folder

#### Scenario: User cancels the folder picker

- **WHEN** the user clicks Export but dismisses the folder picker without selecting a folder
- **THEN** no export operation is invoked and the UI remains unchanged

#### Scenario: Export button is disabled with no stored sources

- **WHEN** the Drop Tables tab is shown and `source_files` has no rows
- **THEN** the Export button is disabled

#### Scenario: Export fails

- **WHEN** the export operation returns a failure
- **THEN** the UI displays the failure message
