# settings-tab Specification

## Requirements

### Requirement: Settings Tab Navigation via File Menu

The application SHALL provide a top-level **File** menu in an OS-level menu bar. The File menu SHALL contain a **Settings** item that navigates the user to the Settings tab. The Settings tab header SHALL be hidden from the tab strip while the Settings tab is inactive, and SHALL be visible in the tab strip while the Settings tab is active.

#### Scenario: File menu is visible

- **WHEN** the application window is open
- **THEN** a menu bar with a "File" menu is shown at the top of the window

#### Scenario: Opening Settings via menu

- **WHEN** the user selects File → Settings
- **THEN** the Settings tab content is displayed and the Settings header becomes visible in the tab strip

#### Scenario: Settings header hidden while inactive

- **WHEN** the Settings tab is not the active tab
- **THEN** the tab strip does not show a Settings header

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

The **Back** button navigates to the tab that was active immediately before the user opened Settings. Clicking a different tab header navigates to that specific tab (not the previously active tab).

#### Scenario: Back with no unsaved changes

- **WHEN** the user clicks Back and no guarded settings have been changed since the last save (or since opening the tab)
- **THEN** the application navigates back to the previously active tab without showing a confirmation dialog

#### Scenario: Back with unsaved changes – user confirms discard

- **WHEN** the user clicks Back and there are unsaved guarded changes, and the user confirms discarding them
- **THEN** the application navigates back to the previously active tab and the unsaved changes are abandoned

#### Scenario: Back with unsaved changes – user cancels

- **WHEN** the user clicks Back and there are unsaved guarded changes, and the user cancels the confirmation
- **THEN** the application remains on the Settings tab and the unsaved changes are preserved

#### Scenario: Changing font size and navigating away triggers no guard

- **WHEN** the user changes the font size and then clicks Back or another tab
- **THEN** no discard-confirmation dialog appears

#### Scenario: Tab click with no unsaved changes

- **WHEN** the user clicks a tab header while on the Settings tab and no guarded settings have been changed
- **THEN** the application navigates to that tab immediately without a confirmation dialog

#### Scenario: Tab click with unsaved changes – user confirms discard

- **WHEN** the user clicks a tab header while on the Settings tab and there are unsaved guarded changes, and the user confirms discarding them
- **THEN** the application navigates to the tab that was clicked and the unsaved changes are abandoned

#### Scenario: Tab click with unsaved changes – user cancels

- **WHEN** the user clicks a tab header while on the Settings tab and there are unsaved guarded changes, and the user cancels the confirmation
- **THEN** the application remains on the Settings tab and the unsaved changes are preserved
