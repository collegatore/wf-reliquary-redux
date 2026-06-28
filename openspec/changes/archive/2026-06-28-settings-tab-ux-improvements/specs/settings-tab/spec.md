## MODIFIED Requirements

### Requirement: Settings Tab Content

The Settings tab SHALL display controls for: font size selection (Small, Medium, Large, Custom), relics panel height, and the drop-tables source URL. It SHALL display **Save** and **Back** buttons. When the **Custom** font size option is selected, a numeric input (range 8–32) for the custom pixel value SHALL be shown inline. The Settings tab SHALL respond to the **Ctrl+S** keyboard shortcut by triggering the Save action while any element within the Settings tab has keyboard focus.

#### Scenario: Font size control is present

- **WHEN** the Settings tab is open
- **THEN** a font size selector showing Small, Medium, Large, and Custom options is visible

#### Scenario: Custom font size numeric input is shown when Custom is selected

- **WHEN** the user selects the Custom font size option
- **THEN** a numeric input field for the pixel value appears inline

#### Scenario: Custom font size numeric input is hidden otherwise

- **WHEN** a preset font size (Small, Medium, or Large) is selected
- **THEN** the numeric pixel input field is not shown

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

### Requirement: Settings Save Action

The Settings tab SHALL persist the drop-tables source URL, relics panel height, and LFG template atomically when the user activates the **Save** button (or Ctrl+S). Font size is NOT part of this save action; it is saved automatically on selection. The **Save** button SHALL be disabled when there are no unsaved changes to the fields it controls.

#### Scenario: Save persists URL and LFG template

- **WHEN** the user edits the source URL or LFG template and clicks Save
- **THEN** those settings are written to persistent storage and the confirmation state reflects that no unsaved changes remain

#### Scenario: Save disabled when unchanged

- **WHEN** the user opens the Settings tab and has made no changes to URL or LFG template (or has reverted all such changes back to saved values)
- **THEN** the Save button is disabled

#### Scenario: Font size change alone does not enable Save

- **WHEN** the user changes only the font size and makes no other edits
- **THEN** the Save button remains disabled

### Requirement: Settings Navigation with Unsaved-Changes Guard

Both the **Back** button and clicking any other tab header while on the Settings tab SHALL trigger the same unsaved-changes guard for fields managed by the Save button (source URL, LFG template). Font size changes SHALL NOT contribute to the unsaved-changes guard. If the user has made changes to guarded fields, the application SHALL prompt to confirm discarding those changes before navigating away.

#### Scenario: Back with no unsaved changes

- **WHEN** the user clicks Back and no guarded settings have been changed since the last save
- **THEN** the application navigates back without a confirmation dialog

#### Scenario: Back with unsaved changes – user confirms discard

- **WHEN** the user clicks Back with unsaved guarded changes and confirms discarding them
- **THEN** the application navigates back and the unsaved changes are abandoned

#### Scenario: Back with unsaved changes – user cancels

- **WHEN** the user clicks Back with unsaved guarded changes and cancels
- **THEN** the application remains on the Settings tab and the unsaved changes are preserved

#### Scenario: Changing font size and navigating away triggers no guard

- **WHEN** the user changes the font size and then clicks Back or another tab
- **THEN** no discard-confirmation dialog appears

#### Scenario: Tab click with unsaved changes – user confirms discard

- **WHEN** the user clicks a tab header while on the Settings tab with unsaved guarded changes, and confirms
- **THEN** the application navigates to the clicked tab and the unsaved changes are abandoned

#### Scenario: Tab click with unsaved changes – user cancels

- **WHEN** the user clicks a tab header while on the Settings tab with unsaved guarded changes, and cancels
- **THEN** the application remains on the Settings tab and the unsaved changes are preserved
