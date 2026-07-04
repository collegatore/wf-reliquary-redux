# drop-table-local-file-import Specification

## Purpose

Provides the ability for a user to supply a local drop-table HTML file as a source, bypassing the URL download path. The file is read, validated, and registered in the `source_files` DB table alongside URL-downloaded sources.

## Requirements

### Requirement: Local File Source Registration

The system SHALL provide an application-layer operation that accepts a local HTML file path, extracts the Last Update value from its content, and checks whether a source with the same Last Update version already exists in `source_files`. If a duplicate version is found, the operation SHALL return a `DuplicateVersion` result carrying the pending `DropTableSourceContent` and the existing row's `Id` without inserting a record. A `DuplicateVersion` result is distinguishable from both success and failure: it SHALL carry no error message, and consumers SHALL treat it as a pending confirmation rather than reporting it as an error. If no duplicate is found, the operation SHALL insert the content and provenance into `source_files`. If no Last Update value can be extracted, the operation SHALL fail without inserting a record.

#### Scenario: Local file is registered successfully with new version

- **WHEN** a valid drop-table HTML file with a parseable Last Update value is submitted and no existing `source_files` row has the same Last Update value
- **THEN** a `source_files` row is inserted with the full HTML content, a file URI as source URL, the current timestamp as `downloaded_at`, and the extracted Last Update value

#### Scenario: Local file produces a duplicate version

- **WHEN** a valid drop-table HTML file is submitted and its extracted Last Update value matches an existing `source_files` row
- **THEN** the operation returns a `DuplicateVersion` result carrying the pending content and the existing row's ID without inserting a new row

#### Scenario: Local file lacks Last Update

- **WHEN** the submitted HTML file does not contain a parseable Last Update value
- **THEN** the operation fails and no `source_files` row is inserted

#### Scenario: Local file cannot be read

- **WHEN** the submitted file path does not exist or cannot be opened
- **THEN** the operation fails and no `source_files` row is inserted

### Requirement: Confirmed File Overwrite

The system SHALL provide an application-layer `ConfirmSaveAsync` operation on `IDropTableLocalFileSourceWorkflow` that accepts a pending `DropTableSourceContent` and an `existingId`, atomically replaces the existing `source_files` row via `IDropTableSourceStore.ReplaceAsync`, and returns a success result. This operation is only called after the user has confirmed the overwrite in the UI.

#### Scenario: Overwrite confirmed by user

- **WHEN** `ConfirmSaveAsync` is called with a valid pending content and an existing row ID
- **THEN** the existing `source_files` row is replaced atomically and the operation returns a success result with the registered Last Update value

#### Scenario: Overwrite with invalid ID

- **WHEN** `ConfirmSaveAsync` is called with an ID that no longer exists in `source_files`
- **THEN** the operation returns a failure result and no row is inserted

### Requirement: Import from File UI Action

The Drop Tables tab SHALL provide an **"Read from file"** button that opens a file picker for the user to select a local HTML file, then registers it in `source_files`. This button only acquires and stores the source; it does not trigger parsing or import.

#### Scenario: User selects a file and registration succeeds

- **WHEN** the user activates "Read from file", selects a valid drop-table HTML file, and the registration succeeds
- **THEN** the UI updates the downloaded version status from the newly stored `source_files` record

#### Scenario: User cancels the file picker

- **WHEN** the user activates "Read from file" but dismisses the file picker without selecting a file
- **THEN** no operation is invoked and the UI remains unchanged

#### Scenario: Registration fails

- **WHEN** the user selects a file but the registration fails (unreadable file or missing Last Update)
- **THEN** the UI displays the failure message and the existing `source_files` records are unchanged
