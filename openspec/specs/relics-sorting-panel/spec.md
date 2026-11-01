# relics-sorting-panel Specification

## Purpose

Defines the sort controls on the Relics bar: an icon-only sort menu (Era, Alphabetical, Wishlist score) and a direction button, the sort semantics, default-direction rules on mode change, the Era descending default, persistence across sessions, and the marked button for a non-default sort.

## Requirements

### Requirement: Sorting panel offers three sort modes via a dropdown
The sort menu on the Relics bar SHALL offer exactly three sort modes: **Era**, **Alphabetical**, and **Wishlist score**. The active mode SHALL determine relic-level ordering identically in all view modes (cards and table rows). Reward ordering within a relic SHALL NOT be affected by the sort mode.

- **Era**: relics ordered by era rank (Lith → Meso → Neo → Axi), then by display name.
- **Alphabetical**: relics ordered purely by display name.
- **Wishlist score**: relics ordered by wishlist score, computed as the sum over the relic's wishlisted rewards of a tier multiplier — Common 3, Uncommon 4, Rare 5. Non-wishlisted rewards SHALL NOT contribute to the score regardless of the Show all rewards option. Score ties SHALL break by era rank, then by display name.

#### Scenario: Era mode matches the previous fixed order
- **WHEN** Era mode is active in ascending direction
- **THEN** relics appear in era order Lith → Meso → Neo → Axi, alphabetically by name within each era

#### Scenario: Alphabetical mode sorts by name only
- **WHEN** Alphabetical mode is active in ascending direction
- **THEN** relics appear in ascending display-name order regardless of era

#### Scenario: Wishlist score sums tier multipliers of wishlisted rewards
- **WHEN** a relic has two wishlisted Common rewards and one wishlisted Rare reward
- **THEN** its wishlist score is 11 (3 + 3 + 5)

#### Scenario: Non-wishlisted rewards do not affect the score
- **WHEN** Show all rewards is enabled and a relic displays non-wishlisted rewards
- **THEN** the relic's wishlist score counts only its wishlisted rewards

#### Scenario: All view modes follow the active sort
- **WHEN** the user activates Wishlist score sorting and switches from Compact to Table view mode
- **THEN** the table rows appear in the same relic order as the cards did

---

### Requirement: Direction button toggles ascending/descending with half-reversed semantics
The direction button on the Relics bar SHALL toggle between ascending and descending, and its icon SHALL show the current direction. Direction SHALL apply to the primary sort key chain only; relic display names SHALL always sort ascending (A→Z), except in Alphabetical mode where the name is itself the primary key:

- **Era descending**: era rank reversed (Axi → Neo → Meso → Lith), names still A→Z within each era.
- **Alphabetical descending**: names Z→A.
- **Wishlist score descending**: score high→low; ties break by reversed era rank (Axi first), names still A→Z.

#### Scenario: Era descending shows Axi first with names ascending
- **WHEN** Era mode is active and the user toggles the direction to descending
- **THEN** Axi relics appear first and Lith relics last, with names in ascending order within each era

#### Scenario: Alphabetical descending reverses the name order
- **WHEN** Alphabetical mode is active and the user toggles the direction to descending
- **THEN** relics appear in descending display-name order

#### Scenario: Score descending breaks ties by reversed era then ascending name
- **WHEN** Wishlist score mode is active in descending direction and two relics tie on score
- **THEN** the relic from the later era (Axi before Lith) appears first, and same-era ties order by ascending name

---

### Requirement: Sort controls sit on the Relics bar
The Relics control bar SHALL contain the sort controls directly, with no sorting row below it: an icon-only **sort button** showing a fixed double-arrow icon, followed closely by an icon-only **direction button**. Neither SHALL show the mode's name; their tooltips state it. Only the direction button's icon SHALL reflect the direction.

#### Scenario: Sort controls are on the bar
- **WHEN** the Relics and Missions tab is displayed
- **THEN** the Relics bar shows the sort button with a double-arrow icon and no mode name, followed by the direction button, and no sorting row appears under the bar

#### Scenario: Only the direction icon changes with the direction
- **WHEN** the user toggles the direction
- **THEN** the direction button's icon flips while the sort button's double-arrow icon stays the same

---

### Requirement: Sort menu lists the modes
Pressing the sort button SHALL open a menu listing the sort modes as radio entries under a "Sort by" caption, with the active mode marked. Choosing a mode SHALL apply it and close the menu.

#### Scenario: Sort menu marks the active mode
- **WHEN** Wishlist score is active and the user presses the sort button
- **THEN** a menu opens listing Era, Alphabetical, and Wishlist score, with Wishlist score marked

#### Scenario: Choosing a mode closes the menu
- **WHEN** the user opens the sort menu and chooses Alphabetical
- **THEN** the relics re-sort alphabetically and the menu closes

---

### Requirement: Newly selected sort mode starts in its default direction
When the user selects a sort mode in the sort menu, the direction SHALL reset to that mode's default: **descending** for Era and Wishlist score, **ascending** for Alphabetical. The direction button SHALL remain freely toggleable afterwards. Re-selecting the already-active mode SHALL NOT change the direction. This rule applies only to user-initiated mode changes, not to loading saved settings at startup.

#### Scenario: Switching to Era defaults to descending
- **WHEN** Alphabetical ascending is active and the user selects Era
- **THEN** the direction becomes descending and Axi relics appear first

#### Scenario: Switching to Alphabetical defaults to ascending
- **WHEN** Era descending is active and the user selects Alphabetical
- **THEN** the direction becomes ascending and relics appear A to Z

#### Scenario: Switching to Wishlist score defaults to descending
- **WHEN** the user selects Wishlist score
- **THEN** the direction becomes descending and the highest-scoring relics appear first

#### Scenario: Direction stays toggleable after a mode change
- **WHEN** the user selects Wishlist score and then presses the direction button
- **THEN** the relics re-sort by ascending score

---

### Requirement: Sort choice persists across sessions
The application SHALL save the sort mode and direction when the user changes them and restore them at startup as saved, without applying the mode's default direction. The default sort SHALL be **Era descending**: a settings file with no sort mode loads as Era, and one with no direction loads as descending. A saved direction SHALL be kept, so an install that saved Era ascending keeps it.

#### Scenario: Sort combination is restored after restart
- **WHEN** the user selects Wishlist score, toggles the direction to ascending, and restarts the application
- **THEN** the relics are sorted by Wishlist score ascending

#### Scenario: Fresh settings sort by Era descending
- **WHEN** the application starts with no saved sort settings
- **THEN** relics are sorted Axi first and Lith last, by name within each era

#### Scenario: Saved Era ascending is kept
- **WHEN** the application loads settings that saved Era ascending
- **THEN** relics are sorted Lith first, and the sort control shows the non-default indicator

---

### Requirement: Sort control marks a non-default sort
The sort button SHALL show the non-default indicator defined by the `control-bar-visual-style` capability on its double-arrow icon whenever the sort is anything other than Era descending. Its tooltip, and the direction button's tooltip, SHALL state the active mode and direction (e.g. "Sorted by Wishlist score, descending").

#### Scenario: Wishlist score marks the sort control
- **WHEN** Wishlist score descending is active
- **THEN** the sort button's double-arrow icon is tinted with the selection accent

#### Scenario: Default sort is unmarked
- **WHEN** Era descending is active
- **THEN** the sort icon is drawn in the normal text color

#### Scenario: Tooltip states the sort
- **WHEN** Alphabetical ascending is active and the user rests the pointer on the direction button
- **THEN** after a short delay a tooltip reads that relics are sorted alphabetically, ascending
