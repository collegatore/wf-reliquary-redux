## Requirements

### Requirement: Wishlist Relics tab exists in main navigation
The application SHALL display a "Wishlist Relics" tab in the main tab bar alongside the existing "Drop Tables" and "Prime Gear" tabs.

#### Scenario: Tab is visible in main window
- **WHEN** the application is open
- **THEN** a tab labeled "Wishlist Relics" is visible in the tab bar

#### Scenario: Tab displays relic cards when selected
- **WHEN** the user selects the "Wishlist Relics" tab
- **THEN** relic cards are shown for the active wishlist

---

### Requirement: Relic cards show all rewards grouped by tier with wishlisted parts highlighted
Each relic card SHALL have the relic display name as its header, separated from the body by a line. The card body SHALL group rewards by tier in order Common → Uncommon → Rare. Each tier group SHALL begin with a label ("Common:", "Uncommon:", or "Rare:") and end with a line separator. Any reward item whose normalized name matches a wishlisted part SHALL be rendered in bold text.

#### Scenario: Card header shows relic name
- **WHEN** a relic card is displayed
- **THEN** the card header contains the relic's display name (e.g., "Lith A1 Relic") separated from the rewards body by a line

#### Scenario: Card body groups rewards by tier
- **WHEN** a relic card is displayed
- **THEN** rewards are grouped under "Common:", "Uncommon:", and "Rare:" labels in that order, each group followed by a line separator

#### Scenario: Wishlisted reward is bold
- **WHEN** a reward item in a relic card matches a part in the active wishlist
- **THEN** that reward item is rendered in bold text

#### Scenario: Non-wishlisted reward is normal weight
- **WHEN** a reward item in a relic card does not match any part in the active wishlist
- **THEN** that reward item is rendered in normal (non-bold) text

---

### Requirement: Only relics containing at least one wishlisted part are shown
The view SHALL only display relics that contain at least one reward item matching a part in the active wishlist.

#### Scenario: Relic with no wishlisted rewards is excluded
- **WHEN** a relic has no reward items that match any part in the active wishlist
- **THEN** that relic is not shown in the Wishlist Relics tab

#### Scenario: Empty wishlist shows placeholder
- **WHEN** the active wishlist contains no items
- **THEN** a placeholder message is displayed instead of relic cards

---

### Requirement: Cards are ordered by era then alphabetically by name
Relic cards SHALL be presented in era order (Lith → Meso → Neo → Axi) and within each era in ascending alphabetical order by relic display name. There are no era section headers; cards flow in a single flat wrap layout.

#### Scenario: Lith relics appear before Meso relics
- **WHEN** both Lith and Meso relics are shown
- **THEN** all Lith relic cards appear before any Meso relic cards

#### Scenario: Cards within an era are alphabetical
- **WHEN** multiple relics from the same era are shown
- **THEN** they appear in ascending alphabetical order by display name

---

### Requirement: Vaulted relics can be shown or hidden via a checkbox
Above the relic cards there SHALL be a control panel containing a checkbox labeled "Show Vaulted Relics". By default the checkbox is unchecked (vaulted relics hidden). Checking it reveals vaulted relics.

A relic is considered vaulted if it does not appear as a reward in any active mission.

#### Scenario: Vaulted relics hidden by default
- **WHEN** the Wishlist Relics tab is first displayed
- **THEN** relics that are vaulted are not shown

#### Scenario: Checking show-vaulted reveals vaulted relics
- **WHEN** the user checks "Show Vaulted Relics"
- **THEN** vaulted relics that contain wishlisted parts become visible

#### Scenario: Unchecking show-vaulted hides vaulted relics again
- **WHEN** the user unchecks "Show Vaulted Relics" after they were shown
- **THEN** vaulted relics are hidden again

---

### Requirement: Vaulted relics are visually marked the same as vaulted prime gear
Vaulted relic cards SHALL use the same reduced-opacity visual treatment applied to vaulted prime gear cards.

#### Scenario: Vaulted relic card has reduced opacity
- **WHEN** a vaulted relic is visible (show-vaulted is checked)
- **THEN** its card is rendered at reduced opacity (matching the vaulted gear appearance)

#### Scenario: Non-vaulted relic card has full opacity
- **WHEN** a relic is not vaulted
- **THEN** its card is rendered at full opacity

---

### Requirement: View refreshes when active wishlist changes
The Wishlist Relics view SHALL reload its relic cards whenever the active wishlist is changed.

#### Scenario: Changing wishlist updates displayed relics
- **WHEN** the user changes the active wishlist (from another tab)
- **THEN** the Wishlist Relics view refreshes to show relics for the newly active wishlist
