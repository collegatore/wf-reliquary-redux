# appearance-window Specification

## Purpose

Provides a dedicated, modeless Appearance window (opened via File → Appearance) that hosts
the font size, relic card width, and Relics/Missions layout orientation controls previously
found in the Settings tab, so the user can preview appearance changes against the main window
without navigating away from whatever tab they are currently viewing.

## Requirements

### Requirement: File Menu Appearance Item

The application SHALL provide an **Appearance** item in the **File** menu, alongside the existing Settings and Drop Tables items. Selecting it SHALL open the Appearance window.

#### Scenario: File menu shows Appearance item

- **WHEN** the user opens the File menu
- **THEN** an "Appearance" item is shown alongside "Settings" and "Drop Tables"

#### Scenario: Opening Appearance via menu

- **WHEN** the user selects File → Appearance
- **THEN** the Appearance window opens (or is activated if already open)

### Requirement: Appearance Window Content

The Appearance window SHALL display the font size selector (Small, Medium, Large, Custom with numeric input), the relic card width control, the Relics/Missions layout orientation control, and the **relic view mode selector** (Verbatim / Compact / Table). The controls carried over from the Settings tab remain unchanged in function.

#### Scenario: Appearance controls are present

- **WHEN** the Appearance window is open
- **THEN** the font size selector, relic card width control, layout orientation control, and relic view mode selector are all visible

### Requirement: Modeless Window Behavior

The Appearance window SHALL be modeless: it SHALL NOT block interaction with the main window. The user SHALL be able to interact with either window while the other is open.

#### Scenario: Main window remains interactive

- **WHEN** the Appearance window is open
- **THEN** the user can still click, scroll, and switch tabs in the main window without closing or dismissing the Appearance window

### Requirement: Single Instance

Only one Appearance window SHALL exist at a time. Selecting File → Appearance while the window is already open SHALL bring the existing window to the foreground rather than opening a second one.

#### Scenario: Second invocation activates the existing window

- **WHEN** the Appearance window is already open and the user selects File → Appearance again
- **THEN** no new window is created and the existing Appearance window is activated (brought to front)

### Requirement: Live Preview via Main Window

Changes made in the Appearance window SHALL apply immediately to the main window, regardless of which tab or state the main window is currently showing. No separate preview surface is rendered inside the Appearance window itself.

#### Scenario: Main window reflects a change while showing an unrelated tab

- **WHEN** the main window is showing any tab (e.g. Prime Gear) and the user changes a control in the Appearance window
- **THEN** the main window's content immediately reflects the change (e.g. text size, or relic card sizing/orientation when the Relics and Missions tab is shown), without the user needing to switch tabs

### Requirement: Fixed-Size, Non-Resizable Window

The Appearance window SHALL have a fixed size and SHALL NOT be resizable by the user. Its fixed size SHALL be wide enough to display the font size row (preset radio buttons and the numeric point-size input) without clipping or wrapping.

#### Scenario: User cannot resize the Appearance window

- **WHEN** the user attempts to drag an edge or corner of the Appearance window
- **THEN** the window does not resize

#### Scenario: Font size row fits at the fixed window size

- **WHEN** the Appearance window is open
- **THEN** the font size row's radio buttons and numeric input are fully visible without clipping or wrapping

### Requirement: Position Offset from Main Window

Each time the Appearance window opens, it SHALL be positioned 50 pixels to the left and 50 pixels down from the main window's current position. This position is computed fresh from the main window's position at open time; it is not saved or restored across opens.

#### Scenario: Appearance window opens offset from the main window

- **WHEN** the user opens the Appearance window via File → Appearance
- **THEN** the Appearance window's position is 50 pixels left of and 50 pixels below the main window's current position

#### Scenario: Position follows the main window's current location, not a remembered one

- **WHEN** the main window has been moved since the Appearance window was last open, and the user opens the Appearance window again
- **THEN** the Appearance window's position is computed from the main window's new (current) position, not from where the Appearance window was previously

### Requirement: Fixed Font Size, Independent of the Font Size Setting

The Appearance window's own controls (labels, radio buttons, numeric inputs) SHALL always render at a fixed small font size, regardless of the font size the user selects via the window's own font size control. The Appearance window's content is not itself subject to the app-wide font size it configures.

#### Scenario: Changing font size to Large does not change the Appearance window's own text

- **WHEN** the user selects Large in the Appearance window's font size control
- **THEN** the Appearance window's own labels and controls remain at the fixed small size, while the main window's text grows to reflect the Large selection

#### Scenario: Changing font size to Small does not change the Appearance window's own text

- **WHEN** the user selects Small in the Appearance window's font size control
- **THEN** the Appearance window's own labels and controls remain at the same fixed small size as before (no visible change to the Appearance window itself)

### Requirement: Default Window Close Behavior

The Appearance window SHALL use the platform's default window lifecycle. Closing the Appearance window SHALL NOT close the main window or exit the application. No custom shutdown handling is introduced for this window.

#### Scenario: Closing Appearance window leaves the app running

- **WHEN** the user closes the Appearance window
- **THEN** the main window remains open and the application continues running

#### Scenario: Closing main window while Appearance window is open

- **WHEN** the main window is closed while the Appearance window is open
- **THEN** the application follows its existing default shutdown behavior
