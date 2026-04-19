# Quickstart: Application Layer Boilerplate

## Prerequisites

- .NET 10 SDK available from the command line.
- Repository opened at the project root in a terminal or Visual Studio Code.

## Validate The Boilerplate

From the repository root:

```powershell
dotnet restore
dotnet build
dotnet test
```

Expected result:

- Restore completes for the root solution.
- Build completes for presentation, application, infrastructure, and test
  projects.
- All initial NUnit tests pass.

## Inspect Layer Layout

Confirm these paths exist:

```powershell
Get-ChildItem src/Presentation
Get-ChildItem src/Application
Get-ChildItem src/Infrastructure
Get-ChildItem tests/Unit
Get-ChildItem tests/Integration
```

Expected structure:

- `src/Presentation/WF.Reliquary.Presentation/`
- `src/Application/WF.Reliquary.Application/`
- `src/Infrastructure/WF.Reliquary.Infrastructure/`
- `tests/Unit/WF.Reliquary.UnitTests/`
- `tests/Integration/WF.Reliquary.IntegrationTests/`

## Dependency Direction Check

Review project references:

- Application has no project references to Presentation or Infrastructure.
- Presentation may reference Application.
- Infrastructure may reference Application.
- Unit tests reference Application.
- Integration tests reference Application and Infrastructure as needed.

## Scope Check

This boilerplate is complete without:

- Drop-table parsing.
- SQLite schema or runtime database files.
- Item lookup behavior.
- Mission recommendation behavior.
- Full product UI workflows.
