## 1. Application Import Contracts

- [x] 1.1 Add application import result/status models for source path, source Last Update, imported-at timestamp, row counts, warnings, and failure messages under `src/Application/WF.Reliquary.Application/DropTables/`.
- [x] 1.2 Add parsed domain models for reward items, relics, relic rewards, mission sources, reward rotations, mission rewards, and import metadata.
- [x] 1.3 Add application ports for HTML parsing and parsed import persistence without referencing infrastructure, presentation, or SQLite APIs directly.
- [x] 1.4 Add an application-layer import workflow that loads the stored source file through `IDropTableSourceStore`, invokes the parser, persists successful imports, and preserves previous data on failure.

## 2. Parser Implementation

- [x] 2.1 Implement relic section traversal for source tables headed by labels such as `Lith C14 Relic (Intact)`.
- [x] 2.2 Parse base relic era/code/display names and avoid creating separate relics for each refinement level.
- [x] 2.3 Parse Intact relic reward rows into reward items, source rarity/chance values, and reward tiers derived from Intact chances.
- [x] 2.4 Implement mission section traversal for source tables headed by mission labels such as `Mercury/Apollodorus (Survival)`.
- [x] 2.5 Parse mission source display labels, source order, optional region/node/mission type/qualifier fields, and preserve the full label when structured parsing is incomplete or duplicated.
- [x] 2.6 Parse rotated and unrotated mission reward rows with source rarity labels, drop percentages, source order, and optional rotation links scoped to the parent mission source occurrence.
- [x] 2.7 Link mission reward rows to parsed base relics when reward item names match relic display names.
- [x] 2.8 Preserve non-relic mission reward rows and non-Prime relic reward rows with source context for auditability.

## 3. SQLite Import Storage

- [x] 3.1 Implement infrastructure SQLite schema creation for import metadata, reward items, relics, relic rewards, mission sources, reward rotations, and mission rewards.
- [x] 3.2 Implement atomic snapshot replacement so a successful import replaces prior parsed data in one transaction.
- [x] 3.3 Ensure failed parse or persistence operations leave the previous parsed data set available.
- [x] 3.4 Store provenance fields from the downloaded source, including source URL, downloaded timestamp, Last Update when available, source file path, and import timestamp.

## 4. Composition and Workflow Access

- [x] 4.1 Wire the import workflow in `WF.Reliquary.Composition` using application services and infrastructure parser/storage implementations.
- [x] 4.2 Keep Presentation free of direct Infrastructure references and keep Application free of Presentation, Infrastructure, and Composition references.
- [x] 4.3 Add a minimal Presentation import testing button that invokes only the composed import workflow for the already stored source file.
- [x] 4.4 Display import-in-progress state, success/failure status, source Last Update when available, imported row counts, and failure messages.
- [x] 4.5 Keep the import testing UI limited to manual validation, with no Prime Part search, acquisition-path display, mission ranking, wishlist behavior, source refresh/re-download, live refresh, or background scheduling.

## 5. Test Coverage

- [x] 5.1 Add unit tests for missing stored source, malformed source, successful import orchestration, parser warning/error propagation, and previous-data preservation.
- [x] 5.2 Add focused parser tests for relic parsing, reward-tier derivation, mission rotations, unrotated rewards, ambiguous mission labels, duplicate rows, non-relic rows, and relic linking.
- [x] 5.3 Add integration tests for SQLite schema creation, duplicate mission source labels, atomic replacement, provenance persistence, and failed replacement rollback.
- [x] 5.4 Add an integration-scale test using `specs/001-drop-table-schema/artifacts/warframe-drop-tables-sample.html` that verifies non-zero relics, Prime Part reward rows, mission sources, rotations, and relic-linked mission rewards.
- [x] 5.5 Extend project-reference boundary tests to cover the new import contracts and composition wiring.
- [x] 5.6 Add presentation or view-model coverage for the import testing action, including disabled/in-progress behavior, import-only invocation with no source refresh/re-download call, and status/error reporting where existing test infrastructure supports it.

## 6. Documentation and Validation

- [x] 6.1 Update README with the manual import testing action and any storage location contributors/users need to know.
- [x] 6.2 Review implementation scope to confirm no Prime Part search UI, acquisition-path display, mission ranking, wishlist behavior, source refresh/re-download from the import button, live network refresh, or background scheduling was added.
- [x] 6.3 Run `dotnet restore` from the repository root and resolve any restore issues.
- [x] 6.4 Run `dotnet build` from the repository root and resolve any build issues.
- [x] 6.5 Run `dotnet test` from the repository root and resolve any test failures.
- [x] 6.6 Run `openspec validate implement-downloaded-tables-parsing --type change --strict` and resolve any proposal/spec/task issues.
