# Spec Delta

## ADDED Requirements

### Requirement: Menu markers use the palette's selection accent
In menus opened from a control-bar button (the Filters and Display menus), the check mark of a checked entry and the dot of the chosen radio entry SHALL use the palette's selection accent, the same color as a checked chip's tick.

#### Scenario: Checked filter entry uses the selection accent
- **WHEN** the Stone palette is active and the user opens a Filters menu with Vaulted checked
- **THEN** the Vaulted entry's check mark is drawn in Stone's selection accent

#### Scenario: Chosen view mode uses the selection accent
- **WHEN** the Linen palette is active and the user opens the Display menu
- **THEN** the dot marking the active view mode is drawn in Linen's selection accent

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
