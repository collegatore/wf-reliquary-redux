# Prime Gear Filter Panel Specification

## Purpose

Defines the Filters menu on the Prime Gear bar: a funnel-icon menu button whose "Hide" entries hide vaulted, owned, or unwishlisted gear, with nothing hidden by default, the choices saved across sessions, and a marked button while anything is hidden.

## Requirements

### Requirement: Filters menu button on the Prime Gear bar
The Prime Gear primary bar SHALL contain an icon-only **Filters** menu button showing a funnel icon, as defined by the `control-bar-visual-style` capability, in place of the former Filters toggle. Pressing it SHALL open a menu below it listing what can be hidden. No filter row SHALL appear below the bar.

#### Scenario: Filters button opens its menu
- **WHEN** the user presses the Filters button on the Prime Gear bar
- **THEN** a menu opens below it, and no extra row appears under the bar

### Requirement: Prime Gear Filters menu hides categories with checked entries
The Prime Gear Filters menu SHALL list, under a non-interactive "Hide" caption, three checkbox entries: **Vaulted** (the Vaulted section, see `prime-gear-parts-view`), **Owned** (see `prime-gear-owned-status`), and **Not wishlisted** (cards with no part in the active wishlist). A checked entry SHALL hide that category; none SHALL be checked by default. Toggling an entry SHALL apply at once and keep the menu open.

#### Scenario: Nothing is hidden by default
- **WHEN** the application starts with no saved filters and the user opens the Prime Gear tab
- **THEN** active, vaulted, owned, and unwishlisted gear cards are all shown, and no Filters entry is checked

#### Scenario: Not wishlisted hides cards with no checked parts
- **WHEN** the user checks Not wishlisted and a gear card has no parts checked
- **THEN** that card is hidden, while cards with at least one checked part stay visible

#### Scenario: Unchecking restores hidden cards
- **WHEN** Not wishlisted is checked and the user unchecks it
- **THEN** cards hidden only because of that entry become visible again

#### Scenario: Hide entries combine
- **WHEN** both Owned and Not wishlisted are checked, and a gear card is owned with at least one part checked
- **THEN** that card is hidden, because it is owned

#### Scenario: Menu stays open while toggling
- **WHEN** the user checks Owned in the Prime Gear Filters menu
- **THEN** owned cards disappear and the menu stays open for further changes

### Requirement: Prime Gear filters persist across sessions
The Prime Gear filter choices SHALL be saved when changed and restored at startup, independently of the Relics filters. A settings file without them SHALL load with nothing hidden.

#### Scenario: Filters are restored after restart
- **WHEN** the user checks Owned and Vaulted in the Prime Gear Filters menu and restarts the application
- **THEN** owned gear and the Vaulted section are hidden and both entries are checked

#### Scenario: Older settings hide nothing
- **WHEN** the application loads a settings file that has no Prime Gear filter values
- **THEN** no Prime Gear filter entry is checked

### Requirement: Prime Gear Filters button marks active filters
The Prime Gear Filters button SHALL show the non-default indicator defined by the `control-bar-visual-style` capability while any of its entries is checked. Its tooltip SHALL name the Filters control and list what is hidden, or state that nothing is hidden.

#### Scenario: Tooltip lists every hidden category
- **WHEN** Owned and Not wishlisted are checked and the user rests the pointer on the Filters button
- **THEN** the funnel icon is tinted with the selection accent and, after a short delay, a tooltip states that owned and unwishlisted gear are hidden
