# Spec Delta

## ADDED Requirements

### Requirement: Menu markers use the palette's selection accent
In menus opened from a control-bar button (the Filters, Sort, and Display menus), the check mark of a checked entry and the dot of the chosen radio entry SHALL use the palette's selection accent, the same color as a checked chip's tick.

#### Scenario: Checked filter entry uses the selection accent
- **WHEN** the Stone palette is active and the user opens a Filters menu with Vaulted checked
- **THEN** the Vaulted entry's check mark is drawn in Stone's selection accent

#### Scenario: Chosen view mode uses the selection accent
- **WHEN** the Linen palette is active and the user opens the Display menu
- **THEN** the dot marking the active view mode is drawn in Linen's selection accent

### Requirement: ComboBoxes follow the palette
Every ComboBox in the application, including the Wishlist and Profile selectors and the Drop Tables source selector, SHALL follow the active palette. A closed ComboBox SHALL use the palette's field background and outline, with the hover tint while the pointer is over it. In an open list, the item under the pointer SHALL use the hover tint, and the selected item SHALL use the selection color.

#### Scenario: Open list marks hover and selection
- **WHEN** the user opens the Wishlist selector with Default active and points at another wishlist
- **THEN** Default shows the palette's selection color and the pointed-at wishlist shows the hover tint

#### Scenario: ComboBoxes outside the control bars follow the palette
- **WHEN** the Stone palette is active and the user opens the Drop Tables source selector
- **THEN** its field, hovered item, and selected item use the Stone colors

### Requirement: Selectors mark the active entry by the selection color only
In the Wishlist and Profile selectors, the active entry SHALL be marked only by the palette's selection color, as any ComboBox marks its selected item. No check glyph SHALL be shown.

#### Scenario: Active wishlist is marked by color
- **WHEN** the user opens the Wishlist selector
- **THEN** the active wishlist's entry shows the palette's selection color and no ✓ appears next to it

#### Scenario: Active profile is marked the same way
- **WHEN** the user opens the Profile selector on the Settings tab
- **THEN** the active profile's entry shows the selection color and no ✓

## MODIFIED Requirements

### Requirement: Two selectable palettes with Linen as the default
The application SHALL offer exactly two color palettes: **Linen** and **Stone**. Linen SHALL be the default. Linen SHALL use warm linen bars, white controls, a fern-green hover tint, and an ink-blue selection color. Stone SHALL use light stone bars, off-white controls, a sage hover tint, and a heather (dusty mauve) selection color.

#### Scenario: Linen is active on first launch
- **WHEN** the application starts with no saved palette
- **THEN** the control bars use the Linen palette

#### Scenario: Stone uses its own selection color
- **WHEN** the Stone palette is active and the user checks the Show exact drop chances chip
- **THEN** the checked chip is tinted heather rather than ink blue

### Requirement: Chip tick follows the palette; other checkboxes keep the standard accent
The checkbox tick inside a checked chip SHALL use the palette's selection accent. Checkboxes that are not chips (for example the part checkboxes on gear cards and checkboxes in Settings) SHALL keep the standard accent color.

#### Scenario: Gear card part checkboxes keep the standard accent
- **WHEN** the Stone palette is active and a gear card shows a checked part
- **THEN** that part's checkbox uses the standard accent color, while a checked Show exact drop chances chip uses heather

## REMOVED Requirements

### Requirement: Dropdowns follow the palette
**Reason**: Its example was the relic sort-mode dropdown, which becomes an icon-only menu button; the rule is restated for the ComboBoxes that remain.
**Migration**: See "ComboBoxes follow the palette" in this capability; menus are covered by "Menus follow the palette".

### Requirement: Selector popups mark the active entry by color alone
**Reason**: Its scenario compared the active wishlist with the selected sort mode, which is no longer a dropdown.
**Migration**: See "Selectors mark the active entry by the selection color only" in this capability.
