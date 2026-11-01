# Spec Delta

## MODIFIED Requirements

### Requirement: Appearance Window Content

The Appearance window SHALL display the font size selector (Small, Medium, Large, Custom with numeric input), the relic card width control, the Relics/Missions layout orientation control, and the **palette selector** (Linen / Stone, specified by the `color-palette-setting` capability). The relic view mode is chosen from the Display menu on the Relics bar (see the `relics-display-menu` capability), not here. The controls carried over from the Settings tab remain unchanged in function.

#### Scenario: Appearance controls are present

- **WHEN** the Appearance window is open
- **THEN** the font size selector, relic card width control, layout orientation control, and palette selector are all visible, and no relic view mode selector is shown

#### Scenario: Palette selector shows the active palette

- **WHEN** the Appearance window opens while the Linen palette is active
- **THEN** the palette selector shows Linen selected
