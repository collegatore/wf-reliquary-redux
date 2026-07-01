## MODIFIED Requirements

### Requirement: Settings Tab Content

The Settings tab SHALL display controls for: font size selection (Small, Medium, Large, Custom), relics panel height, relic card width, and the drop-tables source URL. It SHALL display **Save** and **Back** buttons. A numeric input (range 8–32) for the pixel value SHALL always be visible inline next to the font size radio buttons, showing the effective size for any selection, with its up/down spinner positioned on the right. The Settings tab SHALL respond to the **Ctrl+S** keyboard shortcut by triggering the Save action while any element within the Settings tab has keyboard focus.

#### Scenario: Font size control is present

- **WHEN** the Settings tab is open
- **THEN** a font size selector showing Small, Medium, Large, and Custom options is visible

#### Scenario: Font size numeric input is always visible

- **WHEN** the Settings tab is open
- **THEN** a numeric input showing the effective font size in points is visible alongside the font size radio buttons regardless of which option is selected

#### Scenario: Font size spinner is positioned on the right

- **WHEN** the Settings tab is open
- **THEN** the font size numeric input's up/down spinner is positioned to the right of the value

#### Scenario: Relics panel height control is present

- **WHEN** the Settings tab is open
- **THEN** the saved Relics panel height in pixels is displayed under the label "Relics panel height" along with a reset-to-default action

#### Scenario: Drop-tables URL field is present

- **WHEN** the Settings tab is open
- **THEN** the drop-tables source URL text field is visible and editable

#### Scenario: Ctrl+S triggers save while Settings tab is focused

- **WHEN** the user presses Ctrl+S while any element inside the Settings tab has keyboard focus
- **THEN** the Save action is triggered (equivalent to clicking the Save button)

#### Scenario: Ctrl+S does not fire outside the Settings tab

- **WHEN** the Settings tab is not the active tab (or no Settings element holds keyboard focus)
- **THEN** pressing Ctrl+S does not trigger the Settings Save action
