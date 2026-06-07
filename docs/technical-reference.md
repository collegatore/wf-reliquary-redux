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

## EF Core Data Layer

### Why EF Core

The project started with raw ADO.NET (string DDL, manual `IDataReader` loops). Once the schema grew to 11 tables with FKs, CHECK constraints, and several multi-join queries, the cost of maintaining boilerplate (`SqliteConnection`, `SqliteCommand`, schema strings, PRAGMA introspection) outweighed the simplicity benefit. EF Core was adopted to:

- own DDL via code-first migrations (single source of truth for schema)
- eliminate redundant connection/command helpers spread across store classes
- enable set-based bulk deletes without loading entities (`ExecuteDeleteAsync`)

**Dapper note:** given that the four most important queries are kept as raw SQL anyway, Dapper would likely be a better fit for this workload — thin mapping over hand-written SQL with none of EF's change-tracking overhead. EF Core was chosen here deliberately as a learning exercise to observe how code-first migrations and the EF workflow handle themselves in a real project.

### What stays as embedded SQL

Queries that are stylistically or functionally clearer as SQL than LINQ are kept as `.sql` files in `Infrastructure/Storage/Queries/` rather than expressed in LINQ. Typical candidates are CTEs, correlated UPDATEs, and joins on computed expressions — for example `GetWishlistRelics.sql` uses a CTE with a 4-way JOIN and a LEFT JOIN that would be verbose and hard to read in LINQ.

Loaded at construction time via `SqlQueryLoader` (reads embedded resource stream). Mapped with `Database.SqlQueryRaw<T>` using named `@param` parameters and explicit PascalCase column aliases (e.g. `ri.name AS ItemName`) because EF does not auto-map snake_case columns to PascalCase properties on raw-query result types.

Raw-query DTO properties for SQLite INTEGER 0/1 columns use `int`, not `bool`, to avoid type-mapping uncertainty (`row.IsVaulted != 0` at the call site).

### Bulk import pattern

The snapshot import (replace everything on each drop-table import) does not fit EF change-tracking well. Approach:

1. **Delete** in FK-dependency order with `ExecuteDeleteAsync` — no entity loading.
2. **Insert in tiers** with a `SaveChangesAsync` between each tier so EF populates auto-generated PKs before dependent rows reference them.
3. **Post-insert computed steps** run as `ExecuteSqlRawAsync` (vault status CTE insert + correlated relic vault update).
4. All three phases execute inside a single explicit transaction.

### Soft reference

`wishlist_items.part_normalized_name` is a plain string column with **no FK** to `reward_items.normalized_name`. This is intentional: drop-table re-imports truncate `reward_items`, and wishlist entries must survive that cycle. Ghost detection (wishlisted part absent from current import) depends on the constraint being absent.

### Connection string

`SqliteConnectionStringBuilder.ForeignKeys = true` is set on every context via `ReliquaryDbContext.BuildOptions`. All FK constraints are enforced at the SQLite level, not just the EF model.

### Design-time migrations

`ReliquaryDesignTimeDbContextFactory` implements `IDesignTimeDbContextFactory<ReliquaryDbContext>` so `dotnet ef migrations add` works without a startup project. The tool is pinned as a local tool in `.config/dotnet-tools.json`; run `dotnet tool restore` before first use.

## Pre-V1 Persistence Rule

Applies to **all persistent state**: database data, settings files, config files, and any other stored format. 

Evolve formats in place rather than writing migration or upgrade code.
Notify the developer when breaking change happens and manual cleanup or re-init is expected.
For the EF Core schema, EF migrations are exempt from this rule. All schema and format changes must be squashed into a single clean init before V1. 
**This rule lifts at V1.**

## Testing

- **Unit tests** — cover Application-layer logic only. One test class per service class. All ports are mocked with Moq.
- **Integration tests** — exercise the real SQLite stack end-to-end, ViewModel wiring, and project structure. `ReadmeGuidanceTests` and `VsCodeWorkflowTests` enforce documentation and build-tooling compliance; keep README and VS Code tasks consistent with them.

## UI Design

- **Card display** — the rationale behind the relic and farming-mission cards (the ~5–10s reward-match use case, verbatim names, tier accent bars, the light-theme palette, and font-scaled card widths) is documented in [card-display-design.md](card-display-design.md).

## Code Conventions

- All I/O methods are `async` and accept `CancellationToken`.
- Nullable reference types are enabled project-wide; do not use `!` without a comment explaining why the null is impossible.
- Immutable domain models use `record` types (`PrimeGearEntry`, `WishlistEntry`, etc.).
- Pre-V1: no backwards-compatibility shims, data-migration helpers, or feature flags.
