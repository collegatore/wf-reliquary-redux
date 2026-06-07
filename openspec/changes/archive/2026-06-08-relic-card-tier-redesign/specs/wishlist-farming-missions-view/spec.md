## ADDED Requirements

### Requirement: Farming-mission card width scales with the selected font size
Each farming-mission card's width SHALL scale proportionally with the application's content font size, using the same font-ratio rule as relic cards, so that the card grows and shrinks together with the text it contains. Card width SHALL update when the font size setting changes, without requiring an application restart.

#### Scenario: Larger font widens mission cards
- **WHEN** the user selects the Large font size
- **THEN** each farming-mission card is rendered wider than it is at the Medium font size

#### Scenario: Smaller font narrows mission cards
- **WHEN** the user selects the Small font size
- **THEN** each farming-mission card is rendered narrower than it is at the Medium font size

#### Scenario: Mission card width updates live with the font setting
- **WHEN** the user changes the font size while farming-mission cards are visible
- **THEN** the mission cards resize to the new width without an application restart
