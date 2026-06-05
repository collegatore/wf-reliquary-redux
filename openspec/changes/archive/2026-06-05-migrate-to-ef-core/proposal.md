## Why

The data layer has accumulated manual schema management — DDL strings, PRAGMA introspection methods, and duplicate ADO.NET helper boilerplate — that will only grow as new features are added. The schema is still early and actively evolving, making this the right moment to replace hand-rolled migration detection with EF Core code-first migrations.

## What Changes

- **Replace `SqliteDropTableImportStore.CreateSchemaAsync`** and all PRAGMA introspection methods (`RewardItemsTableContainsObsoletePrimePartColumnAsync`, `MissionSourcesTableHasUniqueDisplayLabelAsync`, `RelicsTableMissingIsVaultedColumnAsync`, `EnsureIsVaultedColumnAsync`) with EF Core migrations
- **Introduce `ReliquaryDbContext`** in Infrastructure with entity classes mapping all 11 tables; entities are internal to Infrastructure
- **Replace duplicate ADO.NET helpers** (`ExecuteNonQueryAsync`/`NonQueryAsync`, `InsertAndReturnIdAsync`, `ScalarAsync`) with EF Core query methods
- **Extract complex SQL queries to embedded `.sql` resource files**: `GetWishlistRelics.sql`, `GetPrimeGear.sql`, `InsertGearVaultStatus.sql`, `UpdateRelicVaultStatus.sql`
- **Retain raw SQL** for the four queries above via `context.Database.ExecuteSqlRaw` / `Database.SqlQuery<T>` — these are untranslatable to LINQ or cleaner as SQL
- **Bulk import pattern**: `ExecuteDeleteAsync` on snapshot tables (EF Core 7+), `AddRange` + `SaveChangesAsync` for inserts, `ExecuteSqlRaw` for post-insert computed steps
- **Remove all old PRAGMA migration hacks** — the project has no released version, so backward compatibility with old database shapes is not required

## Capabilities

### New Capabilities

- `ef-core-data-layer`: EF Core DbContext, internal entity model, code-first migration history, and embedded `.sql` resource files for complex read queries

### Modified Capabilities

<!-- No existing spec-level requirements change. All user-visible behavior is preserved. -->

## Impact

- **Infrastructure project**: major rewrite of all `Sqlite*.cs` files; new `ReliquaryDbContext`, entity classes, and `Migrations/` folder
- **New NuGet dependencies**: `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Design`
- **New dev tool**: `dotnet-ef` CLI (global tool)
- **Application project**: no changes — port interfaces and domain records are unchanged
- **Presentation/Composition projects**: no changes — DI wiring may need `DbContext` registration added in Composition
- **Database file**: existing databases will not be migrated; a fresh import is required after this change (acceptable — no released version)
