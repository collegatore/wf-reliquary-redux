## MODIFIED Requirements

### Requirement: Relic Card Width Persistence and Default

The relic card base width SHALL be persisted in user settings and restored on the next launch. When no relic card width preference is saved, the default base width SHALL be 200 pixels.

#### Scenario: Default relic card width on first launch

- **WHEN** the application starts and no relic card width preference is saved
- **THEN** relic cards are rendered at 200 pixels wide at Medium font size

#### Scenario: Relic card width restored from settings

- **WHEN** the application starts and a relic card width preference is saved
- **THEN** relic cards are rendered at the saved base width, scaled by the current font-size ratio
