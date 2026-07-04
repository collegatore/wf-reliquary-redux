## ADDED Requirements

### Requirement: Source Version Uniqueness Constraint

The `source_files` table SHALL enforce a UNIQUE index on the `source_last_update` column. No two rows MAY share the same `source_last_update` value. A EF Core migration SHALL deduplicate existing rows (keeping the row with the highest `id` per `source_last_update` value) before creating the index. The migration is applied by the application-startup database initializer (see the `ef-core-data-layer` capability), so deduplication takes effect before the first source list is read.

#### Scenario: Deduplication migration keeps latest row per version

- **WHEN** the database migration runs and `source_files` contains multiple rows with the same `source_last_update` value
- **THEN** all but the row with the highest `id` for that version are deleted before the UNIQUE index is created

#### Scenario: Unique index prevents duplicate version insert

- **WHEN** application code attempts to insert a row whose `source_last_update` matches an existing row
- **THEN** the database rejects the insert with a unique-constraint violation

#### Scenario: Upgraded database is deduplicated before the first read

- **WHEN** the application starts against a database containing duplicate-version rows and the source list is subsequently queried
- **THEN** the startup initializer has already applied the deduplication migration, and the query result contains at most one row per `source_last_update` value

### Requirement: Source Summary List Query

The system SHALL expose a `ListSummariesAsync` method on `IDropTableSourceStore` that returns lightweight summary records for all `source_files` rows ordered by descending `id`. Summaries SHALL include `Id`, `SourceUrl`, `DownloadedAt`, and `LastUpdate` but SHALL NOT load the `content` column.

#### Scenario: Summaries listed without HTML content

- **WHEN** `ListSummariesAsync` is called and rows exist
- **THEN** each returned `DropTableSourceSummary` contains metadata fields only and no HTML

### Requirement: Source Lookup by Last Update

The system SHALL expose a `FindSummaryByLastUpdateAsync(string lastUpdate)` method on `IDropTableSourceStore` that returns the matching `DropTableSourceSummary` when a row with that `source_last_update` value exists, or `null` when none exists.

#### Scenario: Existing version is found

- **WHEN** `FindSummaryByLastUpdateAsync` is called with a `lastUpdate` value that matches a row in `source_files`
- **THEN** the corresponding `DropTableSourceSummary` is returned

#### Scenario: No matching version

- **WHEN** `FindSummaryByLastUpdateAsync` is called with a `lastUpdate` value not present in `source_files`
- **THEN** `null` is returned

### Requirement: Source Load by ID

The system SHALL expose a `LoadByIdAsync(long id)` method on `IDropTableSourceStore` that returns the full `DropTableSourceContent` for the specified row, or `null` when no row with that `id` exists.

#### Scenario: Known ID loads content

- **WHEN** `LoadByIdAsync` is called with an `id` that exists in `source_files`
- **THEN** the full `DropTableSourceContent` including HTML is returned

#### Scenario: Unknown ID returns null

- **WHEN** `LoadByIdAsync` is called with an `id` not present in `source_files`
- **THEN** `null` is returned

### Requirement: Atomic Source Replacement

The system SHALL expose a `ReplaceAsync(long existingId, DropTableSourceContent newContent)` method on `IDropTableSourceStore` that atomically deletes the row with the specified `id` and inserts a new row for `newContent` in a single transaction. This operation is the only sanctioned path for overwriting an existing version.

#### Scenario: Existing row is replaced atomically

- **WHEN** `ReplaceAsync` is called with a valid `existingId`
- **THEN** the old row is deleted and a new row is inserted within a single transaction; no intermediate state is visible to concurrent readers

#### Scenario: Replace with unknown ID fails

- **WHEN** `ReplaceAsync` is called with an `existingId` not present in `source_files`
- **THEN** the operation fails with an error the caller can observe (the store throws), and no row is inserted
