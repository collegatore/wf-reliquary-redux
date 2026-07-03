# Relics Sorting Panel

## ADDED Requirements

### Requirement: Sorting button toggles a collapsible sorting panel
The Relics control panel SHALL contain a **Sorting** button (transparent background, borderless, matching the Prime Gear Filters button style). Pressing it SHALL toggle a collapsible sorting panel rendered as its own row directly below the Relics control panel, using the same background tint and corner styling as the control panel. The sorting panel SHALL be closed by default when the application starts. Closing the panel SHALL NOT reset the active sort mode or direction. The sorting panel SHALL be available in all three relic view modes (Verbatim, Compact, Table). The Sorting and Filters panels MAY be open at the same time; each toggles independently and they stack as separate rows.

#### Scenario: Sorting button opens the panel
- **WHEN** the user presses the Sorting button while the sorting panel is closed
- **THEN** the sorting panel appears below the Relics control panel, showing the sort-mode dropdown and the direction button

#### Scenario: Sorting button closes the panel without resetting the sort
- **WHEN** the user selects Wishlist score sorting and then presses the Sorting button to close the panel
- **THEN** the panel hides and the relics remain sorted by Wishlist score

#### Scenario: Panel is closed on startup
- **WHEN** the application starts
- **THEN** the sorting panel is not visible until the Sorting button is pressed

#### Scenario: Panel is available in every view mode
- **WHEN** the user switches between Verbatim, Compact, and Table view modes
- **THEN** the Sorting button and (when open) the sorting panel remain visible and functional

### Requirement: Sorting panel offers three sort modes via a dropdown
The sorting panel SHALL contain a dropdown with exactly three sort modes: **Era**, **Alphabetical**, and **Wishlist score**. The active mode SHALL determine relic-level ordering identically in all view modes (cards and table rows). Reward ordering within a relic SHALL NOT be affected by the sort mode.

- **Era**: relics ordered by era rank (Lith → Meso → Neo → Axi), then by display name.
- **Alphabetical**: relics ordered purely by display name.
- **Wishlist score**: relics ordered by wishlist score, computed as the sum over the relic's wishlisted rewards of a tier multiplier — Common 3, Uncommon 4, Rare 5. Non-wishlisted rewards SHALL NOT contribute to the score regardless of the Show all rewards filter. Score ties SHALL break by era rank, then by display name.

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

### Requirement: Direction button toggles ascending/descending with half-reversed semantics
The sorting panel SHALL contain a direction button that toggles between ascending and descending and displays the current direction. Direction SHALL apply to the primary sort key chain only; relic display names SHALL always sort ascending (A→Z), except in Alphabetical mode where the name is itself the primary key:

- **Era descending**: era rank reversed (Axi → Meso → Neo reversed to Axi → Neo → Meso → Lith), names still A→Z within each era.
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

### Requirement: Selecting a sort mode applies that mode's default direction
When the user selects a sort mode in the dropdown, the direction SHALL reset to that mode's default: ascending for Era and Alphabetical, descending for Wishlist score. The direction button SHALL remain freely toggleable afterwards. Re-selecting the already-active mode SHALL NOT change the direction. This default-direction rule applies only to user-initiated mode changes, not to loading persisted settings at startup.

#### Scenario: Switching to Wishlist score defaults to descending
- **WHEN** the user selects Wishlist score in the dropdown
- **THEN** the direction becomes descending and the highest-scoring relics appear first

#### Scenario: Switching to Era defaults to ascending
- **WHEN** Wishlist score descending is active and the user selects Era in the dropdown
- **THEN** the direction becomes ascending and Lith relics appear first

#### Scenario: Direction stays toggleable after a mode change
- **WHEN** the user selects Wishlist score (direction defaults to descending) and then presses the direction button
- **THEN** the relics re-sort by ascending score

### Requirement: Sort mode and direction persist across sessions
The application SHALL persist the last-used sort mode and direction in user settings and restore them at startup as-is (without applying the mode's default direction). Settings files written before this capability SHALL load with the defaults Era/ascending. The sorting panel's open/closed state SHALL NOT persist.

#### Scenario: Sort combination is restored after restart
- **WHEN** the user selects Wishlist score, toggles the direction to ascending, and restarts the application
- **THEN** the relics are sorted by Wishlist score ascending

#### Scenario: Older settings default to Era ascending
- **WHEN** the application loads a settings file that predates the sort settings
- **THEN** relics sort by Era ascending

### Requirement: Sorting button label reflects the active sort
The Sorting button SHALL display the active sort mode and direction (e.g. "Sorting: Score ↓"), so a non-default sort remains visible while the panel is closed.

#### Scenario: Button label shows a non-default sort with the panel closed
- **WHEN** Wishlist score descending is active and the sorting panel is closed
- **THEN** the Sorting button label indicates the Score mode and descending direction
