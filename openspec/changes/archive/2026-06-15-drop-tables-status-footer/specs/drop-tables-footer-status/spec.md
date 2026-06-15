## ADDED Requirements

### Requirement: Drop Tables status region in footer

The global footer SHALL contain a Drop Tables status region, separated from the active profile display by a vertical line divider. The status region is always present in the DOM but its visible content varies by state (see requirements below).

#### Scenario: Footer contains separator and status region

- **WHEN** the main window is displayed
- **THEN** the footer shows the profile name, a vertical separator, and the Drop Tables status region to the right

### Requirement: Operation status in footer during in-progress operations

While a Drop Tables operation is in progress (`IsBusy`), the footer status region SHALL display a spinning progress indicator alongside the current operation status text. The spinner and text SHALL disappear when the operation completes.

#### Scenario: Spinner and text shown during download

- **WHEN** the "Download from URL" operation is in progress
- **THEN** the footer shows a spinner and "Downloading source from URL…"

#### Scenario: Spinner and text shown during import

- **WHEN** the "Import stored source" operation is in progress
- **THEN** the footer shows a spinner and the current operation status text

#### Scenario: Spinner and text shown during clear

- **WHEN** the "Clear imported data" operation is in progress
- **THEN** the footer shows a spinner and "Clearing imported data…"

#### Scenario: Spinner visible from any tab during operation

- **WHEN** a Drop Tables operation is in progress and the user is on the Prime Gear tab
- **THEN** the footer spinner and operation text are visible without switching to the Drop Tables tab

#### Scenario: Footer status is empty when idle and Drop Tables tab is active

- **WHEN** no operation is in progress and the Drop Tables tab is the active tab
- **THEN** the footer status region shows no text (the tab body already shows import status)

### Requirement: Condensed import status in footer when Drop Tables tab is hidden

When no operation is in progress and the Drop Tables tab is not the active tab, the footer status region SHALL display a condensed import status:

- "Source data not imported" when no drop-table import data exists.
- "Drop Tables version: `<last update>`" when drop-table import data exists.

#### Scenario: Condensed "not imported" shown on Prime Gear tab with no import

- **WHEN** no import data exists, no operation is in progress, and the Prime Gear tab is active
- **THEN** the footer status region displays "Source data not imported"

#### Scenario: Condensed version shown on Relics tab after import

- **WHEN** import data exists, no operation is in progress, and the Relics and Missions tab is active
- **THEN** the footer status region displays "Drop Tables version: `<last update>`"

#### Scenario: Condensed status updates after import completes

- **WHEN** an import completes and the user is on a non-Drop-Tables tab
- **THEN** the footer status region updates to "Drop Tables version: `<last update>`" immediately

#### Scenario: Condensed status updates after clear completes

- **WHEN** a clear-import completes and the user is on a non-Drop-Tables tab
- **THEN** the footer status region updates to "Source data not imported" immediately

#### Scenario: Condensed status disappears when Drop Tables tab becomes active

- **WHEN** the user navigates to the Drop Tables tab while idle
- **THEN** the footer status region becomes empty (import status is now visible in the tab body)

### Requirement: Import statistics shown in ImportStatusText after successful import

After a successful import, `ImportStatusText` SHALL include the import statistics (relic count, relic reward count, mission source count, mission reward count). `OperationStatusText` after a successful import SHALL contain only a brief completion line (version string, no counts), so it is safe to display in the footer without overflow.

#### Scenario: ImportStatusText shows counts after import

- **WHEN** an import completes successfully
- **THEN** the Import section in the Drop Tables tab shows "Source data imported. Drop Tables version: X. Relics: N; relic rewards: N; mission sources: N; mission rewards: N."

#### Scenario: OperationStatusText after import is concise

- **WHEN** an import completes successfully
- **THEN** the footer operation status shows only "Import completed. Drop Tables version: X." (no counts)

### Requirement: OperationStatusText removed from Drop Tables tab body

The standalone `OperationStatusText` `TextBlock` at the bottom of the Drop Tables tab body SHALL be removed. Operation feedback SHALL be communicated exclusively through the footer status region.

#### Scenario: Drop Tables tab body has no operation-status TextBlock

- **WHEN** the Drop Tables tab is displayed
- **THEN** no standalone operation-status text block is rendered inside the tab body

### Requirement: StatusText property renamed to SourceStatusText

The `StatusText` property on `MainWindowViewModel` and its corresponding AXAML binding SHALL be renamed to `SourceStatusText`. No change to displayed wording or behavior.

#### Scenario: AXAML binds SourceStatusText in Source acquisition section

- **WHEN** the Drop Tables tab is displayed
- **THEN** the Source acquisition section status TextBlock is bound to `SourceStatusText` (not `StatusText`)
