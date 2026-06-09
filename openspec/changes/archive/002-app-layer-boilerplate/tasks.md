# Tasks: Application Layer Boilerplate

**Input**: Design documents from `/specs/002-app-layer-boilerplate/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: NUnit test tasks are included because the specification requires unit coverage for application-layer placeholder behavior and an integration test space for boilerplate wiring.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel after prerequisite setup because it touches a different file or project boundary.
- **[Story]**: User story label for story-scoped tasks only.
- Every task includes an exact repository path.

## Phase 1: Setup

**Purpose**: Create the solution and physical project structure required by the boilerplate.

- [X] T001 Create the root solution file in `WF.Reliquary.Redux.sln`
- [X] T002 Create source and test layer folders in `src/Presentation/`, `src/Application/`, `src/Infrastructure/`, `tests/Unit/`, and `tests/Integration/`
- [X] T003 Create the application class library project in `src/Application/WF.Reliquary.Application/WF.Reliquary.Application.csproj`
- [X] T004 Create the infrastructure class library project in `src/Infrastructure/WF.Reliquary.Infrastructure/WF.Reliquary.Infrastructure.csproj`
- [X] T005 Create the Avalonia presentation project in `src/Presentation/WF.Reliquary.Presentation/WF.Reliquary.Presentation.csproj`
- [X] T006 Create the NUnit unit test project in `tests/Unit/WF.Reliquary.UnitTests/WF.Reliquary.UnitTests.csproj`
- [X] T007 Create the NUnit integration test project in `tests/Integration/WF.Reliquary.IntegrationTests/WF.Reliquary.IntegrationTests.csproj`

## Phase 2: Foundational

**Purpose**: Establish dependencies and shared conventions that block all user stories.

**CRITICAL**: Complete this phase before starting any user story implementation.

- [X] T008 Add all source and test projects to `WF.Reliquary.Redux.sln`
- [X] T009 Configure project references so `src/Presentation/WF.Reliquary.Presentation/WF.Reliquary.Presentation.csproj` references `src/Application/WF.Reliquary.Application/WF.Reliquary.Application.csproj`
- [X] T010 Configure project references so `src/Infrastructure/WF.Reliquary.Infrastructure/WF.Reliquary.Infrastructure.csproj` references `src/Application/WF.Reliquary.Application/WF.Reliquary.Application.csproj`
- [X] T011 Configure project references so `tests/Unit/WF.Reliquary.UnitTests/WF.Reliquary.UnitTests.csproj` references `src/Application/WF.Reliquary.Application/WF.Reliquary.Application.csproj`
- [X] T012 Configure project references so `tests/Integration/WF.Reliquary.IntegrationTests/WF.Reliquary.IntegrationTests.csproj` references `src/Application/WF.Reliquary.Application/WF.Reliquary.Application.csproj` and `src/Infrastructure/WF.Reliquary.Infrastructure/WF.Reliquary.Infrastructure.csproj`
- [X] T013 Add approved dependencies and `net10.0` target configuration to `src/Presentation/WF.Reliquary.Presentation/WF.Reliquary.Presentation.csproj`, `src/Infrastructure/WF.Reliquary.Infrastructure/WF.Reliquary.Infrastructure.csproj`, `tests/Unit/WF.Reliquary.UnitTests/WF.Reliquary.UnitTests.csproj`, and `tests/Integration/WF.Reliquary.IntegrationTests/WF.Reliquary.IntegrationTests.csproj`
- [X] T014 Remove template sample classes or unused generated files from `src/Application/WF.Reliquary.Application/`, `src/Infrastructure/WF.Reliquary.Infrastructure/`, `tests/Unit/WF.Reliquary.UnitTests/`, and `tests/Integration/WF.Reliquary.IntegrationTests/`

## Phase 3: User Story 1 - Open and Validate the Boilerplate (Priority: P1)

**Goal**: A contributor can restore dependencies, build the solution, and run the initial test suite from documented command-line workflows.

**Independent Test**: From a clean checkout with the required SDK, run the documented restore, build, and test commands and confirm they succeed.

### Tests for User Story 1

- [X] T015 [P] [US1] Add an NUnit test for application placeholder metadata in `tests/Unit/WF.Reliquary.UnitTests/ApplicationInfoTests.cs`
- [X] T016 [P] [US1] Add an NUnit integration smoke test for boilerplate project availability in `tests/Integration/WF.Reliquary.IntegrationTests/BoilerplateSmokeTests.cs`

### Implementation for User Story 1

- [X] T017 [US1] Add minimal application placeholder metadata behavior in `src/Application/WF.Reliquary.Application/ApplicationInfo.cs`
- [X] T018 [US1] Add a minimal presentation entry point that builds without product workflows in `src/Presentation/WF.Reliquary.Presentation/Program.cs`
- [X] T019 [US1] Add minimal Avalonia application shell files in `src/Presentation/WF.Reliquary.Presentation/App.axaml`, `src/Presentation/WF.Reliquary.Presentation/App.axaml.cs`, `src/Presentation/WF.Reliquary.Presentation/MainWindow.axaml`, and `src/Presentation/WF.Reliquary.Presentation/MainWindow.axaml.cs`
- [X] T020 [US1] Verify the quickstart restore, build, and test commands from `specs/002-app-layer-boilerplate/quickstart.md`

## Phase 4: User Story 2 - Preserve Application Layer Boundaries (Priority: P1)

**Goal**: Presentation, application, infrastructure, and test responsibilities are visibly separated and project references preserve dependency direction.

**Independent Test**: Inspect project files and integration tests to confirm application has no project reference to presentation or infrastructure and presentation does not reference infrastructure directly.

### Tests for User Story 2

- [X] T021 [P] [US2] Add dependency direction validation tests in `tests/Integration/WF.Reliquary.IntegrationTests/ProjectReferenceBoundaryTests.cs`
- [X] T022 [P] [US2] Add infrastructure placeholder wiring tests in `tests/Integration/WF.Reliquary.IntegrationTests/InfrastructurePlaceholderTests.cs`

### Implementation for User Story 2

- [X] T023 [US2] Add application-owned placeholder contract in `src/Application/WF.Reliquary.Application/Ports/IStorageHealthCheck.cs`
- [X] T024 [US2] Add infrastructure SQLite dependency placeholder implementing the application contract in `src/Infrastructure/WF.Reliquary.Infrastructure/Storage/SqliteStorageHealthCheck.cs`
- [X] T025 [US2] Ensure `src/Application/WF.Reliquary.Application/WF.Reliquary.Application.csproj` contains no project references to presentation or infrastructure projects
- [X] T026 [US2] Ensure `src/Presentation/WF.Reliquary.Presentation/WF.Reliquary.Presentation.csproj` does not reference `src/Infrastructure/WF.Reliquary.Infrastructure/WF.Reliquary.Infrastructure.csproj`

## Phase 5: User Story 3 - Provide Contributor Guidance for Future Features (Priority: P2)

**Goal**: README guidance and placeholder tests make the layer layout, validation commands, and future extension points clear to maintainers.

**Independent Test**: Review README and placeholder tests to confirm they document setup, validation, layer responsibilities, and future feature boundaries.

### Tests for User Story 3

- [X] T027 [P] [US3] Add README validation checks for required command and layer guidance in `tests/Integration/WF.Reliquary.IntegrationTests/ReadmeGuidanceTests.cs`

### Implementation for User Story 3

- [X] T028 [US3] Update prerequisite and validation command guidance in `README.md`
- [X] T029 [US3] Update layer layout and project responsibility guidance in `README.md`
- [X] T030 [US3] Update future-work scope notes for parsing, SQLite schema, item lookup, mission recommendation, and UI workflows in `README.md`

## Phase 6: Polish & Cross-Cutting Verification

**Purpose**: Confirm the boilerplate satisfies the contract without accidental domain implementation.

- [X] T031 Run `dotnet restore` from the repository root and resolve any restore issues in `WF.Reliquary.Redux.sln`
- [X] T032 Run `dotnet build` from the repository root and resolve any build issues in `WF.Reliquary.Redux.sln`
- [X] T033 Run `dotnet test` from the repository root and resolve any test issues in `tests/Unit/WF.Reliquary.UnitTests/` and `tests/Integration/WF.Reliquary.IntegrationTests/`
- [X] T034 Review project references against `specs/002-app-layer-boilerplate/contracts/boilerplate-validation-contract.md`
- [X] T035 Confirm no drop-table parsing, SQLite schema creation, item lookup, mission recommendation, or full product UI workflow was added under `src/`

## Dependencies

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies; creates the project containers.
- **Foundational (Phase 2)**: Depends on Setup; establishes project membership, references, dependencies, and cleanup.
- **User Story 1 (Phase 3)**: Depends on Foundational; delivers the MVP restore/build/test increment.
- **User Story 2 (Phase 4)**: Depends on Foundational; may run after or alongside User Story 1 once shared projects exist.
- **User Story 3 (Phase 5)**: Depends on Foundational; should be finalized after US1 and US2 so README matches the actual boilerplate.
- **Polish (Phase 6)**: Depends on all selected user stories.

### User Story Dependencies

- **US1**: Can begin after Phase 2 and is the suggested MVP.
- **US2**: Can begin after Phase 2; validates layer boundaries and references.
- **US3**: Can begin after Phase 2, but final README wording depends on the implemented layout from US1 and US2.

## Parallel Execution Examples

### User Story 1

```text
Task: T015 [P] [US1] Add unit test in tests/Unit/WF.Reliquary.UnitTests/ApplicationInfoTests.cs
Task: T016 [P] [US1] Add smoke test in tests/Integration/WF.Reliquary.IntegrationTests/BoilerplateSmokeTests.cs
```

### User Story 2

```text
Task: T021 [P] [US2] Add boundary tests in tests/Integration/WF.Reliquary.IntegrationTests/ProjectReferenceBoundaryTests.cs
Task: T022 [P] [US2] Add infrastructure tests in tests/Integration/WF.Reliquary.IntegrationTests/InfrastructurePlaceholderTests.cs
```

### User Story 3

```text
Task: T027 [P] [US3] Add README validation tests in tests/Integration/WF.Reliquary.IntegrationTests/ReadmeGuidanceTests.cs
Task: T028 [US3] Update prerequisite and validation command guidance in README.md
```

## Implementation Strategy

### MVP First

1. Complete Phase 1 and Phase 2.
2. Complete User Story 1.
3. Run `dotnet restore`, `dotnet build`, and `dotnet test`.
4. Stop here if only the minimum runnable boilerplate is needed.

### Incremental Delivery

1. Add User Story 2 to enforce and test layer boundaries.
2. Add User Story 3 to document contributor workflows and future extension points.
3. Finish with Phase 6 verification.

### Validation Checklist

- Restore, build, and test commands pass from the repository root.
- Application project has no presentation or infrastructure project references.
- Presentation project has no direct infrastructure project reference.
- Infrastructure may reference application and include Microsoft.Data.Sqlite as a placeholder dependency.
- README describes prerequisites, commands, layer layout, and out-of-scope domain work.
