## Context

The Infrastructure project currently manages SQLite schema entirely by hand: DDL is embedded as raw C# string literals, and schema evolution is detected at runtime via PRAGMA introspection. Two separate files (`SqliteDropTableImportStore` and `SqliteWishlistStore`) each carry their own near-identical ADO.NET helper methods. Every schema change requires a new PRAGMA detection method (~20–30 lines each). The project has no released version, so backward compatibility with old database shapes is not required.

The Application layer has a clean ports-and-adapters boundary — port interfaces and domain records (`WishlistEntry`, `PrimeGearEntry`, etc.) are unaware of the storage implementation.

## Goals / Non-Goals

**Goals:**
- Replace all manual schema management with EF Core code-first migrations
- Eliminate duplicate ADO.NET boilerplate across Infrastructure files
- Move complex SQL queries to embedded `.sql` resource files for readability
- Keep the Application layer entirely free of EF knowledge

**Non-Goals:**
- Changing any user-visible behavior
- Switching to a different database engine
- Splitting into multiple DbContexts
- Using EF navigation-based LINQ for complex read queries (kept as raw SQL by design)
- Migrating existing user databases (fresh import required after this change)

## Decisions

### Single `ReliquaryDbContext`

One context over all 11 tables. Alternatives considered:
- **Split into `SnapshotDbContext` + `UserDbContext`**: mirrors the snapshot/user-data lifecycle split, but cross-boundary queries (wishlisted relics join snapshot tables to wishlist tables) would require two connections or leaky abstractions. The single-file SQLite setup makes split contexts actively awkward.

### EF entities are internal to Infrastructure

EF entity classes live in the Infrastructure project and are not exposed to Application. Port implementations map between EF entities and Application domain records. This keeps EF invisible to the rest of the codebase.

Alternatives considered:
- **Share entities with Application**: would couple Application to EF, violating the ports-and-adapters boundary.

### `wishlist_items → reward_items` is a soft reference — no FK constraint

`wishlist_items.part_normalized_name` references `reward_items.normalized_name` semantically, but no FK constraint is defined. Rationale: the import cycle clears and rebuilds `reward_items` on every run. A hard FK (with any ON DELETE behavior) would either destroy wishlist data or break the import. The "ghost" detection feature — flagging wishlist items whose part no longer exists in the current drop table — depends explicitly on wishlist items surviving import cycles unconstrained.

EF entity configuration: `WishlistItemEntity.PartNormalizedName` is a plain `string` property with no navigation property to `RewardItemEntity`. The join is handled in the embedded `.sql` query files.

### Bulk import pattern

`ExecuteDeleteAsync` (EF Core 7 set-based delete, no entity loading) for clearing snapshot tables, followed by `AddRange` + `SaveChangesAsync` for inserts, followed by `ExecuteSqlRaw` for the two computed post-insert steps.

Alternatives considered:
- **Raw ADO.NET for the insert step**: faster for very large datasets but adds back the boilerplate being removed. Import sizes for this app are small enough that `AddRange`/`SaveChanges` is fine.
- **`ExecuteSqlRaw` bulk inserts**: would preserve performance but lose the benefit of EF entity mapping.

### Embedded `.sql` resource files for complex queries

Four queries remain as raw SQL and are stored as embedded resources rather than inline string literals:

| File | Used by |
|------|---------|
| `GetWishlistRelics.sql` | `SqliteWishlistRelicsReader` — CTE + 4-way JOIN + LEFT JOIN |
| `GetPrimeGear.sql` | `SqlitePrimeGearReader` — JOIN with SQLite string functions |
| `InsertGearVaultStatus.sql` | Post-import step — CTE INSERT |
| `UpdateRelicVaultStatus.sql` | Post-import step — correlated UPDATE |

A small `SqlQueryLoader` helper reads these from `Assembly.GetManifestResourceStream` at startup.

Simple CRUD operations (wishlist management, preferences) are handled directly by EF and need no SQL files.

### Delete order for `ExecuteDeleteAsync`

Snapshot tables must be cleared in FK-dependency order (children before parents) to avoid constraint violations:

```
mission_rewards → reward_rotations → mission_sources
                                   → relic_rewards  → relics
                                                    → reward_items
gear_vault_status
import_metadata
```

## Risks / Trade-offs

- **EF `SaveChanges` performance on large imports** → Acceptable at current data volume; can switch to raw SQL batch insert later if profiling shows a bottleneck.
- **`ExecuteDeleteAsync` requires correct delete ordering** → Mitigation: ordering is explicit in the import service implementation and covered by an integration test.
- **`SqlQueryLoader` adds file-loading plumbing** → Small surface area (~20 lines); failure mode is a clear `InvalidOperationException` on startup if a `.sql` file is missing from the assembly.

## Migration Plan

1. Add NuGet packages (`Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Design`) to the Infrastructure project
2. Install `dotnet-ef` global tool
3. Create entity classes and `ReliquaryDbContext` in Infrastructure
4. Run `dotnet ef migrations add InitialSchema` to generate the first migration from the entity model
5. Replace `SqliteDropTableImportStore.CreateSchemaAsync` call with `context.Database.MigrateAsync()`
6. Remove all PRAGMA introspection methods
7. Rewrite port implementations using EF
8. Add `.sql` resource files and `SqlQueryLoader`
9. Update DI registration in Composition to provide `ReliquaryDbContext`

No rollback strategy — existing databases are not migrated; a fresh drop table import after deployment is the expected path.
