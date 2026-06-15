## Context

The current download pipeline writes two files to `%LocalAppData%\WF Reliquary Redux\DropTables\`:
- `warframe-drop-tables.html` — the raw source, overwritten on every refresh
- `warframe-drop-tables.metadata.json` — a JSON sidecar holding `DropTableSourceProvenance` (SourceUrl, DownloadedAt, SourceLastUpdate)

`LocalDropTableSourceStore` writes both files on save and reads both on load. `SqliteDropTableImportStore` receives the HTML content and provenance via `IDropTableSourceStore.LoadAsync` and writes provenance into the singleton `import_metadata` row.

The fixed filename means every refresh silently discards the previous HTML. The JSON sidecar means provenance lives outside the DB. There is no way to load a source file from the local filesystem, and no way to recover a previously stored version.

## Goals / Non-Goals

**Goals:**
- Store raw HTML content directly in a new `source_files` SQLite table — one row per acquisition
- Eliminate both the on-disk HTML file and the JSON sidecar; the DB is the sole source of truth
- Support two acquisition paths that both produce a `source_files` row: "Download from URL" and "Read from file" (local file picker)
- The import workflow reads HTML from the DB unchanged
- Export all stored versions to a user-selected folder as individual date-stamped HTML files

**Non-Goals:**
- Keeping HTML files on disk (intentionally removed from the download path)
- Surfacing historical versions in the UI (version list, rollback, diffing)
- Migrating existing on-disk HTML or JSON sidecar files into the DB
- Compressing stored HTML content
- Deduplicating rows when the same version is acquired more than once

## Decisions

### New `source_files` DB table with `content` column

A new EF Core entity `SourceFileEntity` maps to a `source_files` table:

| column | type | notes |
|--------|------|-------|
| `id` | INTEGER PK autoincrement | |
| `content` | TEXT NOT NULL | full raw HTML |
| `source_url` | TEXT NOT NULL | `file://` URI for local-file acquisitions; `https://` URI for downloads |
| `downloaded_at` | TEXT | ISO 8601 timestamp |
| `source_last_update` | TEXT NOT NULL | version string extracted from HTML |

One row is inserted per successful acquisition. The most recent row (highest `id`) is always "current" for status display and import. The table is a user-data table — it is never cleared by the import operation.

SQLite stores TEXT as UTF-8 with no practical size limit beyond ~1 GB per DB file. At ~1–2 MB per file, a decade of weekly Warframe updates would total well under 1 GB.

**Alternative considered:** store content as BLOB. Rejected — the content is UTF-8 text (HTML); TEXT avoids encoding overhead at the SQLite boundary.

**Alternative considered:** reuse `import_metadata` for download provenance. Rejected — `import_metadata` is written at import time; conflating the two would erase download provenance whenever a re-import runs without a new download.

### `LocalDropTableSourceStore` takes a `ReliquaryDbContext` dependency

`LocalDropTableSourceStore` lives in Infrastructure alongside `ReliquaryDbContext`. The context is injected via constructor and used for all `source_files` reads and writes.

**Alternative considered:** introduce a new `ISourceFilesRepository` port. Rejected — `source_files` is a DB-internal concern with no application-layer behavior; the extra port would be premature abstraction.

### `IDropTableSourceStore.LoadAsync` returns content from the most recent `source_files` row

`LoadAsync` queries `source_files ORDER BY id DESC LIMIT 1` and returns the `content` plus provenance fields. If no row exists it returns the "no source" sentinel as before — no file read involved.

For status-only queries (provenance without HTML), a separate projected query selects only the metadata columns to avoid loading megabytes of HTML unnecessarily.

### Two acquisition paths: "Download from URL" and "Read from file"

Both paths produce the same outcome — a new `source_files` row — and share the same Last Update extraction step via `IDropTableMetadataReader`:

- **"Download from URL"**: existing `DropTableSourceRefreshService` downloads HTML via `IDropTableSourceClient`, extracts Last Update, and calls `IDropTableSourceStore.SaveAsync`. `source_url` is set to the configured source URL.
- **"Read from file"**: new `IDropTableLocalFileSourceWorkflow.RegisterFromFileAsync(string filePath)` reads the file from disk, extracts Last Update via `IDropTableMetadataReader`, and inserts a `source_files` row. `source_url` is stored as a `file://` URI derived from the file path to indicate local origin.

If Last Update extraction fails on either path, no row is inserted and the operation reports a failure.

The Presentation layer is responsible for opening the file picker (`StorageProvider.OpenFilePickerAsync()` for "Read from file") and passing the resolved path to the workflow. The application layer never references Avalonia APIs.

### Export: new `IDropTableSourceExporter` port

Export is orchestrated by the application layer but executed by Infrastructure (filesystem writes). A new port `IDropTableSourceExporter` exposes a single method `ExportAllAsync(string destinationFolderPath, CancellationToken)` returning a result with the file count or an error message.

The Infrastructure implementation reads all `source_files` rows (content + provenance), then writes each to `{folder}\warframe-drop-tables-{source_last_update}.html`. When `source_last_update` is null, the fallback filename uses the `downloaded_at` timestamp formatted as `yyyyMMddTHHmmss`. Files are written sequentially; if any write fails the operation stops and reports the error, leaving already-written files in place.

The folder picker (`StorageProvider.OpenFolderPickerAsync()`) lives in Presentation; the resolved path is passed into `IDropTableSourceExporter.ExportAllAsync`.

**Alternative considered:** let Presentation read the DB and write files directly. Rejected — bypasses layer boundaries and duplicates Infrastructure concerns.

### `HasStoredSources` drives enabled state for Export and Import buttons

The Drop Tables view model tracks a `HasStoredSources` bool derived from whether `source_files` has at least one row. It is evaluated on load and refreshed after each successful acquisition (URL download or file read). The Export button and the existing Import button both bind `IsEnabled` to `HasStoredSources`.

### `import_metadata.SourceFilePath` is deprecated

`SourceFilePath` was designed to hold the path to the on-disk HTML file. Since content now lives in the DB there is no meaningful path to store. New imports will leave the field null. A future migration can drop the column; leaving it nullable adds no harm.

### JSON sidecar and HTML file are abandoned, not migrated

Existing `*.html` and `*.metadata.json` files are left on disk harmlessly. On first run after the update the user must re-acquire once (via "Download from URL" or "Read from file") to populate `source_files`.

## Risks / Trade-offs

- **Re-acquisition required on first upgrade** — users with a previously downloaded file will need to download or re-import once. Acceptable: it is a single-click operation.
- **DB file growth** — each acquisition adds ~1–2 MB. With infrequent Warframe updates accumulation is slow. No retention policy is introduced here; it can be added later.
- **Content column loaded by default** — EF will pull the full `content` column unless projected away. Status and provenance queries must use column projections; only the import and export paths should load `content`.

## Migration Plan

1. Add EF Core migration adding `source_files` table.
2. Mark `source_files` as a user-data table (excluded from snapshot clear in `SqliteDropTableImportStore`).
3. Update `LocalDropTableSourceStore`: remove HTML file and JSON sidecar read/write; insert into `source_files` on save; query `source_files` (with projection for metadata-only, full load for import) on load.
4. Add `IDropTableLocalFileSourceWorkflow` and its implementation; register in `ReliquaryServiceFactory`.
5. Add `IDropTableSourceExporter` and its implementation; register in `ReliquaryServiceFactory`.
6. Update `ReliquaryServiceFactory` to inject `ReliquaryDbContext` into `LocalDropTableSourceStore`.
7. Update Presentation: rename "Download drop tables" button to "Download from URL"; add "Read from file" button; add Export button; wire `HasStoredSources`.
8. No data migration; existing users re-acquire once.
