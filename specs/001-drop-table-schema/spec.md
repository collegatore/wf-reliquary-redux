# Feature Specification: Drop Table Schema Preparation

**Feature Branch**: `001-drop-table-schema`
**Created**: 2026-04-18
**Status**: Draft
**Input**: User description: "This is a preparational stage of the project. In this stage, a sample of Warframe drop tables should be downloaded from https://warframe-web-assets.nyc3.cdn.digitaloceanspaces.com/uploads/cms/hnfvc0o3jnfvc873njb03enrf56.html and stored in the design artifacts folder for further analysis. Then, its part related to Prime Parts, Relics, and Mission Rewards should be analyzed to define the DB structure suitable for storage and querying these entities."

## Clarifications

### Session 2026-04-18

- Q: What is the expected deliverable for this feature? -> A: A set of design documents and artifacts only; this feature must not produce or modify application source code.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Preserve Official Drop Table Sample (Priority: P1)

A project maintainer needs the referenced Warframe drop table page captured as a local design artifact so the team can analyze a stable sample while planning the first product increment.

**Why this priority**: The schema cannot be validated against the target source without a preserved sample of the source data.

**Independent Test**: Verify the feature directory contains the downloaded HTML sample, records the source URL, and identifies the source's own last-update date.

**Acceptance Scenarios**:

1. **Given** the referenced public drop table URL is available, **When** the preparatory step is completed, **Then** the raw HTML sample is present in the feature design artifacts.
2. **Given** the sample has been stored locally, **When** a planner reviews the artifacts, **Then** they can see the original source URL and the source's `08 April, 2026` last-update marker.

---

### User Story 2 - Understand Relic and Mission Reward Relationships (Priority: P1)

A product analyst needs the Prime Part, Relic, and Mission Reward portions of the drop table interpreted into clear relationships that describe how a player moves from a desired Prime Part to relics and then to missions that can award those relics.

**Why this priority**: The first product increment depends on modeling the acquisition chain from Prime Part to Relic to mission source, including rotation-specific chances.

**Independent Test**: Review the analysis artifact and confirm it describes relic reward rows, reward tiers, fixed refinement chance rules, mission reward rows, rotations, mission rarity labels, and mission drop percentages using examples from the downloaded sample.

**Acceptance Scenarios**:

1. **Given** a Prime Part appears in the Relics section, **When** the analysis is reviewed, **Then** the required relationship to each base relic, reward tier, and derived refinement chance rule is described.
2. **Given** a relic appears in the Missions section, **When** the analysis is reviewed, **Then** the required relationship to mission, mission type, rotation when present, rarity label, and drop chance is described.

---

### User Story 3 - Define Query-Ready Storage Model (Priority: P2)

A developer needs design documents that define a proposed data structure for later storage and query work for Prime Parts, Relics, Relic reward tiers, Mission reward pools, and relic source lookups.

**Why this priority**: Planning and implementation need a shared target model before parser and persistence work begins.

**Independent Test**: Confirm the specification and analysis artifact define testable entities and relationships that can answer Prime Part-to-Relic and Relic-to-Mission source questions without re-reading the raw HTML.

**Acceptance Scenarios**:

1. **Given** a desired Prime Part name, **When** the proposed model is queried conceptually, **Then** it can return matching relics, reward tiers, and derived refinement-specific reward percentages.
2. **Given** a relic name, **When** the proposed model is queried conceptually, **Then** it can return mission sources, rotation labels where present, mission reward rarity labels, and mission reward percentages.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST preserve the referenced Warframe drop table HTML as a local design artifact for this feature.
- **FR-002**: System MUST record the source URL, retrieval date, and source last-update date alongside the analysis.
- **FR-003**: Analysis MUST focus on the Mission Rewards and Relics sections needed for Void Relic and Prime Part acquisition planning.
- **FR-004**: Analysis MUST identify Prime Parts as reward items from relic reward rows, including names, rarity labels, and drop percentages.
- **FR-005**: Analysis MUST identify Relics by era and relic code, and MUST use source refinement labels only where needed to derive reward tiers and chance rules.
- **FR-006**: Analysis MUST represent relic reward tiers and derived refinement chances separately from mission reward probabilities because they answer different player questions.
- **FR-007**: Analysis MUST identify mission reward sources by location label, mission type when available, rotation label when available, reward item, rarity label, and drop percentage.
- **FR-008**: The proposed storage model MUST support a query path from Prime Part to relics and from those relics to mission sources.
- **FR-009**: The proposed storage model MUST preserve source-row context so duplicate reward names, event variants, unrotated rewards, and repeated mission pools can be distinguished.
- **FR-010**: The proposed storage model MUST keep non-relic mission rewards available as contextual reward rows without making broader non-relic acquisition a first-increment requirement.
- **FR-011**: The feature MUST produce a design artifact describing the candidate DB structure, key relationships, and representative query needs.
- **FR-012**: The feature MUST deliver only design documents and preserved design artifacts; it MUST NOT produce or modify application source code, test code, project files, or runtime database files.

### Architecture & Testing Requirements *(mandatory)*

- **AR-001**: The design documents MUST preserve separation between
  presentation, application, and data storage concerns for later implementation.
- **AR-002**: The design documents MUST identify core parser, normalization, and
  query behavior that later implementation should cover with NUnit unit tests.
- **AR-003**: Any validation instructions included by this feature MUST remain
  workable from Visual Studio Code and standard shell tooling on Windows and
  Linux without requiring a runnable codebase.
- **AR-004**: The design MUST remain within the approved stack recorded in the
  constitution and `README.md`, or the spec MUST state and justify any approved
  deviation.
- **AR-005**: User-facing and contributor-facing workflow changes are out of
  scope unless required to locate or review these design artifacts.

### Key Entities *(include if feature involves data)*

- **Source Sample**: A non-persistent design artifact containing the captured public drop table source, source URL, retrieval date, source last-update date, and raw artifact location.
- **Reward Item**: A normalized item name that may represent a Prime Part, relic, blueprint, resource, mod, or contextual mission reward.
- **Prime Part**: A reward item identified from relic reward rows as a Prime item or Prime blueprint relevant to relic acquisition planning.
- **Relic**: A Void Relic identified by era and code, such as Lith C14, Meso D8, Neo C7, or Axi A1.
- **Relic Reward**: A reward slot connecting a base relic to a Prime Part or other relic reward item, with a reward tier of Common, Uncommon, or Rare.
- **Mission Source**: A mission reward table identified by its source label, including planet/location text, mission type when available, and event or special-source qualifiers when present.
- **Reward Rotation**: An optional mission reward grouping such as Rotation A, Rotation B, or Rotation C.
- **Mission Reward**: A probability row connecting a mission source and optional rotation to a reward item, with rarity label and drop percentage.

## Success Criteria *(mandatory)*

- **SC-001**: The downloaded sample artifact can be located in the feature directory in under 1 minute by a project contributor.
- **SC-002**: The analysis identifies at least 95% of the data fields needed for Prime Part-to-Relic and Relic-to-Mission source queries from the sample sections.
- **SC-003**: A reviewer can trace at least 3 representative acquisition paths from Prime Part to Relic to Mission Source using only the proposed model, fixed reward-tier chance rules, and the preserved sample.
- **SC-004**: The proposed model covers rotated and unrotated mission reward rows without requiring separate ad hoc structures.
- **SC-005**: Planning can proceed without additional clarification about the core entities, relationships, or first-increment scope boundaries.
- **SC-006**: The feature is complete when the spec, plan, research notes, data model, parser/query contracts, quickstart, analysis artifact, and preserved sample are present; no application source or runtime database files are required.

## Assumptions

- The preserved sample is a planning artifact, not the final production refresh mechanism.
- This feature is a documentation and design-artifact deliverable only; implementation tasks are deferred to later features.
- The first increment is limited to Void Relics, Prime Parts obtained from relics, and mission sources that award those relics.
- The source's `Relics` section is treated as authoritative for base relic reward slots; reward tiers are derived from Intact relic chances, and refinement-specific chances follow the fixed relic reward-tier table.
- The source's `Missions` section is treated as authoritative for mission reward chances and rotation labels where present.
- Broader non-relic item acquisition remains out of scope except as contextual rows needed to preserve complete mission reward pools.
