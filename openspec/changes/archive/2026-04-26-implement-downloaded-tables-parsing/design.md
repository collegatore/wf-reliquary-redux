## Context

WF Reliquary Redux already has a layered .NET 10/Avalonia solution, an
application-owned drop-table source refresh workflow, infrastructure adapters
for HTTP and local raw-file storage, and a preserved Warframe drop-table sample
with schema analysis under `specs/001-drop-table-schema/`. The current runtime
feature boundary stops at downloading HTML and exposing its Last Update value.

PI1 needs the next ingestion step: load the already stored HTML, parse the
source's `Relics` and `Missions` sections, normalize the rows into the researched
logical model, and persist them in SQLite for later query features. The parser
must keep source fidelity high because the official page can contain duplicate
rows, optional rotations, ambiguous or duplicate mission labels, and contextual
non-relic mission rewards.

## Goals / Non-Goals

**Goals:**

- Load the previously downloaded raw drop-table HTML through the existing
  application-owned source-store port.
- Parse relic reward tables into base relics, Prime Part reward items, and
  reward tiers.
- Parse mission reward tables into mission sources, optional reward rotations,
  mission reward rows, rarity labels, drop percentages, and relic links when the
  reward item is recognized as a relic.
- Persist parsed import data in SQLite using infrastructure-owned storage behind
  application-owned ports.
- Preserve source Last Update, source path, import timestamp, row counts,
  source order, full display labels, duplicate mission source occurrences, and
  parse warnings/errors.
- Provide deterministic import replacement behavior so a successful import
  replaces the previous parsed data set atomically.
- Cover parser and persistence behavior with NUnit tests using focused fixtures
  and the preserved sample as an integration-scale validation source.
- Expose a minimal manual import test button in the presentation layer that
  invokes the composed import workflow for the already stored source file and
  displays success/failure status plus imported row counts.

**Non-Goals:**

- Do not download live data during import; the existing source refresh workflow
  owns network access.
- Do not let the manual import testing button trigger source refresh or
  re-download the drop-table file.
- Do not add Prime Part search UI, acquisition-path UI, mission ranking,
  wishlist tracking, or recommendation behavior.
- Do not combine mission reward chance with relic opening chance.
- Do not require perfect parsing of every possible mission-label format; preserve
  the full source label when structured fields are unavailable.
- Do not implement background scheduling or automatic refresh/import coupling.
- Do not add Prime Part search, acquisition-path, ranking, wishlist, or other
  data-consumption UI beyond the import testing action.

## Decisions

### Decision: Application defines import use case and ports

Add application-layer contracts and models for:

- `IDropTableImportWorkflow`: loads the stored source and imports it.
- `IDropTableParser`: turns raw HTML plus provenance into a parsed import model.
- `IDropTableImportStore`: replaces and reads the normalized SQLite-backed data
  set.
- Import result/status models with source Last Update, imported-at timestamp,
  row counts, warnings, and failure messages.

Rationale: parsing/import is a product use case and should remain testable
without Avalonia, file-system, or SQLite details leaking into application code.

Alternative considered: implement parsing directly inside infrastructure because
it reads HTML. This was rejected because parsing determines domain semantics,
reward tiers, source fidelity, and future query contracts.

### Decision: Parser consumes stored HTML, not a live URL

The import workflow should call the existing `IDropTableSourceStore.LoadAsync`
with the configured drop-table file name. If no stored file exists, import
returns a failed result explaining that the user must download the source first.

Rationale: source refresh and parsing have separate failure modes. Keeping them
separate avoids live-network tests and lets users import a known stored source.

Alternative considered: make import download automatically. This was rejected
because it mixes network refresh with parsing and makes deterministic validation
harder.

### Decision: Persist a complete import snapshot

Infrastructure should store a snapshot with import metadata and normalized
tables for reward items, relics, relic rewards, mission sources, reward
rotations, and mission rewards. A successful import should replace the previous
snapshot inside one SQLite transaction. A failed import should leave the
previous imported data untouched.

Rationale: future queries need a coherent data set. Snapshot replacement keeps
refresh behavior simple until incremental import or migrations become valuable.

Alternative considered: append new rows on every import. This was rejected
because duplicate snapshots would make future queries ambiguous.

### Decision: Derive relic reward tiers from Intact chances

For each base relic, the parser should use the Intact refinement table to derive
reward tier by chance:

- `25.33%` -> `Common`
- `11.00%` -> `Uncommon`
- `2.00%` -> `Rare`

Other refinement tables can validate consistency but do not create separate
relic reward rows. Future UI can derive refinement-specific odds from the fixed
tier table.

Rationale: the researched model separates persistent reward tiers from
refinement-specific display chances.

Alternative considered: store every refinement row as a separate reward. This
was rejected because it duplicates relic reward rows and blurs source odds with
derived relic-opening odds.

### Decision: Mission parsing preserves labels before structured parsing

Mission source records should always store the full source table label. The
parser may fill optional region, node, mission type, and qualifier fields when a
label matches known patterns, but unknown labels must still import.
The full label is display/audit text only; it is not a uniqueness key. Duplicate
mission source labels must remain separate source records, and rotations and
mission rewards must link by source occurrence, such as internal ID/source
order, rather than by label alone.

Rationale: Warframe source labels include special events, variants, and unusual
reward pools. Preserving the label keeps the row auditable even when structured
fields are incomplete.

Alternative considered: reject mission labels that cannot be parsed. This was
rejected because it would discard valid reward pools and create brittle imports.

### Decision: Keep all mission reward rows, link relic rows when recognized

Mission rewards should store every row in PI1-relevant mission tables, including
non-relic rewards, while adding a direct relic link only when the reward item
matches the base relic pattern.

Rationale: complete reward pools are useful for validation and future display,
but PI1 acquisition-path queries primarily traverse rows where a mission can
award a relic.

Alternative considered: persist only mission rows that award relics. This was
rejected because it loses source context and makes pool-level auditing harder.

### Decision: Use focused parser fixtures plus sample-scale integration tests

Unit tests should cover minimal HTML cases for relic tables, mission rotations,
unrotated rewards, duplicate rows, malformed percentages, and missing sections.
Integration tests should run against the preserved sample file to validate row
counts are non-zero and representative known rows are imported.

Rationale: small fixtures make failures understandable, while the preserved
sample protects against drift between intended behavior and real source shape.

Alternative considered: test only against the full sample. This was rejected
because failures would be harder to diagnose and maintain.

### Decision: Add a minimal import testing button

Presentation should include a narrow manual action for testing the import
workflow during PI1. The button invokes the composed `IDropTableImportWorkflow`
from the UI layer against the already stored source file, reports progress while
the import is running, then displays success/failure status, source Last Update
when available, and imported row counts or failure messages. It must not call
the source refresh workflow or re-download the drop-table file.

Rationale: the parser and SQLite import path need a runtime validation path
before search/query screens exist, and a minimal import-only button lets
contributors test parsing and persistence without mixing in network refresh
behavior or adding a full product surface.

Alternative considered: keep import accessible only through tests/composition.
This was rejected because it would make manual validation of the stored source
workflow unnecessarily indirect.

## Risks / Trade-offs

- Source HTML structure changes -> Keep parser errors explicit, preserve raw
  source metadata, and use focused tests around heading/table traversal.
- SQLite schema chosen too early -> Keep schema narrow and aligned with the
  researched logical model; future query features can add read models if needed.
- Import takes noticeable time on large source files -> Use async file/database
  operations and one transaction; defer progress UI until import is user-facing.
- Ambiguous mission labels -> Preserve full labels and treat structured fields
  as optional.
- Duplicate reward names or mission source labels -> Preserve source order and
  row context rather than deduplicating source rows or constraining display
  labels to be unique.

## Migration Plan

1. Add application import models, parser/store ports, and workflow orchestration.
2. Implement parser behavior for relic and mission sections with focused tests.
3. Implement SQLite schema creation/replacement behind the import store.
4. Wire the import workflow in composition without changing presentation's direct
   project-reference boundaries.
5. Add integration tests that import minimal fixtures and the preserved sample.
6. Add the manual import testing button and status display in presentation,
   ensuring the button triggers import only and never refreshes/downloads the
   source file.
7. Update README with the import testing action because a user-visible UI action
   is added during implementation.

Rollback is simple before user-facing query features depend on the parsed data:
remove the import workflow and SQLite import store, leaving the raw source
refresh feature intact.

## Open Questions

- Which HTML parsing package, if any, should be adopted after implementation
  spikes the source shape against standard .NET APIs?
