# font-size-setting Specification

## Requirements

### Requirement: Three-Step Font Size Selector

The system SHALL support three font size levels — **Small**, **Medium**, and **Large** — where Medium is the default. The selected level SHALL be persisted in user settings and restored on the next launch.

#### Scenario: Default font size on first launch

- **WHEN** the application starts and no font size preference is saved
- **THEN** the active font size is Medium

#### Scenario: Font size restored from settings

- **WHEN** the application starts and a font size preference is saved
- **THEN** the active font size matches the saved preference

### Requirement: App-Wide Font Size Application

The selected font size SHALL apply to all text in the application window immediately when the user selects a radio button in the Settings tab (live preview). If the user discards the change, the font reverts. The selection is persisted when the user clicks Save.

#### Scenario: Font size previewed immediately on selection

- **WHEN** the user selects a different font size radio button in the Settings tab
- **THEN** all text in the application immediately reflects the selected size

#### Scenario: Font size reverted when discarding

- **WHEN** the user changes the font size in Settings and discards the change without saving
- **THEN** all text reverts to the font size that was active when Settings was opened

#### Scenario: Font size persisted after Save

- **WHEN** the user selects a font size and clicks Save
- **THEN** the selection is written to persistent storage and survives application restart

#### Scenario: Small font is smaller than Medium

- **WHEN** the Small font size is active
- **THEN** body text is rendered at a point size strictly smaller than the Medium default

#### Scenario: Large font is larger than Medium

- **WHEN** the Large font size is active
- **THEN** body text is rendered at a point size strictly larger than the Medium default
