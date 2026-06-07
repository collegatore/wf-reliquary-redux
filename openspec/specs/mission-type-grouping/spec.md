## Requirements

### Requirement: Missions of the same type and identical reward table are rendered as one card
The farming missions panel SHALL group missions that share the same non-null `MissionType` and an identical reward table (same set of relics with the same individual chances per rotation or flat pool) into a single card. The card's title SHALL be the `MissionType` string. Missions with a null `MissionType` SHALL each appear as their own card using the existing `DisplayLabel` as the title.

#### Scenario: Two same-type missions with identical relics are merged
- **WHEN** two missions share the same `MissionType` and both offer relic R1 at 11.11% and relic R2 at 7.41%
- **THEN** they appear as a single card titled with the mission type, not two separate cards

#### Scenario: Two same-type missions with different relics remain separate
- **WHEN** two missions share the same `MissionType` but offer different relics or different chances
- **THEN** they each appear as a separate card

#### Scenario: Mission with null type keeps its own card and existing display label
- **WHEN** a mission has a null `MissionType`
- **THEN** it appears as a standalone card with the mission's `DisplayLabel` as the title

#### Scenario: Group card is still sorted by effective chance
- **WHEN** grouped cards are rendered
- **THEN** they are ordered by effective wishlist-relic drop chance descending, same as before grouping

---

### Requirement: Group card body lists the member mission nodes before the relic content
A group card's body SHALL display a "Nodes" section immediately below the card title separator. The nodes section SHALL list the `Node` (or `DisplayLabel` when `Node` is null) of each member mission as a comma-separated inline text or as separate lines. The existing rotation or flat-relic content SHALL follow the nodes section, unchanged in structure.

#### Scenario: Nodes section shows all member missions
- **WHEN** a group card represents three missions with nodes "Io", "Hydron", and "Stöfler"
- **THEN** all three node labels are visible in the nodes section of the card

#### Scenario: Nodes section appears before relics
- **WHEN** a group card is rendered
- **THEN** the nodes section appears above the rotation or relic list

#### Scenario: Singleton group shows node section with a single entry
- **WHEN** a group contains exactly one mission
- **THEN** the nodes section still appears and shows that mission's node label
