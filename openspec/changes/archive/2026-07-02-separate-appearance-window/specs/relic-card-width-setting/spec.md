## MODIFIED Requirements

### Requirement: Relic Card Width Control

The application SHALL provide a user control, located in the Appearance window (opened via File → Appearance) alongside the font-size and Relics/Missions layout controls, to set the base width (in pixels) of relic cards — the effective card width when font size is Medium (14pt). The control SHALL be a numeric input with an up/down spinner positioned on the right, accepting values between 180 and 600, inclusive. Values entered outside this range SHALL be clamped to the nearest bound.

#### Scenario: Relic card width control is present

- **WHEN** the Appearance window is open
- **THEN** a "Relic card width" numeric input is visible, alongside the font-size and Relics/Missions layout controls

#### Scenario: Spinner is positioned on the right

- **WHEN** the Appearance window is open
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

The selected relic card base width SHALL apply to all relic cards immediately when the user changes the numeric input in the Appearance window, scaled by the current font-size ratio per the Font-Size-Proportional Scaling requirement. The change SHALL be saved automatically without requiring a Save button.

#### Scenario: Relic card width applied immediately on input change

- **WHEN** the user changes the numeric pixel value in the relic card width control
- **THEN** all relic cards immediately reflect the new effective width

#### Scenario: Relic card width auto-saved without a Save button

- **WHEN** the user changes the relic card width value in the Appearance window
- **THEN** the new base width is written to persistent storage immediately, with no Save action required
