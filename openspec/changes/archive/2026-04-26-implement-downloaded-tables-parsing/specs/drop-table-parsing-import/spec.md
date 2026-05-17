## ADDED Requirements

### Requirement: Stored Drop Table Source Import

The system SHALL provide an application-layer import workflow that loads the
previously downloaded Warframe drop-table HTML from local source storage and
imports parsed PI1 data from that stored file.

#### Scenario: Stored source is imported
- **WHEN** a stored drop-table HTML file exists with provenance
- **THEN** the import workflow parses the stored HTML and returns an import
  result that includes source path, source Last Update when available, import
  timestamp, and imported row counts

#### Scenario: Stored source is missing
- **WHEN** no stored drop-table HTML file exists
- **THEN** the import workflow fails without creating parsed data and reports
  that the source file must be downloaded first

#### Scenario: Stored source is malformed
- **WHEN** the stored drop-table HTML cannot be parsed into the required Relics
  and Missions sections
- **THEN** the import workflow fails without replacing the previous parsed data
  set

### Requirement: Relic Reward Parsing

The system SHALL parse the source `Relics` section into base relics, Prime Part
reward items, and relic reward rows suitable for PI1 Prime Part acquisition
queries.

#### Scenario: Base relic is parsed from refinement table
- **WHEN** a relic reward table is headed by a label such as
  `Lith C14 Relic (Intact)`
- **THEN** the parser records the base relic era, code, and display name without
  treating each refinement level as a separate relic

#### Scenario: Prime Part reward row is parsed
- **WHEN** an Intact relic table contains a Prime Part reward row
- **THEN** the parser records the reward item, source rarity, source chance,
  and derived reward tier

#### Scenario: Reward tier is derived from Intact chance
- **WHEN** an Intact relic reward row has chance `25.33%`, `11.00%`, or `2.00%`
- **THEN** the parser maps the relic reward tier to Common, Uncommon, or Rare
  respectively

#### Scenario: Non-Prime relic reward is encountered
- **WHEN** a relic reward row does not represent a PI1 Prime Part or blueprint
- **THEN** the parser preserves the reward item row with its source context and
  classifies it outside the Prime Part subset

### Requirement: Mission Reward Parsing

The system SHALL parse the source `Missions` section into mission sources,
optional rotations, mission reward rows, source rarity labels, and mission drop
percentages.

#### Scenario: Mission source label is parsed
- **WHEN** a mission reward table has a source label such as
  `Mercury/Apollodorus (Survival)`
- **THEN** the parser preserves the full display label and records parsed
  region, node, and mission type fields when available

#### Scenario: Duplicate mission source labels are parsed
- **WHEN** the Missions section contains multiple source headings with the same
  display label
- **THEN** the parser preserves them as distinct mission source records with
  source order, and links each rotation and reward row to the matching source
  occurrence rather than using the display label alone

#### Scenario: Mission reward rotation is parsed
- **WHEN** mission reward rows are grouped under `Rotation A`, `Rotation B`, or
  `Rotation C`
- **THEN** the parser records the rotation label and links affected reward rows
  to that rotation

#### Scenario: Unrotated mission reward is parsed
- **WHEN** a mission reward table contains rows without a rotation heading
- **THEN** the parser records those reward rows with no rotation link while
  preserving their mission source context

#### Scenario: Relic mission reward is linked
- **WHEN** a mission reward item name matches a parsed base relic such as
  `Lith C14 Relic`
- **THEN** the mission reward row links to that relic while preserving the
  original reward item name, rarity label, and mission drop percentage

#### Scenario: Non-relic mission reward is preserved
- **WHEN** a mission reward item is not a relic
- **THEN** the parser stores the mission reward row without a relic link

### Requirement: Parsed Import Persistence

The system SHALL persist parsed drop-table import data in SQLite behind
application-owned storage contracts.

#### Scenario: Import persistence succeeds
- **WHEN** parsing completes successfully
- **THEN** the system stores import metadata, reward items, relics, relic
  rewards, mission sources, reward rotations, and mission rewards in SQLite

#### Scenario: Existing import is replaced atomically
- **WHEN** a successful import is persisted after a previous import exists
- **THEN** the parsed data set is replaced as one coherent snapshot without
  mixing rows from different source imports

#### Scenario: Import persistence fails
- **WHEN** SQLite persistence fails during import replacement
- **THEN** the previous parsed data set remains available and the failed import
  reports an error

### Requirement: Source Fidelity Preservation

The system SHALL preserve enough source context to audit parsed rows against
the downloaded HTML.

#### Scenario: Duplicate source rows exist
- **WHEN** the source contains duplicate reward names in the same or different
  reward pools
- **THEN** the import preserves distinct rows with source order and parent
  source context

#### Scenario: Duplicate mission source labels exist
- **WHEN** the source contains multiple mission reward pools with the same
  display label, including the same rotation label under each pool
- **THEN** persistence allows the duplicate source labels and keeps rewards and
  rotations linked to the correct mission source occurrence

#### Scenario: Mission label cannot be fully structured
- **WHEN** a mission source label cannot be parsed into region, node, mission
  type, or qualifier fields
- **THEN** the import preserves the full display label and leaves unavailable
  structured fields empty

#### Scenario: Source provenance is available
- **WHEN** the stored source has provenance metadata
- **THEN** the persisted import metadata includes source URL, downloaded
  timestamp, and Last Update when available

### Requirement: Import Layer Boundaries

The implementation SHALL preserve the established layer separation for parsing
and persistence behavior.

#### Scenario: Application defines import contracts
- **WHEN** project references are inspected
- **THEN** parser, import workflow, and import storage contracts are owned by
  the Application project without Application referencing Infrastructure,
  Presentation, or Composition

#### Scenario: Infrastructure owns SQLite details
- **WHEN** parsed import data is persisted
- **THEN** SQLite schema creation, transactions, and SQL commands are
  implemented in Infrastructure behind application-owned contracts

#### Scenario: Composition wires concrete import service
- **WHEN** the runnable app or tests need the concrete import workflow
- **THEN** Composition wires application services to infrastructure
  implementations without Presentation directly referencing Infrastructure

### Requirement: Manual Import Testing Action

The system SHALL provide a minimal presentation-layer button for manually
testing the stored drop-table import workflow without refreshing or
re-downloading the source file.

#### Scenario: Import test button starts import
- **WHEN** a user activates the import testing button
- **THEN** the presentation invokes the composed application import workflow
  for the already stored source file without referencing infrastructure
  directly

#### Scenario: Import test button does not download source
- **WHEN** a user activates the import testing button
- **THEN** the presentation does not invoke source refresh, live network
  download, or any workflow that replaces the stored drop-table source file

#### Scenario: Import test is running
- **WHEN** the manual import test is in progress
- **THEN** the presentation indicates that import is running and prevents a
  duplicate import test from being started by the same action

#### Scenario: Import test succeeds
- **WHEN** the manual import test completes successfully
- **THEN** the presentation displays success status, source Last Update when
  available, and imported row counts

#### Scenario: Import test fails
- **WHEN** the manual import test fails because the source is missing,
  malformed, or persistence fails
- **THEN** the presentation displays the failure message returned by the
  application import workflow

### Requirement: Import Validation

The system SHALL include automated tests for parser behavior, import
orchestration, SQLite persistence, source fidelity, and dependency direction.

#### Scenario: Focused parser tests run
- **WHEN** `dotnet test` is run from the repository root
- **THEN** unit tests validate relic parsing, mission rotation parsing,
  unrotated reward parsing, relic linking, duplicate row preservation, and
  malformed source handling without live network access

#### Scenario: Sample import test runs
- **WHEN** integration tests import the preserved Warframe drop-table sample
- **THEN** the imported data includes non-zero relics, Prime Part reward rows,
  mission sources, rotations, and relic-linked mission rewards

#### Scenario: Manual import action test runs
- **WHEN** presentation or view-model tests run
- **THEN** they validate that the import testing action invokes the application
  import workflow, does not invoke source refresh or re-download behavior,
  blocks duplicate runs while in progress, and reports success or failure status
  where existing test infrastructure supports it

### Requirement: Import Scope Boundary

The drop-table parsing import feature SHALL NOT add user-facing Prime Part
search, acquisition-path display, mission ranking, wishlist behavior, live
network refresh, or background scheduling.

#### Scenario: Parsed data is available
- **WHEN** a stored source has been imported successfully
- **THEN** the feature stops at persisted parsed data, import status, and the
  manual import testing action until a later approved query or UI change
  consumes that data
