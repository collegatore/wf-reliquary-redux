## MODIFIED Requirements

### Requirement: App-Wide Font Size Application

The selected font size SHALL apply to all text in the application immediately when the user selects a radio button (or changes the custom numeric input) in the Appearance window. The change SHALL be saved automatically without requiring a Save button, consistent with the Appearance window's live-apply behavior.

#### Scenario: Font size applied immediately on selection

- **WHEN** the user selects a different font size radio button in the Appearance window
- **THEN** all text in the application immediately reflects the selected size

#### Scenario: Custom font size applied immediately on input change

- **WHEN** the user changes the numeric pixel value in the Appearance window
- **THEN** all text in the application immediately reflects the entered size

#### Scenario: Font size auto-saved without a Save button

- **WHEN** the user selects a font size option (including changing the custom pixel value) in the Appearance window
- **THEN** the new font size is written to persistent storage immediately, with no Save action required
