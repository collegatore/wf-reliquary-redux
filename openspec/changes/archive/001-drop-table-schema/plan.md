# Implementation Plan: Drop Table Schema Preparation

**Branch**: `001-drop-table-schema` | **Date**: 2026-04-18 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-drop-table-schema/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. It must
reflect the project constitution before implementation starts.

## Summary

Prepare the first Product Increment data foundation as a design-only feature:
preserve the official Warframe drop table sample as a design artifact,
document how the relevant Mission Rewards and Relics sections map into
normalized application entities, and define the contracts needed for future
parser, persistence, and query work. This feature must not create or modify
application source code, test code, project files, or runtime database files.

## Technical Context

**Language/Version**: C# / .NET 10 (`net10.0`)  
**Primary Dependencies**: Avalonia 12, SQLite via Microsoft.Data.Sqlite, NUnit 4,
AngleSharp for HTML parsing  
**Storage**: SQLite  
**Testing**: No code tests are produced by this feature; design artifacts must
identify future NUnit coverage for parsing, normalization, and query behavior  
**Target Platform**: Windows and Linux desktop  
**Project Type**: Cross-platform desktop application  
**Constraints**: Preserve layer separation; keep VS Code/.NET CLI workflows
viable on Windows and Linux; store relic reward tiers instead of per-variant
relic reward chances; derive refinement-specific relic chances from the fixed
tier table; preserve source-row context for duplicate reward rows, duplicate
mission source labels, and unrotated mission rows

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- `Layer Separation`: PASS. The documents describe future presentation,
  application, and infrastructure boundaries without creating code in any layer.
- `Unit Testing`: PASS. The feature does not add tests, but documents parser,
  normalization, and query behavior that future implementation should cover
  with NUnit.
- `Cross-Platform`: PASS. Artifact validation uses local files and standard
  shell commands that work on Windows and Linux.
- `Technology Stack`: PASS. The design stays inside the approved constitution
  and `README.md` stack: C#/.NET, Avalonia, SQLite, NUnit, and VS Code/.NET CLI.
  AngleSharp is a narrow parser dependency for HTML table extraction and does
  not replace the approved stack. The preserved sample is a design artifact, not
  a persistent DB entity or runtime database input produced by this feature.
- `Documentation`: PASS. Planning artifacts document the import/query model.
  No `README.md` update is required because no executable workflow or codebase
  behavior changes in this design-only feature.

## Project Structure

### Documentation (this feature)

```text
specs/001-drop-table-schema/
|-- plan.md
|-- research.md
|-- data-model.md
|-- quickstart.md
|-- artifacts/
|   |-- drop-table-schema-analysis.md
|   `-- warframe-drop-tables-sample.html
|-- contracts/
|   |-- parser-contract.md
|   `-- query-contract.md
`-- checklists/
    `-- requirements.md
```

### Future Source Code (not produced by this feature)

```text
src/
|-- Presentation/
|   `-- WF.Reliquary.Presentation/
|-- Application/
|   `-- WF.Reliquary.Application/
`-- Infrastructure/
    `-- WF.Reliquary.Infrastructure/

tests/
|-- Unit/
|   `-- WF.Reliquary.UnitTests/
`-- Integration/
    `-- WF.Reliquary.IntegrationTests/
```

**Structure Decision**: This feature produces only design documents and
artifacts under `specs/001-drop-table-schema/`. The source-code tree above is a
future implementation target, not an output of this feature. The documents keep
future importer/query behavior centered on application-layer abstractions,
future HTML parsing and SQLite details in infrastructure, and future UI
consumption in presentation.

## Phase 0 Research

See [research.md](research.md). All future technical-context unknowns are
resolved for planning purposes: .NET 10, Avalonia 12, Microsoft.Data.Sqlite,
NUnit 4, AngleSharp, fixture-based sample validation, and SQLite schema/query
approach. These are design decisions for later implementation, not packages or
projects added by this feature.

## Phase 1 Design

See [data-model.md](data-model.md) for entities, fields, relationships, and
validation rules.

Contracts:

- [parser-contract.md](contracts/parser-contract.md): expected parser input and
  normalized parse output shape.
- [query-contract.md](contracts/query-contract.md): query behaviors needed to
  resolve Prime Part to Relic and Relic to Mission Source paths.

See [quickstart.md](quickstart.md) for validation workflow and expected
planning artifact checks.

## Post-Design Constitution Check

- `Layer Separation`: PASS. The contracts split parser input/output,
  application queries, preparation-only source metadata, and persistence-ready
  entities without UI dependencies or source-code changes.
- `Unit Testing`: PASS. `data-model.md` and contracts identify parser,
  normalization, duplicate reward row, duplicate mission source label,
  optional-rotation, and query-path behaviors that future implementation should
  cover with NUnit.
- `Cross-Platform`: PASS. Quickstart uses repo-relative paths and shell
  commands intended to work on Windows PowerShell, Linux shells, and VS Code
  terminals without requiring a runnable project.
- `Technology Stack`: PASS. No unapproved replacement stack is introduced.
  AngleSharp is selected only as an HTML parser dependency for application data
  ingestion.
- `Documentation`: PASS. Feature docs are the deliverable. `README.md` does not
  require immediate changes because no executable workflow is added in this
  design-only feature.

## Risk Tracking

| Risk | Why It Matters | Mitigation |
|------|----------------|------------|
| Parser assumes too much from current HTML formatting | The official drop table is generated and may change row ordering or table structure | Contract parser around semantic headings, table headers, row cells, and fixture tests rather than brittle line positions |
| Mission source parsing loses source fidelity | Event, Extra, Cache, and unrotated pools can be hard to classify | Store full `display_label` and parsed fields separately; queries can use parsed fields without discarding original text |
| Mission source labels are treated as unique | The official source can repeat source labels, and label-only linking can attach rotations or rewards to the wrong row | Preserve `source_order`; use internal IDs/source occurrence for relationships; do not add uniqueness rules to `mission_sources.display_label` |
| Relic and mission probabilities get merged too early | A Prime Part tier inside a relic and a relic source chance in a mission mean different things | Store `relic_rewards.reward_tier`, compute refinement chances by rule, and keep mission reward chances separate |
| Duplicate reward rows collapse during normalization | Some pools repeat reward names with different source-row meaning | Preserve `source_order` and avoid uniqueness rules that collapse rows within a pool |
| SQLite schema overfits PI1 | Later increments may need broader item acquisition | Keep `reward_items` generic and classify PI1 rows without excluding contextual non-relic rewards |
