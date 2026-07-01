## MODIFIED Requirements

### Requirement: Layout Orientation Toggle

The application SHALL provide a user control, located in the Appearance window (opened via File → Appearance) alongside the font-size and relic card width controls, to switch the Relics and Missions tab between the stacked and side-by-side layouts. The control SHALL be a set of radio options (Stacked / Side by side). Selecting a layout SHALL apply immediately and auto-save (the same way the font-size control behaves), without reloading the underlying relic or mission data, so the Relics and Missions tab reflects the chosen layout the next time it is shown.

#### Scenario: User switches to side-by-side

- **WHEN** the user selects the side-by-side layout in the Appearance window while the saved layout is stacked
- **THEN** the choice is applied and saved immediately, and the Relics and Missions tab shows the side-by-side layout, preserving the currently displayed relics and missions

#### Scenario: User switches back to stacked

- **WHEN** the user selects the stacked layout in the Appearance window while the saved layout is side-by-side
- **THEN** the choice is applied and saved immediately, and the Relics and Missions tab shows the stacked layout
