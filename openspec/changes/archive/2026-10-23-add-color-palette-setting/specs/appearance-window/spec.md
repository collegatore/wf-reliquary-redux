# Spec Delta

## MODIFIED Requirements

### Requirement: Appearance Window Content

The Appearance window SHALL display the font size selector (Small, Medium, Large, Custom with numeric input), the relic card width control, the Relics/Missions layout orientation control, the **relic view mode selector** (Verbatim / Compact / Table), and the **palette selector** (Linen / Stone, specified by the `color-palette-setting` capability). The controls carried over from the Settings tab remain unchanged in function.

#### Scenario: Appearance controls are present

- **WHEN** the Appearance window is open
- **THEN** the font size selector, relic card width control, layout orientation control, relic view mode selector, and palette selector are all visible

#### Scenario: Palette selector shows the active palette

- **WHEN** the Appearance window opens while the Linen palette is active
- **THEN** the palette selector shows Linen selected
