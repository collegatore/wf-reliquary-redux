# Implementation Plan: Application Layer Boilerplate

**Branch**: `002-app-layer-boilerplate` | **Date**: 2026-04-18 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/002-app-layer-boilerplate/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. It must
reflect the project constitution before implementation starts.

## Summary

Create the first runnable boilerplate for WF Reliquary Redux: a solution with
separated presentation, application, infrastructure, unit test, and integration
test projects. The implementation will prove the approved stack can restore,
build, and test from command-line workflows on Windows and Linux while keeping
domain features limited to placeholders. README guidance will document the
layer layout and validation commands so future drop-table parsing, persistence,
query, and UI features have a stable home.

## Technical Context

**Language/Version**: C# / .NET 10 (`net10.0`)  
**Primary Dependencies**: Avalonia 12, Microsoft.Data.Sqlite, NUnit 4, NUnit3TestAdapter, Microsoft.NET.Test.Sdk  
**Storage**: SQLite dependency placeholder only; no runtime schema or database file in this feature  
**Testing**: NUnit unit tests for application-layer placeholder behavior; NUnit integration tests for solution/infrastructure placeholder wiring  
**Target Platform**: Windows and Linux desktop  
**Project Type**: Cross-platform desktop application  
**Constraints**: Preserve layer separation; keep VS Code/.NET CLI workflows viable on Windows and Linux; avoid domain behavior beyond structural placeholders; update README for restore/build/test and project layout guidance

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- `Layer Separation`: PASS. Presentation, application, infrastructure, unit
  tests, and integration tests will be separate projects with dependency
  direction documented and validated. Application must not reference
  presentation or infrastructure implementation projects.
- `Unit Testing`: PASS. The feature introduces placeholder application logic
  solely to prove test wiring, and that behavior will have NUnit coverage.
- `Cross-Platform`: PASS. Validation uses `dotnet restore`, `dotnet build`,
  and `dotnet test`, which are standard .NET CLI workflows for Windows and
  Linux.
- `Technology Stack`: PASS. The plan uses the approved stack from the
  constitution and README: C#/.NET, Avalonia, SQLite via Microsoft.Data.Sqlite,
  and NUnit. No stack deviations are proposed.
- `Documentation`: PASS. README updates are in scope because this feature
  changes contributor setup, validation, and project navigation.

## Project Structure

### Documentation (this feature)

```text
specs/002-app-layer-boilerplate/
|-- plan.md
|-- research.md
|-- data-model.md
|-- quickstart.md
|-- contracts/
|   `-- boilerplate-validation-contract.md
`-- tasks.md
```

### Source Code (repository root)

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

WF.Reliquary.Redux.sln
README.md
```

**Structure Decision**: Use a solution-level `.sln` at the repository root and
place source projects under `src/<Layer>/` with matching test projects under
`tests/<TestType>/`. Presentation may reference Application. Infrastructure may
reference Application to implement future application-defined abstractions.
Application references neither Presentation nor Infrastructure. Test projects
may reference the projects they validate.

## Phase 0 Research

See [research.md](research.md). Technical choices are resolved for .NET 10,
Avalonia 12, Microsoft.Data.Sqlite, NUnit 4, solution/project layout, dependency
direction, and README validation guidance.

## Phase 1 Design

See [data-model.md](data-model.md) for the boilerplate structure entities,
relationships, validation rules, and lifecycle.

Contracts:

- [boilerplate-validation-contract.md](contracts/boilerplate-validation-contract.md):
  defines the required project layout, dependency rules, validation commands,
  README content, and out-of-scope domain behavior.

See [quickstart.md](quickstart.md) for the expected contributor validation flow.

## Post-Design Constitution Check

- `Layer Separation`: PASS. The data model and contract explicitly enforce
  separate projects and the application dependency rule.
- `Unit Testing`: PASS. The contract requires at least one NUnit unit test for
  application-layer placeholder behavior and an integration placeholder
  validation space.
- `Cross-Platform`: PASS. Quickstart commands are .NET CLI commands intended
  for PowerShell and Linux shells.
- `Technology Stack`: PASS. Planned dependencies match the constitution and
  README. Microsoft.Data.Sqlite is included as a future persistence placeholder,
  not as a runtime schema implementation.
- `Documentation`: PASS. README changes are required and defined by contract.

## Risk Tracking

| Risk | Why It Matters | Mitigation |
|------|----------------|------------|
| Boilerplate grows into domain implementation | This feature should create structure, not parse drop tables or implement product workflows | Add explicit out-of-scope contract checks and placeholder-only acceptance criteria |
| Application layer references infrastructure or UI | This would violate constitution layer separation from the start | Define dependency direction in the contract and validate project references during implementation |
| Avalonia setup dominates the boilerplate | Too much UI work could obscure the application-layer foundation | Keep presentation to the minimum runnable or buildable placeholder needed to prove the project exists |
| Tests validate only project existence | Later contributors need a pattern for core behavior tests | Include at least one application-layer placeholder behavior with NUnit coverage |
| README becomes stale immediately | Contributors need a reliable entry point after boilerplate lands | Update README in the same feature and include commands verified by quickstart |
