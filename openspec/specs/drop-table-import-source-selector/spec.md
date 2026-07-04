# drop-table-import-source-selector Specification

## Purpose

Provides the source selection UI and view model state that lets users choose which stored drop-table source to import, browse all stored sources in a modal, and confirm overwrites when acquiring a duplicate version.

## Requirements

### Requirement: Source Summary List Query

The system SHALL expose a `ListSummariesAsync` method on `IDropTableSourceStore` that returns a read-only list of `DropTableSourceSummary` records for all rows in `source_files`, ordered newest-first (descending `id`). When no rows exist, the method SHALL return an empty list. Each `DropTableSourceSummary` SHALL carry the row `Id`, source URL string, `DownloadedAt` timestamp, and `LastUpdate` version string. The method SHALL NOT load the `content` column.

#### Scenario: Multiple sources are listed newest-first

- **WHEN** `ListSummariesAsync` is called and `source_files` contains multiple rows
- **THEN** the returned list contains one entry per row ordered by descending `id`, with each entry carrying the correct `Id`, source URL, `DownloadedAt`, and `LastUpdate` values and no HTML content

#### Scenario: No sources are available

- **WHEN** `ListSummariesAsync` is called and `source_files` has no rows
- **THEN** the returned list is empty

### Requirement: Source Selection View Model State

The Drop Tables view model SHALL expose an `AvailableSources` collection of `DropTableSourceSummary` items and a `SelectedSource` property of type `DropTableSourceSummary?`. `AvailableSources` SHALL be populated via `ListSummariesAsync`. `SelectedSource` SHALL default to `null` when `AvailableSources` is empty.

#### Scenario: Source list is loaded on initialisation

- **WHEN** the Drop Tables view model initialises (via `LoadStatusAsync`)
- **THEN** `AvailableSources` is populated with all stored source summaries ordered newest-first

#### Scenario: Newest source is pre-selected after acquisition

- **WHEN** a download or read-from-file operation completes successfully (including a confirmed overwrite)
- **THEN** `AvailableSources` is refreshed and `SelectedSource` is set to the first (newest) item in the updated list

#### Scenario: No sources available on first launch

- **WHEN** the view model initialises and `source_files` is empty
- **THEN** `AvailableSources` is empty and `SelectedSource` is `null`

### Requirement: Import Gated on Source Selection

The Drop Tables view model's `ImportCommand` SHALL only be executable when `SelectedSource` is non-null and no operation is in progress.

#### Scenario: Import command disabled when no source selected

- **WHEN** `SelectedSource` is `null`
- **THEN** `ImportCommand.CanExecute` returns `false`

#### Scenario: Import command enabled when source selected

- **WHEN** `SelectedSource` is non-null and the view model is not busy
- **THEN** `ImportCommand.CanExecute` returns `true`

#### Scenario: Import command re-evaluated when selection changes

- **WHEN** `SelectedSource` changes
- **THEN** `ImportCommand.NotifyCanExecuteChanged` is called

### Requirement: Source Selector UI Control

The Drop Tables tab Import section SHALL display a `ComboBox` bound to `AvailableSources` (ItemsSource) and `SelectedSource` (SelectedItem, two-way). Each item SHALL display only the `LastUpdate` version string. When `LastUpdate` is empty, the item label SHALL read `"(version unknown)"` (a defensive fallback: acquisition workflows reject sources without a parseable version, so empty values can only originate from older database states). A pictogram button SHALL appear adjacent to the ComboBox and be bound to an `OpenSourceBrowserCommand` that opens the source browser modal. Both the ComboBox and the pictogram button SHALL be disabled when `AvailableSources` is empty.

#### Scenario: ComboBox shows version-only labels

- **WHEN** the Drop Tables tab is open and `AvailableSources` has items
- **THEN** each ComboBox item displays only the `LastUpdate` string (no download timestamp)

#### Scenario: Unknown version label is shown

- **WHEN** a source summary has an empty `LastUpdate`
- **THEN** the ComboBox item displays `"(version unknown)"`

#### Scenario: ComboBox and pictogram are disabled when no sources

- **WHEN** `AvailableSources` is empty
- **THEN** both the ComboBox and the pictogram button are disabled

#### Scenario: Selecting a source enables the Import button

- **WHEN** the user selects an item in the ComboBox
- **THEN** `SelectedSource` is updated and `ImportCommand.CanExecute` becomes `true`

### Requirement: Source Browser Modal Window

The system SHALL provide a modal window (`SourceBrowserWindow`) opened by the pictogram button that displays a DataGrid listing all stored sources. The DataGrid SHALL have three columns: **Version** (the `LastUpdate` value), **Source** (the download URL for HTTP-sourced records, or the filename for file-URI records), and **Downloaded** (the `DownloadedAt` value in local short date/time format). The window is read-only. It receives its data from the view model's current `AvailableSources` list.

#### Scenario: Source browser opens with all sources

- **WHEN** the user activates the pictogram button
- **THEN** the source browser modal opens and displays all items from `AvailableSources` in the DataGrid

#### Scenario: Source browser shows correct source label

- **WHEN** a source summary was acquired via URL download
- **THEN** the Source column shows the download URL

#### Scenario: File-sourced record shows filename

- **WHEN** a source summary was acquired via local file read (file URI)
- **THEN** the Source column shows the filename extracted from the URI rather than the full file path

### Requirement: Overwrite Confirmation Dialog

The system SHALL present a `ConfirmSourceOverwriteDialog` modal when an acquisition operation detects that the incoming version already exists in `source_files`. The dialog SHALL display the version label and download timestamp of the existing record and require the user to confirm or cancel the replacement. A duplicate-version detection SHALL NOT be reported through the error reporter; the confirmation dialog is the only feedback shown. If the confirmed replacement itself fails (for example the existing row no longer exists), that failure SHALL be reported as an error.

#### Scenario: Overwrite dialog shown on duplicate detection

- **WHEN** a download or file-read produces a `LastUpdate` value that matches an existing `source_files` row
- **THEN** a confirmation dialog is displayed showing the existing record's version and download timestamp before any write occurs

#### Scenario: User confirms overwrite

- **WHEN** the user confirms the overwrite dialog
- **THEN** the existing `source_files` row is replaced with the new content, `AvailableSources` is refreshed, and the replaced version is pre-selected

#### Scenario: User cancels overwrite

- **WHEN** the user cancels the overwrite dialog
- **THEN** no write occurs, the existing `source_files` row is unchanged, and `AvailableSources` is not refreshed

#### Scenario: Duplicate detection is not reported as an error

- **WHEN** an acquisition operation returns a `DuplicateVersion` result
- **THEN** no error is reported; the view model routes the result to the overwrite confirmation flow

#### Scenario: Confirmed overwrite fails because the row is gone

- **WHEN** the user confirms the overwrite but the existing row was removed in the meantime and `ConfirmSaveAsync` returns a failure
- **THEN** the failure message is reported as an error and no row is inserted
