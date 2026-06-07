# Technical Reference

Single source of truth for stack decisions, project layout, commands, architecture, database design, testing, and code conventions. Referenced by both [README.md](../README.md) and [CLAUDE.md](../CLAUDE.md).

## Tech Stack

| Concern | Choice |
|---|---|
| Language / Runtime | C# 13, .NET 10 SDK |
| UI | Avalonia 12 (AXAML + MVVM) |
| Database | SQLite via EF Core 9 |
| HTML parsing | HtmlAgilityPack |
| Testing | NUnit 4.5 + Moq |
| IDE | Visual Studio Code |
| Target OS | Windows and Linux |

## Commands

```bash
dotnet restore
dotnet build
dotnet test
dotnet test --logger "console;verbosity=detailed"          # verbose output
dotnet test --filter "FullyQualifiedName~<TestClassName>"  # single test class
dotnet run --project src/Presentation/WF.Reliquary.Presentation/WF.Reliquary.Presentation.csproj
```

VS Code workspace tasks (`Ctrl+Shift+B`): `dotnet: restore`, `dotnet: build`, `dotnet: test`, `dotnet: run presentation`. Launch configurations: `.NET: Debug Presentation` (with debugger), `.NET: Launch Presentation` (without).

## Project Layout

```
WF.Reliquary.Redux.sln
src/
  Application/   — domain models, port interfaces, workflow orchestration
  Infrastructure/ — EF Core/SQLite, HttpClient, HTML parser, file storage
  Composition/   — ReliquaryServiceFactory: manual wiring of app ↔ infra
  Presentation/  — Avalonia ViewModels + AXAML views
tests/
  Unit/          — NUnit, Application layer only, Moq for all ports
  Integration/   — NUnit, real SQLite stack, ViewModel wiring, boundary checks
```

## Architecture

Strict four-layer architecture. Dependency direction is enforced by `ProjectReferenceBoundaryTests`:

```
Presentation  →  Application  ←  Infrastructure
     ↓                ↑               ↑
 Composition  ────────┴───────────────┘
```

- **Application** — zero external dependencies; defines all port interfaces (`IWishlistStore`, `IDropTableParser`, etc.) and orchestrates workflows.
- **Infrastructure** — implements every Application port; owns EF Core DbContext, HTTP client, HTML parser, and local file I/O.
- **Composition** — `ReliquaryServiceFactory` is the only place that references both Application and Infrastructure; no DI container.
- **Presentation** — calls Composition to obtain services; never references Infrastructure directly.

**Extending the app:** add an interface in Application → implement it in Infrastructure → wire it in `ReliquaryServiceFactory` → consume it via the interface in Presentation.

## Database

**Location:** `%LOCALAPPDATA%\WF Reliquary Redux\DropTables\drop-table-import.sqlite`

**Migrations:** `src/Infrastructure/Storage/Migrations/`. There is currently one migration (`20260605172101_InitialSchema`); see the pre-V1 persistence rule below.

**Queries:** complex joins (prime gear, wishlist relics, farming missions) use embedded `.sql` files in `src/Infrastructure/Storage/Queries/`, loaded by `SqlQueryLoader`. Prefer embedded SQL over LINQ when it is stylistically or functionally clearer.

**Soft references:** `wishlist_items` stores `part_normalized_name` as a plain string with no FK to `reward_items`, so wishlist entries survive drop-table re-imports.

## Pre-V1 Persistence Rule

Applies to **all persistent state**: database data, settings files, config files, and any other stored format. 

Evolve formats in place rather than writing migration or upgrade code.
Notify the developer when breaking change happens and manual cleanup or re-init is expected.
For the EF Core schema, EF migrations are exempt from this rule. All schema and format changes must be squashed into a single clean init before V1. 
**This rule lifts at V1.**

## Testing

- **Unit tests** — cover Application-layer logic only. One test class per service class. All ports are mocked with Moq.
- **Integration tests** — exercise the real SQLite stack end-to-end, ViewModel wiring, and project structure. `ReadmeGuidanceTests` and `VsCodeWorkflowTests` enforce documentation and build-tooling compliance; keep README and VS Code tasks consistent with them.

## Code Conventions

- All I/O methods are `async` and accept `CancellationToken`.
- Nullable reference types are enabled project-wide; do not use `!` without a comment explaining why the null is impossible.
- Immutable domain models use `record` types (`PrimeGearEntry`, `WishlistEntry`, etc.).
- Pre-V1: no backwards-compatibility shims, data-migration helpers, or feature flags.
