# Contract: Boilerplate Validation

## Purpose

Define the observable outcomes required for the application-layer boilerplate.
This contract is used by planning, task generation, and implementation
validation.

## Required Project Layout

| Area | Required Path |
|------|---------------|
| Solution | `WF.Reliquary.Redux.sln` |
| Presentation | `src/Presentation/WF.Reliquary.Presentation/` |
| Application | `src/Application/WF.Reliquary.Application/` |
| Infrastructure | `src/Infrastructure/WF.Reliquary.Infrastructure/` |
| Unit tests | `tests/Unit/WF.Reliquary.UnitTests/` |
| Integration tests | `tests/Integration/WF.Reliquary.IntegrationTests/` |

## Dependency Rules

- Presentation may reference Application.
- Infrastructure may reference Application.
- Unit tests may reference Application.
- Integration tests may reference Application and Infrastructure.
- Application must not reference Presentation.
- Application must not reference Infrastructure.
- Presentation must not reference Infrastructure directly in this boilerplate.

## Validation Commands

The following commands must be documented and expected to pass from the
repository root:

```powershell
dotnet restore
dotnet build
dotnet test
```

## Required Placeholder Behavior

- Application must expose at least one minimal behavior that can be tested by
  the unit test project.
- The placeholder must prove test wiring without implementing drop-table
  parsing, item lookup, mission recommendation, or storage schema behavior.

## README Requirements

README must include:

- Required SDK/runtime guidance.
- Restore/build command.
- Test command.
- Short layer layout explanation.
- Note that domain features such as drop-table parsing and item lookup are
  future work.

## Out of Scope

- Warframe drop-table parsing.
- SQLite schema or runtime database creation.
- Item lookup workflows.
- Mission recommendation workflows.
- Full product UI beyond a minimal presentation placeholder.
