# Project Baseline Specification

## Purpose

This spec captures the confirmed baseline for WF Reliquary Redux as of
2026-04-19. It consolidates the implemented boilerplate from
`specs/002-app-layer-boilerplate/` and the conducted schema research from
`specs/001-drop-table-schema/` so future OpenSpec changes have a stable
reference point.

## Requirements

### Requirement: Product Purpose Baseline

WF Reliquary Redux SHALL remain a Warframe companion desktop application whose
primary purpose is to help players identify missions, relics, rotations, and
reward sources for desired Prime Parts and related acquisition planning.

#### Scenario: Contributor checks project intent
- **WHEN** a contributor reviews the baseline spec
- **THEN** they can identify that the product is focused on Warframe drop-table
  analysis and farming decision support

#### Scenario: Future change proposes unrelated scope
- **WHEN** a proposed change adds behavior unrelated to Warframe acquisition
  planning
- **THEN** the proposal identifies why that behavior belongs in this product
  before implementation proceeds

### Requirement: Approved Technology Stack

The project SHALL use C# with .NET 10, Avalonia 12 for desktop presentation,
SQLite through Microsoft.Data.Sqlite for local storage, and NUnit 4 for unit
and integration-style validation unless a later approved specification records
and justifies a deviation.

#### Scenario: Dependency change is proposed
- **WHEN** a change introduces a new primary runtime, UI, storage, or testing
  dependency
- **THEN** the change documents the deviation from the baseline stack and the
  reason it is necessary

#### Scenario: Contributor validates local toolchain
- **WHEN** a contributor prepares the repository locally
- **THEN** the required SDK and validation workflow are based on .NET 10 CLI
  commands

### Requirement: Layered Solution Baseline

The repository SHALL preserve the root `WF.Reliquary.Redux.sln` solution with
separate presentation, application, infrastructure, unit test, and integration
test projects under the established `src/` and `tests/` layout.

#### Scenario: Contributor locates implementation boundaries
- **WHEN** a contributor reviews the repository layout
- **THEN** they can locate presentation code in `src/Presentation/`,
  application code in `src/Application/`, infrastructure code in
  `src/Infrastructure/`, unit tests in `tests/Unit/`, and integration tests in
  `tests/Integration/`

#### Scenario: Solution-level validation runs
- **WHEN** a contributor runs restore, build, or test from the repository root
- **THEN** the root solution is the primary validation entry point

### Requirement: Dependency Direction Baseline

The application layer SHALL remain independent of presentation and
infrastructure implementation projects. Presentation MAY reference application.
Infrastructure MAY reference application to implement application-owned ports.
Presentation SHALL NOT reference infrastructure directly unless a future
approved specification changes the composition model.

#### Scenario: Application project references are inspected
- **WHEN** a reviewer inspects the application project file
- **THEN** it contains no project references to presentation or infrastructure
  implementation projects

#### Scenario: Adapter boundary is implemented
- **WHEN** infrastructure provides persistence or file access behavior
- **THEN** the behavior is exposed through application-owned contracts rather
  than by making application depend on infrastructure

### Requirement: Current Boilerplate Behavior

The current runnable application baseline SHALL remain placeholder-only:
application metadata can report the product name and boilerplate version,
infrastructure can prove the SQLite provider wiring shape, and presentation can
build as an Avalonia shell without product workflows.

#### Scenario: Application placeholder is queried
- **WHEN** the application metadata summary is requested
- **THEN** it includes `WF Reliquary Redux` and the current boilerplate version
  marker

#### Scenario: Infrastructure placeholder is checked
- **WHEN** the SQLite storage health check is executed
- **THEN** it proves a SQLite in-memory connection string can be produced
  without creating runtime schema or database files

#### Scenario: Presentation project builds
- **WHEN** the presentation project is built
- **THEN** it remains a minimal Avalonia shell and does not implement
  drop-table parsing, mission recommendation, item lookup, or persistence
  workflows

### Requirement: Validation Baseline

The baseline validation workflow SHALL pass with `dotnet restore`,
`dotnet build`, and `dotnet test` from the repository root on a prepared
development machine, and the test suite SHALL continue to protect placeholder
application behavior, project availability, dependency direction, infrastructure
wiring, and README guidance.

#### Scenario: Baseline tests are run
- **WHEN** a contributor runs `dotnet test` from the repository root
- **THEN** the unit and integration test projects complete successfully

#### Scenario: README guidance drifts
- **WHEN** contributor-facing commands or layer descriptions change
- **THEN** README guidance and README validation tests are updated in the same
  change

### Requirement: Drop Table Source Research Baseline

The project SHALL treat the official Warframe drop table sample preserved by
`specs/001-drop-table-schema/` as the current design research source for Prime
Part, Void Relic, and Mission Reward modeling until a later import-refresh
feature defines runtime source acquisition.

#### Scenario: Research source is needed
- **WHEN** a planner needs to understand the analyzed source structure
- **THEN** they can use the preserved sample and analysis artifacts under
  `specs/001-drop-table-schema/`

#### Scenario: Runtime source refresh is proposed
- **WHEN** a future change implements live source download or refresh behavior
- **THEN** it defines runtime provenance, error handling, and validation rather
  than relying only on the preserved planning artifact

### Requirement: Drop Table Domain Model Baseline

Future parser, persistence, and query features SHALL preserve the researched
distinction between Reward Items, Prime Parts, Relics, Relic Rewards, Mission
Sources, Reward Rotations, and Mission Rewards.

#### Scenario: Prime Part acquisition path is modeled
- **WHEN** a desired Prime Part is queried conceptually
- **THEN** the model can return matching relics, reward tiers, derived
  refinement chances, and mission sources for those relics

#### Scenario: Relic mission source path is modeled
- **WHEN** a relic is queried conceptually
- **THEN** the model can return mission display labels, parsed mission context,
  optional rotation labels, mission reward rarity labels, and mission reward
  percentages

### Requirement: Probability Semantics Baseline

The project SHALL keep derived relic refinement chances separate from mission
reward drop percentages. Relic reward tiers SHALL determine Intact,
Exceptional, Flawless, and Radiant chances by fixed rules, while mission reward
percentages SHALL represent source-specific mission drop probabilities.

#### Scenario: Relic reward chance is displayed
- **WHEN** a relic reward tier is evaluated for a selected refinement
- **THEN** the chance is derived from the fixed tier-to-refinement table rather
  than stored per relic reward row

#### Scenario: Mission reward chance is displayed
- **WHEN** a mission source can award a relic
- **THEN** the displayed percentage comes from the mission reward row and is not
  combined with relic opening odds unless a later ranking feature explicitly
  defines that calculation

### Requirement: Source Fidelity Baseline

Future drop-table parsing and persistence SHALL preserve source-row context,
full mission display labels, parsed mission fields where available, duplicate
reward rows, unrotated rewards, rotated rewards, special-source qualifiers, and
non-relic contextual mission reward rows needed to keep reward pools auditable.

#### Scenario: Mission label is ambiguous
- **WHEN** a mission source label cannot be fully parsed into region, node,
  mission type, or qualifier
- **THEN** the full display label is preserved and parsed fields remain
  optional

#### Scenario: Duplicate reward names appear
- **WHEN** the source contains repeated reward names in separate rows or pools
- **THEN** those rows remain distinguishable through source context and order

### Requirement: Future Implementation Scope Boundary

The current baseline SHALL NOT be treated as having implemented drop-table
parsing, SQLite schema creation, import persistence, item lookup, mission
recommendation, acquisition ranking, wishlist tracking, OCR, alerting, or full
product UI workflows. Each of those behaviors SHALL be introduced through a
separate approved change.

#### Scenario: Feature depends on parser behavior
- **WHEN** a future change requires parsed Warframe drop-table data
- **THEN** it either introduces that parser behavior in scope or depends on a
  prior approved parser implementation

#### Scenario: Feature depends on persisted data
- **WHEN** a future change requires SQLite tables or stored import rows
- **THEN** it defines schema, migrations or creation flow, validation, and test
  coverage before relying on persisted data
