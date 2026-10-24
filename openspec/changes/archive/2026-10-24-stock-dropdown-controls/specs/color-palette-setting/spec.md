# Spec Delta

## MODIFIED Requirements

### Requirement: Palette covers the control bars, dropdowns, card menus, and the selection color
The active palette SHALL determine the colors of the control bars and their controls (including the surfaces sharing the bar tint: the Prime Gear "Vaulted" header and the footer), of every dropdown, of every menu opened from a `⋮` button (the cards' menus and the selector actions menus), and of the app's selection color: the checked-chip tint and tick, the selected dropdown item, and the wash on gear cards selected for group operations. Colors outside this list SHALL NOT change with the palette.

#### Scenario: Switching palette recolors the bars and the selection wash
- **WHEN** the user switches from Linen to Stone while gear cards are selected in group operations
- **THEN** the control bars take the Stone colors and the selected cards' wash becomes heather

#### Scenario: Relic tier colors are unaffected
- **WHEN** the user switches palette
- **THEN** relic card tier bars, the owned-gear outline, and the wishlist highlight keep their colors

### Requirement: Dropdowns follow the palette
Every dropdown in the application SHALL follow the active palette: all ComboBoxes, including the Wishlist and Profile selectors. A closed dropdown SHALL use the palette's field background and outline, with the hover tint while the pointer is over it. In an open list, the item under the pointer SHALL use the hover tint, and the selected item SHALL use the selection color.

#### Scenario: Open sort list marks hover and selection
- **WHEN** the user opens the relic sort-mode dropdown with Era selected and points at Alphabetical
- **THEN** Era shows the palette's selection color and Alphabetical shows the hover tint

#### Scenario: Pointed-at entry in the selector popup shows the hover tint
- **WHEN** the user opens the Wishlist selector and points at an inactive wishlist
- **THEN** that entry shows the palette's hover tint

#### Scenario: Dropdowns outside the control bars follow the palette
- **WHEN** the Stone palette is active and the user opens the Drop Tables source selector
- **THEN** its field, hovered item, and selected item use the Stone colors

### Requirement: Selector popups mark the active entry by color alone
In the Wishlist and Profile selectors, the active entry SHALL be marked only by the palette's selection color, as any ComboBox marks its selected item. No check glyph SHALL be shown.

#### Scenario: Active wishlist is marked like the selected sort mode
- **WHEN** the user opens the Wishlist selector
- **THEN** the active wishlist's entry shows the palette's selection color, as the selected item does in the sort-mode dropdown, and no ✓ appears next to it

#### Scenario: Active profile is marked the same way
- **WHEN** the user opens the Profile selector on the Settings tab
- **THEN** the active profile's entry shows the selection color and no ✓

### Requirement: Profile selector shares the Wishlist selector's field look
The Profile selector on the Settings tab SHALL have the same palette-driven field look (background, outline, hover tint) as the Wishlist selector on the Prime Gear tab, and its actions button SHALL look like the Wishlist actions button.

#### Scenario: Profile selector looks like the Wishlist selector
- **WHEN** the Settings tab is shown
- **THEN** the Profile selector and its actions button have the same field background and outline as the Wishlist selector and its actions button on the Prime Gear tab

## ADDED Requirements

### Requirement: Menus follow the palette
Every menu opened from a `⋮` button (on relic cards in Verbatim and Compact modes, on gear cards, and next to the Wishlist and Profile selectors) SHALL follow the active palette: the item under the pointer SHALL use the hover tint, a pressed item the pressed tint, and an item reached with the keyboard the hover tint.

#### Scenario: Pointed-at card menu item shows the hover tint
- **WHEN** the user opens a gear card's `⋮` menu and points at "Mark as owned"
- **THEN** the item shows the palette's hover tint

#### Scenario: Pointed-at actions menu item shows the hover tint
- **WHEN** the Stone palette is active and the user opens the Wishlist actions menu and points at "New list"
- **THEN** the item shows the Stone hover tint

### Requirement: Menus open with no item highlighted
Opening a menu from a `⋮` button with the pointer SHALL NOT highlight any item. An item SHALL be highlighted only while the pointer is over it, or after the user moves to it with the keyboard.

#### Scenario: Menu opens plain
- **WHEN** the user clicks a relic card's `⋮` button
- **THEN** the menu opens and "Compose Recruiting Chat message…" is shown without the hover tint until the pointer moves over it

#### Scenario: Actions menu opens plain
- **WHEN** the user clicks the Wishlist actions button
- **THEN** the menu opens with no item highlighted

#### Scenario: Keyboard navigation still highlights
- **WHEN** a menu opened from a `⋮` button is open and the user presses the Down arrow key
- **THEN** the item moved to is highlighted with the hover tint

## REMOVED Requirements

### Requirement: Card menus follow the palette
**Reason**: Widened to cover the selector actions menus as well as card menus.
**Migration**: See "Menus follow the palette" in this capability.

### Requirement: Card menus open with no item highlighted
**Reason**: Widened to cover the selector actions menus as well as card menus.
**Migration**: See "Menus open with no item highlighted" in this capability.
