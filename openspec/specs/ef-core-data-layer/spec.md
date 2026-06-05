## ADDED Requirements

### Requirement: Schema is managed by EF Core migrations
The system SHALL use EF Core code-first migrations as the sole mechanism for creating and evolving the SQLite database schema. No DDL strings, `CREATE TABLE IF NOT EXISTS` statements, or PRAGMA introspection SHALL exist in application code.

#### Scenario: First launch with no database
- **WHEN** the application starts and no database file exists
- **THEN** EF Core creates the database and applies all migrations before any data access occurs

#### Scenario: Launch with an outdated database
- **WHEN** the application starts and the database exists but is behind the current migration history
- **THEN** EF Core applies pending migrations before any data access occurs

#### Scenario: Launch with an up-to-date database
- **WHEN** the application starts and the database is already at the current migration version
- **THEN** no schema changes are made and startup proceeds normally

---

### Requirement: Complex SQL queries are stored as embedded resource files
The system SHALL store the following queries as `.sql` files embedded in the Infrastructure assembly, loaded at runtime via `Assembly.GetManifestResourceStream`:

- `GetWishlistRelics.sql`
- `GetPrimeGear.sql`
- `InsertGearVaultStatus.sql`
- `UpdateRelicVaultStatus.sql`

No other query complexity level requires a `.sql` file; simple CRUD is handled by EF directly.

#### Scenario: Query file is present in the assembly
- **WHEN** a reader or service loads a `.sql` resource file at startup
- **THEN** the SQL string is returned and used for subsequent queries

#### Scenario: Query file is missing from the assembly
- **WHEN** a reader or service attempts to load a `.sql` resource file that is not embedded
- **THEN** the system throws an `InvalidOperationException` with a message identifying the missing resource name

---

### Requirement: Wishlist items reference reward items via a soft (unconstrained) link
The `wishlist_items.part_normalized_name` column SHALL reference `reward_items.normalized_name` by value only. No foreign key constraint SHALL be defined between these two columns.

#### Scenario: Wishlist item exists for a part not in the current drop table
- **WHEN** a wishlist item's `part_normalized_name` has no matching row in `reward_items`
- **THEN** the item is returned with `IsGhost = true` and is not deleted

#### Scenario: Drop table import clears and rebuilds reward_items
- **WHEN** a drop table import runs and deletes all rows from `reward_items` then reinserts them
- **THEN** all wishlist items are preserved and ghost status is re-evaluated against the new data

---

### Requirement: EF entity model is internal to Infrastructure
EF entity classes and `ReliquaryDbContext` SHALL be declared `internal` and SHALL NOT be referenced from the Application, Presentation, or Composition projects. All port implementations in Infrastructure SHALL map between EF entities and Application domain records at the boundary.

#### Scenario: Port implementation returns a domain record
- **WHEN** `IWishlistStore.GetAllAsync` is called
- **THEN** the implementation queries via EF entities and returns `WishlistEntry` domain records; no EF type crosses the Infrastructure boundary

---

### Requirement: Snapshot tables are cleared atomically before re-import
The import operation SHALL delete all rows from snapshot tables using set-based deletion (`ExecuteDeleteAsync`) within a transaction, in FK-dependency order, before inserting new data. User data tables (`wishlists`, `wishlist_items`, `user_preferences`) SHALL NOT be cleared by the import operation.

#### Scenario: Import completes successfully
- **WHEN** a drop table import runs to completion
- **THEN** all snapshot tables contain only data from the new import, and all wishlist data is unchanged

#### Scenario: Import fails mid-flight
- **WHEN** an error occurs during the insert phase of an import
- **THEN** the transaction is rolled back, snapshot tables retain their pre-import state, and wishlist data is unchanged
