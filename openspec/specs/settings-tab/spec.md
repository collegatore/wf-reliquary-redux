# settings-tab Specification

## Requirements

### Requirement: Settings Tab Navigation via File Menu

The application SHALL provide a top-level **File** menu in an OS-level menu bar. The File menu SHALL contain a **Settings** item that navigates the user to the Settings tab. The Settings tab header SHALL NOT be visible in the tab strip.

#### Scenario: File menu is visible

- **WHEN** the application window is open
- **THEN** a menu bar with a "File" menu is shown at the top of the window

#### Scenario: Opening Settings via menu

- **WHEN** the user selects File → Settings
- **THEN** the Settings tab content is displayed and the tab strip does not show a Settings header

### Requirement: Settings Tab Content

The Settings tab SHALL display controls for: font size selection, relics panel height, and the drop-tables source URL. It SHALL also display **Save** and **Back** buttons.

#### Scenario: Font size control is present

- **WHEN** the Settings tab is open
- **THEN** a font size selector showing Small, Medium, and Large options is visible

#### Scenario: Relics panel height control is present

- **WHEN** the Settings tab is open
- **THEN** the saved Relics panel height in pixels is displayed under the label "Relics panel height" along with a reset-to-default action

#### Scenario: Drop-tables URL field is present

- **WHEN** the Settings tab is open
- **THEN** the drop-tables source URL text field is visible and editable

### Requirement: Settings Keyboard Shortcut

The Settings tab SHALL be reachable via the keyboard shortcut **Ctrl+,**. The shortcut SHALL be active at all times while the main window has focus, not only when the File menu is open.

> **Implementation note:** `MenuItem.InputGesture` in Avalonia only renders the hint label inside the menu. The actual global shortcut must be registered via `Window.KeyBindings`.

#### Scenario: Keyboard shortcut opens Settings

- **WHEN** the user presses Ctrl+, while the main window has focus
- **THEN** the Settings tab is displayed

### Requirement: Font Size Live Preview

Selecting a font size option in the Settings tab SHALL apply the change to the entire application immediately, before the user clicks Save. If the user discards their changes (via Back or by navigating to another tab), the font reverts to the last saved value.

#### Scenario: Font size previewed on selection

- **WHEN** the user selects a different font size radio button in the Settings tab
- **THEN** all application text immediately renders at the newly selected size

#### Scenario: Font size reverted on discard

- **WHEN** the user changes the font size and then discards changes (via Back confirm or tab-click confirm)
- **THEN** all application text reverts to the font size that was in effect before the Settings tab was opened

### Requirement: Settings Save Action

The Settings tab SHALL persist all edited settings (font size, Relics panel height, source URL) atomically when the user activates the **Save** button. The **Save** button SHALL be disabled when there are no unsaved changes.

#### Scenario: Save persists all settings

- **WHEN** the user edits one or more settings and clicks Save
- **THEN** all settings are written to persistent storage and the confirmation state reflects that no unsaved changes remain

#### Scenario: Save disabled when unchanged

- **WHEN** the user opens the Settings tab and has made no changes (or has reverted all changes back to saved values)
- **THEN** the Save button is disabled and cannot be activated

### Requirement: Settings Navigation with Unsaved-Changes Guard

Both the **Back** button and clicking any other tab header while on the Settings tab SHALL trigger the same unsaved-changes guard. If the user has made changes that have not been saved, the application SHALL prompt to confirm discarding those changes before navigating away.

The **Back** button navigates to the tab that was active immediately before the user opened Settings. Clicking a different tab header navigates to that specific tab (not the previously active tab).

#### Scenario: Back with no unsaved changes

- **WHEN** the user clicks Back and no settings have been changed since the last save (or since opening the tab)
- **THEN** the application navigates back to the previously active tab without showing a confirmation dialog

#### Scenario: Back with unsaved changes – user confirms discard

- **WHEN** the user clicks Back and there are unsaved changes, and the user confirms discarding them
- **THEN** the application navigates back to the previously active tab and the unsaved changes are abandoned

#### Scenario: Back with unsaved changes – user cancels

- **WHEN** the user clicks Back and there are unsaved changes, and the user cancels the confirmation
- **THEN** the application remains on the Settings tab and the unsaved changes are preserved

#### Scenario: Tab click with no unsaved changes

- **WHEN** the user clicks a tab header while on the Settings tab and no settings have been changed
- **THEN** the application navigates to that tab immediately without a confirmation dialog

#### Scenario: Tab click with unsaved changes – user confirms discard

- **WHEN** the user clicks a tab header while on the Settings tab and there are unsaved changes, and the user confirms discarding them
- **THEN** the application navigates to the tab that was clicked and the unsaved changes are abandoned

#### Scenario: Tab click with unsaved changes – user cancels

- **WHEN** the user clicks a tab header while on the Settings tab and there are unsaved changes, and the user cancels the confirmation
- **THEN** the application remains on the Settings tab and the unsaved changes are preserved
