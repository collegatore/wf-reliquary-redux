# font-size-setting Specification

## Requirements

### Requirement: Three-Step Font Size Selector

The system SHALL support four font size levels — **Small**, **Medium**, **Large**, and **Custom** — where Medium is the default. The selected level SHALL be persisted in user settings and restored on the next launch. When **Custom** is selected, the user SHALL be able to enter a numeric pixel value (between 8 and 32) that defines the font size; this value SHALL also be persisted and restored.

#### Scenario: Default font size on first launch

- **WHEN** the application starts and no font size preference is saved
- **THEN** the active font size is Medium

#### Scenario: Font size restored from settings

- **WHEN** the application starts and a font size preference is saved
- **THEN** the active font size matches the saved preference

#### Scenario: Custom font size restored from settings

- **WHEN** the application starts and the saved font size is a value that does not match a preset
- **THEN** the active font size is the stored pixel value

#### Scenario: Small font is smaller than Medium

- **WHEN** the Small font size is active
- **THEN** body text is rendered at a point size strictly smaller than the Medium default

#### Scenario: Large font is larger than Medium

- **WHEN** the Large font size is active
- **THEN** body text is rendered at a point size strictly larger than the Medium default

### Requirement: App-Wide Font Size Application

The selected font size SHALL apply to all text in the application immediately when the user selects a radio button (or changes the custom numeric input) in the Settings tab. The change SHALL be saved automatically without requiring the user to press the Save button, and SHALL NOT mark the Settings tab as having unsaved changes. Because the save is immediate, there is no revert-on-discard behavior for font size.

#### Scenario: Font size applied immediately on selection

- **WHEN** the user selects a different font size radio button in the Settings tab
- **THEN** all text in the application immediately reflects the selected size

#### Scenario: Custom font size applied immediately on input change

- **WHEN** the user changes the numeric pixel value in the Settings tab
- **THEN** all text in the application immediately reflects the entered size

#### Scenario: Font size auto-saved without Save button

- **WHEN** the user selects a font size option (including changing the custom pixel value)
- **THEN** the new font size is written to persistent storage immediately
- **AND** the Settings Save button state is unaffected by the font size change

#### Scenario: Font size not included in unsaved-changes guard

- **WHEN** the user selects a different font size and then navigates away from Settings without pressing Save
- **THEN** no discard-confirmation dialog appears on account of the font size change
- **AND** the selected font size remains active after navigation
