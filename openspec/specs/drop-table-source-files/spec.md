# drop-table-source-files Specification

## Purpose

Defines the DB-backed storage model for raw drop-table HTML content. The `source_files` table is the single authoritative store for all acquired drop-table sources, regardless of whether they were downloaded from a URL or imported from a local file.

## Requirements

### Requirement: Downloaded Source Content DB Record

The system SHALL insert one row into the `source_files` table for each successful drop-table download. The row SHALL store the full raw HTML content, source URL, download timestamp, and the source Last Update value. Rows are never deleted by the import operation.

#### Scenario: Download succeeds and record is inserted

- **WHEN** a drop-table HTML file is downloaded successfully with a parseable Last Update value
- **THEN** a row is inserted into `source_files` with the full HTML content, source URL, ISO 8601 download timestamp, and source Last Update value

#### Scenario: Source file records are preserved across imports

- **WHEN** a drop-table import runs
- **THEN** all rows in `source_files` remain unchanged

### Requirement: Current Source Content Resolution

The system SHALL resolve the current downloaded source content by selecting the most recently inserted row from the `source_files` table. When no row exists, the system SHALL report that no downloaded source is available.

#### Scenario: Most recent download is selected

- **WHEN** the system resolves the current source content and multiple rows exist in `source_files`
- **THEN** the row with the highest `id` is used to supply the HTML content and provenance

#### Scenario: No downloads recorded

- **WHEN** the system resolves the current source content and `source_files` has no rows
- **THEN** the system reports that no drop-table source has been downloaded (equivalent to a fresh install)
