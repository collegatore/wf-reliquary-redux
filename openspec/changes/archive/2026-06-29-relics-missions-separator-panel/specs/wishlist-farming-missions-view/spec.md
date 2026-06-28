## ADDED Requirements

### Requirement: Mission card uses two dividers with regular-font node names
A mission card SHALL render a horizontal divider after the title/effective-chance header and a second horizontal divider after the node-names line, so the node names sit between the two dividers. The node-names line SHALL be rendered in regular font (non-italic).

#### Scenario: Divider after the header
- **WHEN** a mission card is displayed
- **THEN** a horizontal line separates the title/effective-chance header from the node-names line below it

#### Scenario: Divider after the node names
- **WHEN** a mission card is displayed
- **THEN** a horizontal line separates the node-names line from the relics/rotation content below it

#### Scenario: Node names are regular font
- **WHEN** a mission card's node-names line is displayed
- **THEN** the node names are shown in regular (non-italic) font
