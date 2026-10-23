# color-palette-setting Specification

## Purpose

Lets the user choose between coordinated color palettes for the tab control bars and the app's selection color, applied live and remembered across sessions.

## Requirements

### Requirement: Two selectable palettes with Linen as the default
The application SHALL offer exactly two color palettes: **Linen** and **Stone**. Linen SHALL be the default. Linen SHALL use warm linen bars, white controls, a fern-green hover tint, and an ink-blue selection color. Stone SHALL use light stone bars, off-white controls, a sage hover tint, and a heather (dusty mauve) selection color.

#### Scenario: Linen is active on first launch
- **WHEN** the application starts with no saved palette
- **THEN** the control bars use the Linen palette

#### Scenario: Stone uses its own selection color
- **WHEN** the Stone palette is active and the user checks a filter chip
- **THEN** the checked chip is tinted heather rather than ink blue

### Requirement: Palette covers the control bars, dropdowns, card menus, and the selection color
The active palette SHALL determine the colors of the control bars and their controls (including the surfaces sharing the bar tint: the Prime Gear "Vaulted" header and the footer), of every dropdown, of the cards' `⋮` menus, and of the app's selection color: the checked-chip tint and tick, the selected or active dropdown item, and the wash on gear cards selected for group operations. Colors outside this list SHALL NOT change with the palette.

#### Scenario: Switching palette recolors the bars and the selection wash
- **WHEN** the user switches from Linen to Stone while gear cards are selected in group operations
- **THEN** the control bars take the Stone colors and the selected cards' wash becomes heather

#### Scenario: Relic tier colors are unaffected
- **WHEN** the user switches palette
- **THEN** relic card tier bars, the owned-gear outline, and the wishlist highlight keep their colors

### Requirement: Hover tint stays distinct from selection and bar colors
In every palette, the hover tint SHALL be a green distinct from the palette's selection color and from its bar surface, so an option under the pointer reads as available rather than selected.

#### Scenario: Hovered chip is green, checked chip is not
- **WHEN** the pointer is over an unchecked chip next to a checked chip
- **THEN** the hovered chip shows the palette's green hover tint and the checked chip shows the palette's selection tint

### Requirement: Chip tick follows the palette; other checkboxes keep the standard accent
The checkbox tick inside a checked chip SHALL use the palette's selection accent. Checkboxes that are not chips (for example the part checkboxes on gear cards and checkboxes in Settings) SHALL keep the standard accent color.

#### Scenario: Gear card part checkboxes keep the standard accent
- **WHEN** the Stone palette is active and a gear card shows a checked part
- **THEN** that part's checkbox uses the standard accent color, while checked chips in the filter row use heather

### Requirement: Dropdowns follow the palette
Every dropdown in the application SHALL follow the active palette: all ComboBoxes and the Wishlist and Profile selector popups. A closed dropdown SHALL use the palette's field background and outline, with the hover tint while the pointer is over it. In an open list, the item under the pointer SHALL use the hover tint, and the selected (ComboBox) or active (selector popup) item SHALL use the selection color.

#### Scenario: Open sort list marks hover and selection
- **WHEN** the user opens the relic sort-mode dropdown with Era selected and points at Alphabetical
- **THEN** Era shows the palette's selection color and Alphabetical shows the hover tint

#### Scenario: Pointed-at entry in the selector popup shows the hover tint
- **WHEN** the user opens the Wishlist selector popup and points at an inactive wishlist
- **THEN** that entry shows the palette's hover tint

#### Scenario: Dropdowns outside the control bars follow the palette
- **WHEN** the Stone palette is active and the user opens the Drop Tables source selector
- **THEN** its field, hovered item, and selected item use the Stone colors

### Requirement: Selector popups mark the active entry by color alone
In the Wishlist and Profile selector popups, the active entry SHALL be marked only by the palette's selection color, the same way a ComboBox marks its selected item. No check glyph SHALL be shown, and entry names SHALL stay aligned whether or not they are active.

#### Scenario: Active wishlist is marked like the selected sort mode
- **WHEN** the user opens the Wishlist selector popup
- **THEN** the active wishlist's entry shows the palette's selection color, as the selected item does in the sort-mode dropdown, and no ✓ appears next to it

#### Scenario: Active profile is marked the same way
- **WHEN** the user opens the Profile selector popup on the Settings tab
- **THEN** the active profile's entry shows the selection color and no ✓

### Requirement: Card menus follow the palette
The `⋮` menus on relic cards (Verbatim and Compact modes) and gear cards SHALL follow the active palette: the item under the pointer SHALL use the hover tint, a pressed item the pressed tint, and an item reached with the keyboard the hover tint.

#### Scenario: Pointed-at card menu item shows the hover tint
- **WHEN** the user opens a gear card's `⋮` menu and points at "Mark as owned"
- **THEN** the item shows the palette's hover tint

### Requirement: Card menus open with no item highlighted
Opening a card's `⋮` menu with the pointer SHALL NOT highlight any item. An item SHALL be highlighted only while the pointer is over it, or after the user moves to it with the keyboard.

#### Scenario: Menu opens plain
- **WHEN** the user clicks a relic card's `⋮` button
- **THEN** the menu opens and "Compose Recruiting Chat message…" is shown without the hover tint until the pointer moves over it

#### Scenario: Keyboard navigation still highlights
- **WHEN** a card menu is open and the user presses the Down arrow key
- **THEN** the item moved to is highlighted with the hover tint

### Requirement: Profile selector shares the Wishlist selector's field look
The Profile selector button on the Settings tab SHALL have the same palette-driven field look (background, outline, hover tint) as the Wishlist selector on the Prime Gear tab.

#### Scenario: Profile selector looks like the Wishlist selector
- **WHEN** the Settings tab is shown
- **THEN** the Profile selector button has the same field background and outline as the Wishlist selector on the Prime Gear tab

### Requirement: Bar fields share the palette's field background
Text input fields on control bars (the Prime Gear search box) SHALL use the palette's field background, the same as the button chips and the wishlist selector, rather than letting the bar color show through.

#### Scenario: Search box is not tinted by the bar
- **WHEN** the Prime Gear tab is shown under the Linen palette
- **THEN** the search box background matches the wishlist selector's background, not the linen bar

### Requirement: Inactive controls take their colors from the palette
Each palette SHALL define an inactive text color and an inactive outline color. Every inactive bar button, chip, and dropdown SHALL keep the field background and show its text and outline in those colors, so inactive controls look the same across the app and each palette tunes their contrast.

#### Scenario: Inactive button follows the palette
- **WHEN** group operations is enabled with nothing selected under the Stone palette
- **THEN** the inactive Select none, Mark owned, and Unmark owned buttons use the Stone palette's inactive colors

### Requirement: Palette changes apply immediately
Selecting a palette SHALL recolor the main window immediately, whatever tab it is showing, without a restart.

#### Scenario: Palette switch while another tab is shown
- **WHEN** the main window shows the Drop Tables tab and the user selects Stone in the Appearance window
- **THEN** the footer and, on switching tabs, the control bars already show the Stone colors without restarting

### Requirement: Palette choice persists across sessions
The selected palette SHALL be saved when chosen and restored at startup. A settings file written before this capability, or holding an unrecognized palette value, SHALL load with Linen.

#### Scenario: Palette is restored after restart
- **WHEN** the user selects Stone and restarts the application
- **THEN** the application starts with the Stone palette and the Appearance window shows Stone selected

#### Scenario: Older settings default to Linen
- **WHEN** the application loads a settings file that has no palette value
- **THEN** the Linen palette is active
