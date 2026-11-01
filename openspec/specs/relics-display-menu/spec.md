# relics-display-menu Specification

## Purpose

Defines the Display menu on the Relics bar: an eye-icon menu button that holds how relics are displayed (the relic view mode and the Show all rewards option), as opposed to which relics are shown (Filters) or in what order (Sort).

## Requirements

### Requirement: Display menu button on the Relics bar
The Relics control bar SHALL contain an icon-only **Display** menu button showing an eye icon, as defined by the `control-bar-visual-style` capability. Pressing it SHALL open a menu below it. The button SHALL be available in every view mode. Choosing an entry SHALL apply immediately.

#### Scenario: Display button opens its menu
- **WHEN** the user presses the Display button on the Relics bar
- **THEN** a menu opens below it showing the view modes and the Show all rewards option

### Requirement: Display menu offers the relic view mode
The Display menu SHALL list the three relic view modes, **Verbatim**, **Compact** and **Table**, as radio entries under a "View" caption, with the active mode marked. Choosing a mode SHALL switch the relics panel to it immediately (as specified by the `relic-view-modes` capability) and close the menu.

#### Scenario: Active mode is marked
- **WHEN** Compact mode is active and the user opens the Display menu
- **THEN** Compact is marked as the chosen view mode and Verbatim and Table are not

#### Scenario: Choosing a mode switches the panel
- **WHEN** the user opens the Display menu and chooses Table
- **THEN** the relics panel switches to Table mode and the menu closes

### Requirement: Display menu offers Show all rewards
The Display menu SHALL contain a **Show all rewards** checkbox entry, below the view modes and separated from them. It SHALL be unchecked by default. Toggling it SHALL update the relic cards immediately and SHALL NOT close the menu. While **Table** mode is active, the entry SHALL be shown disabled, keeping its checked state, so switching back to a card mode restores it.

#### Scenario: Toggling Show all rewards keeps the menu open
- **WHEN** the user checks Show all rewards in the Display menu
- **THEN** all relic cards show their full reward lists and the menu stays open

#### Scenario: Show all rewards is disabled in Table mode
- **WHEN** Table mode is active and the user opens the Display menu
- **THEN** the Show all rewards entry is greyed out and does not respond to clicks

#### Scenario: Show all rewards state survives Table mode
- **WHEN** Show all rewards is checked, the user switches to Table mode and then back to Compact mode
- **THEN** Show all rewards is still checked and the cards show all rewards

### Requirement: Show all rewards persists across sessions
The Show all rewards choice SHALL be saved when changed and restored at startup. A settings file without it SHALL load with Show all rewards unchecked.

#### Scenario: Show all rewards is restored after restart
- **WHEN** the user checks Show all rewards and restarts the application
- **THEN** the relic cards show all rewards and the Display menu shows it checked

#### Scenario: Older settings leave Show all rewards off
- **WHEN** the application loads a settings file that has no Show all rewards value
- **THEN** Show all rewards is unchecked

### Requirement: Display button marks a non-default display
The Display button SHALL show the non-default indicator defined by the `control-bar-visual-style` capability whenever the view mode is not Compact or Show all rewards is checked. Its tooltip SHALL name the active view mode and whether all rewards are shown.

#### Scenario: Table mode marks the Display button
- **WHEN** the active view mode is Table
- **THEN** the Display button's eye icon is tinted with the selection accent

#### Scenario: Default display is unmarked
- **WHEN** the view mode is Compact and Show all rewards is unchecked
- **THEN** the Display button's icon is drawn in the normal text color

#### Scenario: Tooltip describes the display
- **WHEN** the view mode is Verbatim with Show all rewards checked and the user rests the pointer on the Display button
- **THEN** after a short delay a tooltip names the Display control and states Verbatim view with all rewards shown
