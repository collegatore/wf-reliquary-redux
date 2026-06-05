## 1. Dependencies and Tooling

- [x] 1.1 Add `Microsoft.EntityFrameworkCore.Sqlite` and `Microsoft.EntityFrameworkCore.Design` to `WF.Reliquary.Infrastructure.csproj`
- [x] 1.2 Install `dotnet-ef` global tool (`dotnet tool install --global dotnet-ef`) and add/update `dotnet-tools.json`

## 2. EF Entity Model

- [x] 2.1 Create `ReliquaryDbContext` in `Infrastructure/Storage/` with `DbSet<T>` for all 11 tables
- [x] 2.2 Create snapshot entity classes: `RewardItemEntity`, `RelicEntity`, `RelicRewardEntity`, `MissionSourceEntity`, `RewardRotationEntity`, `MissionRewardEntity`, `GearVaultStatusEntity`, `ImportMetadataEntity`
- [x] 2.3 Create user data entity classes: `WishlistEntity`, `WishlistItemEntity`, `UserPreferencesEntity`
- [x] 2.4 Configure entity mappings in `OnModelCreating`: table names, column names, constraints (unique indexes, CHECK constraints for singleton rows), and the `gear_vault_status` primary key on `gear_name`
- [x] 2.5 Confirm `WishlistItemEntity.PartNormalizedName` has no navigation property or FK to `RewardItemEntity` (soft reference by design)

## 3. Initial Migration

- [x] 3.1 Run `dotnet ef migrations add InitialSchema --project src/Infrastructure/... --startup-project src/Presentation/...` to generate the first migration
- [x] 3.2 Review generated migration SQL — verify all tables, column types, unique indexes, and CHECK constraints match the current hand-written DDL
- [x] 3.3 Register `ReliquaryDbContext` in `WF.Reliquary.Composition` with the correct SQLite connection string and call `MigrateAsync()` at startup

## 4. Embedded SQL Query Files

- [x] 4.1 Create `Infrastructure/Storage/Queries/GetWishlistRelics.sql` with the CTE query from `SqliteWishlistRelicsReader`
- [x] 4.2 Create `Infrastructure/Storage/Queries/GetPrimeGear.sql` with the JOIN query from `SqlitePrimeGearReader`
- [x] 4.3 Create `Infrastructure/Storage/Queries/InsertGearVaultStatus.sql` with the CTE INSERT from `SqliteDropTableImportStore`
- [x] 4.4 Create `Infrastructure/Storage/Queries/UpdateRelicVaultStatus.sql` with the correlated UPDATE from `SqliteDropTableImportStore`
- [x] 4.5 Mark all four `.sql` files as `EmbeddedResource` in `WF.Reliquary.Infrastructure.csproj`
- [x] 4.6 Create `SqlQueryLoader` helper that loads a named embedded resource and throws `InvalidOperationException` if the resource is not found

## 5. Rewrite Port Implementations

- [x] 5.1 Rewrite `SqliteDropTableImportStore.ReplaceAsync`: delete snapshot tables in FK order using `ExecuteDeleteAsync`, insert entities with `AddRange`/`SaveChangesAsync`, run `InsertGearVaultStatus.sql` and `UpdateRelicVaultStatus.sql` via `ExecuteSqlRaw`
- [x] 5.2 Delete `CreateSchemaAsync`, `ClearSnapshotAsync` (replaced by EF), and all PRAGMA introspection methods (`RewardItemsTableContainsObsoletePrimePartColumnAsync`, `MissionSourcesTableHasUniqueDisplayLabelAsync`, `RelicsTableMissingIsVaultedColumnAsync`)
- [x] 5.3 Rewrite `SqliteWishlistStore` using `ReliquaryDbContext` for all CRUD operations; delete all ADO.NET helpers (`NonQueryAsync`, `ScalarAsync`, `InsertAndReturnIdAsync`, `OpenAsync`, `EnsureSchemaAsync`)
- [x] 5.4 Rewrite `SqliteWishlistRelicsReader` to load `GetWishlistRelics.sql` via `SqlQueryLoader` and execute via `context.Database.SqlQuery<T>`; delete `EnsureIsVaultedColumnAsync`
- [x] 5.5 Rewrite `SqlitePrimeGearReader` to load `GetPrimeGear.sql` via `SqlQueryLoader` and execute via `context.Database.SqlQuery<T>`

## 6. Verification

- [x] 6.1 Run all existing integration tests — `DropTableInfrastructureTests`, `DropTableImportInfrastructureTests`, `PrimeGearInfrastructureTests` — and confirm they pass
- [x] 6.2 Do a full end-to-end run: refresh drop table source, import, verify prime gear view, create a wishlist, add items, verify wishlist relics tab
- [x] 6.3 Confirm wishlists survive a second import (soft reference / ghost detection still works)
