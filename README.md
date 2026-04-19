# WF Reliquary Redux

## Project Description

WF Reliquary Redux is a companion app for [*Warframe*](https://www.warframe.com). Its goal is to help a player to find the best mission for a combination of items the player desires. The project is intended to process the official drop tables, extract mission and reward data, and organize that information to support item lookup, farming decisions, and activity planning. It should connect items to the missions, rotations, and reward sources where they can be obtained, and use those relationships to build acquisition paths for individual items or entire sets.

## Core Technology Decisions

- Programming language: C#
- Runtime and SDK: .NET 10 SDK
- Primary data storage: SQLite
- Development environment: Visual Studio Code
- Target operating systems: Windows and Linux
- GUI framework: Avalonia

## Development Setup

Install the .NET 10 SDK and open the repository root in a terminal or Visual Studio Code.

Validate the boilerplate with:

```powershell
dotnet restore
dotnet build
dotnet test
```

The same commands are intended to work from Windows PowerShell and Linux shells.

## Project Layout

- `WF.Reliquary.Redux.sln`: solution-level restore, build, and test entry point.
- `src/Application/WF.Reliquary.Application`: core application boundary for future parsing abstractions, normalization, query orchestration, and business rules.
- `src/Infrastructure/WF.Reliquary.Infrastructure`: adapter boundary for future file access, SQLite persistence, and external data ingestion.
- `src/Presentation/WF.Reliquary.Presentation`: Avalonia desktop UI boundary for future screens and user interaction.
- `tests/Unit/WF.Reliquary.UnitTests`: NUnit coverage for application-layer behavior.
- `tests/Integration/WF.Reliquary.IntegrationTests`: NUnit checks for cross-layer wiring, dependency direction, and contributor-facing guidance.

Dependency direction starts deliberately narrow: presentation may reference application, infrastructure may reference application, and application must not reference presentation or infrastructure.

## Future Work Scope

The current implementation is boilerplate only. Drop-table parsing, SQLite schema creation, item lookup, mission recommendation, and full product UI workflows are future work and should be introduced through separate features.

## Suggested Future Extensions

- Wishlist support and account-specific progress tracking.
- Mission rundowns based on multiple desired items.
- Alerts when new drop table data becomes available.
- Rotation-aware mission efficiency analysis.
- MCP
- OCR for mission rewards / inventory tracking
- mission duration statistics
- chat message generator
