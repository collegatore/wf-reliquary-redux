# Tasks: Drop Table Schema Preparation

**Input**: Design documents from `/specs/001-drop-table-schema/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: No code tests are produced by this feature. Tasks validate design artifacts and document future test coverage only.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (for example `US1`)
- Include exact file paths in descriptions

## Phase 1: Setup

- [x] T001 Confirm active feature paths from `.specify/feature.json` and `specs/001-drop-table-schema/plan.md`
- [x] T002 [P] Confirm design-only scope in `specs/001-drop-table-schema/spec.md`
- [x] T003 [P] Confirm required artifact folders exist at `specs/001-drop-table-schema/artifacts/` and `specs/001-drop-table-schema/contracts/`
- [x] T004 Confirm task execution will not create or modify application source code, test code, project files, or runtime database files outside `specs/001-drop-table-schema/`

## Phase 2: Foundational

- [x] T005 [P] Review scope and success criteria in `specs/001-drop-table-schema/spec.md`
- [x] T006 [P] Review design-only constitution checks in `specs/001-drop-table-schema/plan.md`
- [x] T007 [P] Review technology and modeling decisions in `specs/001-drop-table-schema/research.md`
- [x] T008 [P] Review model entities and fixed reward-tier chance rules in `specs/001-drop-table-schema/data-model.md`
- [x] T009 [P] Review parser and query boundaries in `specs/001-drop-table-schema/contracts/parser-contract.md` and `specs/001-drop-table-schema/contracts/query-contract.md`

## Phase 3: User Story 1 - Preserve Official Drop Table Sample (P1)

**Goal**: The feature directory contains the downloaded official sample and enough provenance for later review.

**Independent Test**: A reviewer can locate the raw HTML sample, source URL, retrieval date, and source last-update date without using application code.

- [x] T010 [US1] Verify raw sample file exists at `specs/001-drop-table-schema/artifacts/warframe-drop-tables-sample.html`
- [x] T011 [US1] Verify source URL and retrieval date are recorded in `specs/001-drop-table-schema/artifacts/drop-table-schema-analysis.md`
- [x] T012 [US1] Verify source last-update date `08 April, 2026` is recorded in `specs/001-drop-table-schema/artifacts/drop-table-schema-analysis.md`
- [x] T013 [US1] Verify sample validation commands are documented without codebase prerequisites in `specs/001-drop-table-schema/quickstart.md`

## Phase 4: User Story 2 - Understand Relic and Mission Reward Relationships (P1)

**Goal**: The analysis artifact explains how Prime Parts, base relics, reward tiers, mission rewards, rotations, and mission-source chances relate.

**Independent Test**: A reviewer can explain the Prime Part to Relic to Mission Source acquisition path from the analysis artifact alone.

- [x] T014 [P] [US2] Verify Mission Rewards section shape and representative mission rows in `specs/001-drop-table-schema/artifacts/drop-table-schema-analysis.md`
- [x] T015 [P] [US2] Verify Relics section shape and representative relic rows in `specs/001-drop-table-schema/artifacts/drop-table-schema-analysis.md`
- [x] T016 [US2] Verify Intact chance to reward-tier derivation is documented in `specs/001-drop-table-schema/artifacts/drop-table-schema-analysis.md`
- [x] T017 [US2] Verify fixed refinement chance table is documented in `specs/001-drop-table-schema/artifacts/drop-table-schema-analysis.md`
- [x] T018 [US2] Verify mission chance and derived relic chance are explicitly kept separate in `specs/001-drop-table-schema/artifacts/drop-table-schema-analysis.md`

## Phase 5: User Story 3 - Define Query-Ready Storage Model (P2)

**Goal**: The design documents define the future persistent model and parser/query contracts without creating code.

**Independent Test**: A reviewer can map each proposed persistent entity and contract output to the Prime Part to Relic and Relic to Mission Source query paths.

- [x] T019 [P] [US3] Verify Source Sample is marked non-persistent in `specs/001-drop-table-schema/data-model.md`
- [x] T020 [P] [US3] Verify `RelicReward` links directly to `RelicId` and stores `RewardTier` in `specs/001-drop-table-schema/data-model.md`
- [x] T021 [P] [US3] Verify `RelicVariant`, `DropTableSnapshot`, `SnapshotId`, and `snapshotId` are absent from persistent model docs in `specs/001-drop-table-schema/data-model.md`
- [x] T022 [P] [US3] Verify parser output excludes persistent refinement records in `specs/001-drop-table-schema/contracts/parser-contract.md`
- [x] T023 [P] [US3] Verify query output returns base relics, reward tiers, and derived refinement-specific chances in `specs/001-drop-table-schema/contracts/query-contract.md`
- [x] T024 [US3] Verify future NUnit coverage is described as future implementation guidance only in `specs/001-drop-table-schema/quickstart.md`

## Phase 6: Polish & Verification

- [x] T025 Validate all required design artifacts listed in `specs/001-drop-table-schema/quickstart.md` exist
- [x] T026 Validate `specs/001-drop-table-schema/spec.md`, `specs/001-drop-table-schema/plan.md`, and `specs/001-drop-table-schema/data-model.md` consistently state design-only scope
- [x] T027 Validate no task in `specs/001-drop-table-schema/tasks.md` asks for application source code, test code, project files, or runtime database files
- [x] T028 Validate no stale persistent schema terms remain in `specs/001-drop-table-schema/spec.md`, `specs/001-drop-table-schema/data-model.md`, `specs/001-drop-table-schema/contracts/parser-contract.md`, or `specs/001-drop-table-schema/contracts/query-contract.md`
- [x] T029 Validate `AGENTS.md` points to `specs/001-drop-table-schema/plan.md`

## Dependencies

- Phase 1 must complete before Phase 2.
- Phase 2 must complete before user-story validation phases.
- US1 and US2 can proceed in parallel after Phase 2 because they operate on different review concerns in the artifacts.
- US3 depends on Phase 2 and benefits from US2 terminology review, but can start in parallel once the reward-tier rules are understood.
- Phase 6 depends on US1, US2, and US3 completion.

## Parallel Execution Examples

US1:

```text
T011 and T012 can run in parallel after T010 because both inspect `specs/001-drop-table-schema/artifacts/drop-table-schema-analysis.md` for different provenance fields.
```

US2:

```text
T014 and T015 can run in parallel because one reviews Mission Rewards and the other reviews Relics in `specs/001-drop-table-schema/artifacts/drop-table-schema-analysis.md`.
```

US3:

```text
T019, T020, T022, and T023 can run in parallel because they review distinct design sections and contract files under `specs/001-drop-table-schema/`.
```

## Implementation Strategy

### MVP First

Complete Phase 1, Phase 2, and US1 to prove the official sample and provenance are preserved as design artifacts.

### Incremental Delivery

1. Complete US1 to validate the preserved source sample.
2. Complete US2 to validate source-shape analysis and reward-tier derivation.
3. Complete US3 to validate the future persistent model and parser/query contracts.
4. Complete Phase 6 to confirm the feature remains design-only and ready for downstream analysis.

### Scope Guard

This task list is complete when the design documents and preserved artifacts are validated. Do not create source projects, application code, test code, package references, or runtime database files for this feature.
