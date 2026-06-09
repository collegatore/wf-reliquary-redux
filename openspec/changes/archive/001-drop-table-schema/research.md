# Research: Drop Table Schema Preparation

## Decision: Target .NET 10 (`net10.0`)

**Rationale**: Microsoft lists .NET 10 as supported from 2025-11-11 through
2028-11-14, giving the project the longest current support window. Avalonia 12
documentation recommends .NET 10 while supporting .NET 8 and later.

**Sources**:
- Microsoft .NET lifecycle: https://learn.microsoft.com/en-us/lifecycle/products/microsoft-net-and-net-core
- Avalonia 12 breaking changes: https://docs.avaloniaui.net/docs/avalonia12-breaking-changes

**Alternatives considered**:
- .NET 8 LTS: supported but reaches end of support in 2026, which is too short
  for a new desktop app foundation.
- .NET 9 STS: supported but shorter-lived than .NET 10 and not the recommended
  Avalonia 12 target.

## Decision: Use Avalonia 12 for the future desktop presentation layer

**Rationale**: The constitution and README approve Avalonia for the GUI.
Avalonia 12 is the current documentation track and supports cross-platform
desktop targets including Windows and desktop Linux. This planning feature does
not implement UI, but it keeps future presentation contracts aligned with the
approved UI framework.

**Sources**:
- Avalonia documentation: https://docs.avaloniaui.net/docs/welcome
- Avalonia 12 breaking changes: https://docs.avaloniaui.net/docs/avalonia12-breaking-changes

**Alternatives considered**:
- Avalonia 11: older documentation track; would start the project behind the
  current major version.
- Non-Avalonia UI frameworks: rejected because they conflict with the approved
  stack.

## Decision: Use Microsoft.Data.Sqlite for direct SQLite access

**Rationale**: The approved storage engine is SQLite. Microsoft.Data.Sqlite is
Microsoft's lightweight ADO.NET provider for SQLite and is suitable for explicit
schema/query control, which matters for traceable import rows and acquisition
path queries.

**Source**:
- Microsoft.Data.Sqlite overview: https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/

**Alternatives considered**:
- Entity Framework Core SQLite provider: useful for object mapping but heavier
  than this schema-first import/query layer needs at PI1.
- Raw native SQLite bindings: more platform packaging burden and less aligned
  with standard .NET workflows.

## Decision: Use NUnit 4 for unit tests

**Rationale**: The constitution requires NUnit. Avalonia 12 notes that its
headless testing support targets NUnit 4, which aligns future UI test support
with core parser/query unit tests.

**Sources**:
- NUnit documentation: https://docs.nunit.org/
- Avalonia 12 breaking changes: https://docs.avaloniaui.net/docs/avalonia12-breaking-changes

**Alternatives considered**:
- xUnit or MSTest: rejected because the constitution requires NUnit.
- Manual validation only: rejected because parser, normalization, and query
  behavior can fail silently.

## Decision: Use AngleSharp for HTML parsing

**Rationale**: The source sample is HTML with semantic section headings and
tables. A real HTML parser avoids brittle string slicing and lets future tests
focus on document structure: headings, table headers, rows, cells, and text
content. AngleSharp is documented as a future implementation dependency only;
this feature does not add packages or source code.

**Source**:
- AngleSharp project: https://github.com/AngleSharp/AngleSharp

**Alternatives considered**:
- Regular expressions over raw HTML: rejected as brittle for generated markup
  and harder to maintain.
- Manual line-by-line parsing: rejected because the source table places many
  rows on shared lines and may change formatting.

## Decision: Preserve raw sample as a non-persistent planning artifact

**Rationale**: The raw sample is the audit source for parser behavior. Source
URL, retrieval time, source last-update date, and artifact path belong in
planning artifacts, diagnostics, or import logs, but not in the PI1 persistent
DB schema.

**Alternatives considered**:
- Persist source snapshot rows in the PI1 schema: rejected because this metadata
  is only relevant to preparation and refresh diagnostics, not player-facing
  acquisition queries.
- Store only normalized rows: rejected because parser regressions would be hard
  to diagnose during planning.
- Store only raw HTML: rejected because PI1 queries need structured data.

## Decision: Store relic reward tiers, not persistent refinement records

**Rationale**: Every relic follows the same refinement chance pattern: Common,
Uncommon, and Rare reward tiers map to fixed Intact, Exceptional, Flawless, and
Radiant chances. The persistent relic reward row only needs `RelicId`, `ItemId`,
and `RewardTier`. The tier is derived from the Intact source chance because the
source rarity label in the Relics section is unreliable for this purpose.

**Alternatives considered**:
- Persist refinement records: rejected because those rows would repeat a fixed
  global chance table for every relic.
- Persist relic reward chance per refinement: rejected because the value can be
  derived from `RewardTier` and selected refinement.
- Use source rarity labels for relic tiers: rejected because the source labels
  do not reliably identify Common, Uncommon, and Rare relic slots.

## Decision: Keep derived relic chances separate from mission reward chances

**Rationale**: A derived relic chance answers "what can this relic open into at
this refinement?" while a mission reward percentage answers "where can this
relic drop?" Combining them too early would hide the difference between relic
refinement odds and mission source odds.

**Alternatives considered**:
- Single generic reward table for every probability row: rejected because it
  complicates the two primary query paths and invites accidental chance mixing.
- Precomputed acquisition score table: deferred until mission duration and
  ranking rules exist.

## Decision: Store full display labels beside parsed fields

**Rationale**: Mission labels encode region, node, mission type, event
qualifiers, extra pools, caches, and special variants inconsistently. Keeping
the full label preserves source fidelity while parsed columns support search
and filtering.

**Alternatives considered**:
- Fully normalize every mission label immediately: rejected because edge cases
  can be resolved incrementally without blocking PI1.
- Use only raw display labels: rejected because filtering by mission type,
  region, and node would be awkward.
