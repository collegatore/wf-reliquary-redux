## MODIFIED Requirements

### Requirement: Schema is managed by EF Core migrations
The system SHALL use EF Core code-first migrations as the sole mechanism for creating and evolving the SQLite database schema. No DDL strings, `CREATE TABLE IF NOT EXISTS` statements, or PRAGMA introspection SHALL exist in application code.

Database creation and migration SHALL happen at exactly one point: an `IDatabaseInitializer.InitializeAsync` operation invoked during application startup, before any view model loads data. The initializer SHALL create the database directory and file when missing and apply all pending migrations. Store and reader implementations SHALL NOT apply migrations, create the database file or directory, or check for the database file's existence; they SHALL assume the schema is current and treat a freshly created database as empty tables.

#### Scenario: First launch with no database
- **WHEN** the application starts and no database file exists
- **THEN** the startup initializer creates the database directory and file and applies all migrations before any data access occurs

#### Scenario: Launch with an outdated database
- **WHEN** the application starts and the database exists but is behind the current migration history
- **THEN** the startup initializer applies pending migrations before any data access occurs

#### Scenario: Launch with an up-to-date database
- **WHEN** the application starts and the database is already at the current migration version
- **THEN** no schema changes are made and startup proceeds normally

#### Scenario: Reads against a freshly created database
- **WHEN** a store or reader queries the database immediately after first-launch initialization, before any data has been written
- **THEN** the query runs against empty tables and returns empty/`null` results without any file-existence check

#### Scenario: Stores contain no schema management
- **WHEN** any store or reader method executes
- **THEN** it performs no migration, no directory or file creation, and no database-file existence check

## ADDED Requirements

### Requirement: Startup initialization failure is reported
When `IDatabaseInitializer.InitializeAsync` fails during application startup, the system SHALL report the failure through the application's error-reporting mechanism and SHALL continue running; subsequent data-access failures are reported through their existing error paths.

#### Scenario: Migration fails at startup
- **WHEN** the startup initializer throws while applying migrations
- **THEN** the error is reported via the error reporter and the application window still opens
