# relics-filter-panel Specification

## Purpose

Defines the Filters menu on the Relics bar: a funnel-icon menu button whose "Hide" entries hide categories of relics (Vaulted), with nothing hidden by default, the choice saved across sessions, and a marked button while anything is hidden.

## Requirements

### Requirement: Filters menu button on the Relics bar
The Relics control bar SHALL contain an icon-only **Filters** menu button showing a funnel icon, as defined by the `control-bar-visual-style` capability. Pressing it SHALL open a menu below it listing what can be hidden. No filter row SHALL appear below the bar.

#### Scenario: Filters button opens its menu
- **WHEN** the user presses the Filters button on the Relics bar
- **THEN** a menu opens below it, and no extra row appears under the bar

---

### Requirement: Filters menu hides categories with checked entries
The Relics Filters menu SHALL list, under a non-interactive "Hide" caption, a **Vaulted** checkbox entry. A checked entry SHALL hide that category of relics; nothing SHALL be hidden while no entry is checked. Toggling an entry SHALL apply immediately and SHALL NOT close the menu. Every entry SHALL be unchecked by default.

#### Scenario: Nothing is hidden by default
- **WHEN** the application starts with no saved filters
- **THEN** the Vaulted entry is unchecked and vaulted relics are shown

#### Scenario: Checking Vaulted hides vaulted relics
- **WHEN** the user opens the Filters menu and checks Vaulted
- **THEN** vaulted relics disappear from every view mode and the menu stays open

---

### Requirement: Relics filters persist across sessions
The Relics filter choices SHALL be saved when changed and restored at startup, independently of the Prime Gear filters. A settings file without them SHALL load with nothing hidden.

#### Scenario: Hidden vaulted relics stay hidden after restart
- **WHEN** the user checks Vaulted in the Relics Filters menu and restarts the application
- **THEN** vaulted relics are hidden and the Vaulted entry is checked

#### Scenario: Older settings hide nothing
- **WHEN** the application loads a settings file that has no Relics filter values
- **THEN** vaulted relics are shown

---

### Requirement: Filters button marks active filters
The Relics Filters button SHALL show the non-default indicator defined by the `control-bar-visual-style` capability while any of its entries is checked. Its tooltip SHALL name the Filters control and list what is hidden, or state that nothing is hidden.

#### Scenario: Hiding vaulted marks the button
- **WHEN** Vaulted is checked in the Relics Filters menu
- **THEN** the Filters button's funnel icon is tinted with the selection accent

#### Scenario: Tooltip lists what is hidden
- **WHEN** Vaulted is checked and the user rests the pointer on the Filters button
- **THEN** after a short delay a tooltip names the Filters control and states that vaulted relics are hidden
