## ADDED Requirements

### Requirement: Relic Card Width Control

The application SHALL provide a user control, located in Settings under the Appearance group alongside the font-size and Relics/Missions layout controls, to set the base width (in pixels) of relic cards — the effective card width when font size is Medium (14pt). The control SHALL be a numeric input with an up/down spinner positioned on the right, accepting values between 180 and 600, inclusive. Values entered outside this range SHALL be clamped to the nearest bound.

#### Scenario: Relic card width control is present

- **WHEN** the Settings tab is open
- **THEN** a "Relic card width" numeric input is visible under Appearance, alongside the font-size and Relics/Missions layout controls

#### Scenario: Spinner is positioned on the right

- **WHEN** the Settings tab is open
- **THEN** the relic card width control's up/down spinner is positioned to the right of the numeric value

#### Scenario: Spinner up steps the value up

- **WHEN** the user activates the spinner's up control on the relic card width control
- **THEN** the value increases by the control's step amount, clamped to 600 if it would exceed the maximum

#### Scenario: Spinner down steps the value down

- **WHEN** the user activates the spinner's down control on the relic card width control
- **THEN** the value decreases by the control's step amount, clamped to 180 if it would go below the minimum

#### Scenario: Out-of-range value is clamped

- **WHEN** the user enters a value below 180 or above 600 in the relic card width input
- **THEN** the effective base value is clamped to 180 or 600 respectively

### Requirement: Relic Card Width Applied Immediately

The selected relic card base width SHALL apply to all relic cards immediately when the user changes the numeric input in the Settings tab, scaled by the current font-size ratio per the Font-Size-Proportional Scaling requirement. The change SHALL be saved automatically without requiring the user to press the Save button, and SHALL NOT mark the Settings tab as having unsaved changes.

#### Scenario: Relic card width applied immediately on input change

- **WHEN** the user changes the numeric pixel value in the relic card width control
- **THEN** all relic cards immediately reflect the new effective width

#### Scenario: Relic card width auto-saved without Save button

- **WHEN** the user changes the relic card width value
- **THEN** the new base width is written to persistent storage immediately
- **AND** the Settings Save button state is unaffected by the change

#### Scenario: Relic card width not included in unsaved-changes guard

- **WHEN** the user changes the relic card width and then navigates away from Settings without pressing Save
- **THEN** no discard-confirmation dialog appears on account of the relic card width change
- **AND** the selected relic card base width remains active after navigation

### Requirement: Relic Card Width Persistence and Default

The relic card base width SHALL be persisted in user settings and restored on the next launch. When no relic card width preference is saved, the default base width SHALL be 300 pixels, matching the application's historical fixed card width.

#### Scenario: Default relic card width on first launch

- **WHEN** the application starts and no relic card width preference is saved
- **THEN** relic cards are rendered at 300 pixels wide at Medium font size

#### Scenario: Relic card width restored from settings

- **WHEN** the application starts and a relic card width preference is saved
- **THEN** relic cards are rendered at the saved base width, scaled by the current font-size ratio

### Requirement: Font-Size-Proportional Scaling

The configured relic card width SHALL be treated as the base width at Medium font size (14pt). The effective, rendered relic card width SHALL continue to scale proportionally with font size — `effective width = base width * (current font size / Medium font size)` — exactly as relic card width already scales today from its fixed `300` literal. Changing font size SHALL change the effective relic card width; changing the relic card width setting SHALL change the base that font size scales from.

#### Scenario: Changing font size still resizes relic cards

- **WHEN** the user changes the font size in Settings, with any configured relic card base width
- **THEN** the effective width of relic cards changes proportionally, consistent with the font-size ratio

#### Scenario: Changing relic card width preserves current font-size scaling

- **WHEN** the user changes the relic card width base while a non-Medium font size is active
- **THEN** relic cards immediately reflect the new base scaled by the currently active font-size ratio, not the unscaled base value
