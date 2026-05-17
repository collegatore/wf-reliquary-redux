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

VS Code users can run the checked-in workspace tasks for the same workflows:

- `dotnet: restore`: restore solution dependencies.
- `dotnet: build`: default build task for the solution.
- `dotnet: test`: default test task for the NUnit test projects.
- `dotnet: run presentation`: run the Avalonia presentation shell.

Use the `.NET: Launch Presentation` launch configuration to build and run the
current Avalonia presentation project without attaching the debugger. Use
`.NET: Debug Presentation` when you want to debug it from VS Code.

## Drop Table Source

The application can download and store the official Warframe drop-table HTML
file. The default source URL is:

```text
https://warframe-web-assets.nyc3.cdn.digitaloceanspaces.com/uploads/cms/hnfvc0o3jnfvc873njb03enrf56.html
```

Edit the source URL in the application UI to override the default for future
downloads. The configured URL is stored as application-level user settings in
the current user's local application data folder. Downloaded data is stored as a
raw HTML file under `WF Reliquary Redux/DropTables`, with adjacent metadata for
source URL, download time, and the parsed `Last Update` value. A download is
considered unsuccessful if `Last Update` cannot be parsed from the downloaded
file.

Use **Import stored file** in the application UI to parse the already downloaded
HTML file into the local SQLite import database. The import action does not
download or refresh the source file; use **Download drop tables** first when the
stored source is missing or stale. The import database is stored beside the raw
HTML under `WF Reliquary Redux/DropTables` in the current user's local
application data folder.

This workflow stops at normalized import status and stored parsed data. Item
lookup, mission recommendation, and acquisition ranking remain future feature
work.

## Project Layout

- `WF.Reliquary.Redux.sln`: solution-level restore, build, and test entry point.
- `src/Application/WF.Reliquary.Application`: core application boundary for source refresh, import orchestration, parsing abstractions, normalization, query orchestration, and business rules.
- `src/Infrastructure/WF.Reliquary.Infrastructure`: adapter boundary for file access, HTML parsing, SQLite persistence, and external data ingestion.
- `src/Composition/WF.Reliquary.Composition`: composition boundary that wires application contracts to infrastructure implementations.
- `src/Presentation/WF.Reliquary.Presentation`: Avalonia desktop UI boundary for future screens and user interaction.
- `tests/Unit/WF.Reliquary.UnitTests`: NUnit coverage for application-layer behavior.
- `tests/Integration/WF.Reliquary.IntegrationTests`: NUnit checks for cross-layer wiring, dependency direction, and contributor-facing guidance.

Dependency direction starts deliberately narrow: presentation may reference application and composition, composition may reference application and infrastructure, infrastructure may reference application, and application must not reference presentation, composition, or infrastructure.

## Future Work Scope

The current implementation can download the drop-table source and manually import
the stored file into SQLite. Item lookup, mission recommendation, acquisition
ranking, and full product UI workflows are future work and should be introduced
through separate features.

## Suggested Future Extensions

- Vaulted relics marking and filtering
- Vaulted primes marking and filtering
- Wishlist support and account-specific progress tracking.
- Mission rundowns based on multiple desired items.
- Alerts when new drop table data becomes available.
- Rotation-aware mission efficiency analysis.
- MCP
- OCR for mission rewards / inventory tracking
- mission duration statistics
- chat message generator
