## Why

The downloaded drop-table HTML is stored at a fixed filename and overwrites the previous copy on every refresh, and its provenance (source URL, download time, Last Update) is kept in a JSON sidecar alongside the HTML file. This means a new release silently discards the previous data, and provenance lives outside the database. Storing both the HTML content and its provenance in the DB eliminates filesystem side effects and lets the app retain every version it has ever downloaded.

## What Changes

- A new `source_files` SQLite table replaces both the raw HTML file and the JSON sidecar. One row is inserted per successful download, storing the full HTML content, source URL, download timestamp, and Last Update value.
- The JSON sidecar (`warframe-drop-tables.metadata.json`) is eliminated. All provenance reads and writes go to the DB.
- The HTML file (`warframe-drop-tables.html`) is no longer written to disk. The import workflow reads HTML content from the `source_files` DB table instead of the filesystem.
- The source-refresh status reads provenance from the DB instead of the JSON file.
- A new Export button on the Drop Tables tab lets the user write all stored source versions from the DB to HTML files in a folder of their choice.
- The Drop Tables tab gains a second source-acquisition button: **"Read from file"** opens a file picker, reads the selected HTML, extracts the Last Update value, and stores the content in `source_files` — identical to a URL download except the source is local. The existing Download button is renamed **"Download from URL"**. The separate Import button (parse and persist) is unchanged.

## Capabilities

### New Capabilities

- `drop-table-source-files`: DB-backed storage of all downloaded drop-table source HTML; one row per download with full content and provenance; replaces both the raw HTML file and the JSON provenance sidecar; enables retaining historical versions without any filesystem management
- `drop-table-source-export`: UI action and application service that writes every `source_files` DB row to a user-selected folder as individual date-stamped HTML files
- `drop-table-local-file-import`: Application workflow and UI action for registering a locally selected HTML file into `source_files`, using the same Last Update extraction and DB insert as the URL download path

### Modified Capabilities

- `drop-table-source-refresh`: Downloaded content stored in the `source_files` DB table instead of disk; Download button renamed to "Download from URL"
- `drop-table-parsing-import`: Reads source HTML from `source_files` DB table instead of a file path; source provenance read from DB

## Impact

- `LocalDropTableSourceStore` — remove all HTML file and JSON sidecar read/write; insert full HTML content into `source_files` on download; query `source_files` on load
- `DropTableSourceProvenance` application model — remains, populated from DB
- `IDropTableSourceStore` — `LoadAsync` returns HTML content from DB; no file path needed
- `IDropTableSourceExporter` (new port) — `ExportAllAsync(string folderPath)` contract; Infrastructure implementation reads all `source_files` rows and writes HTML files
- `IDropTableLocalFileSourceWorkflow` (new application workflow) — `RegisterFromFileAsync(string filePath)` reads the file, extracts Last Update, inserts into `source_files`
- Drop Tables tab UI — "Download from URL" button (renamed); new "Read from file" button with file picker; existing Import button unchanged
- `ReliquaryDbContext` / EF Core migration — new `source_files` entity and migration
- `SqliteDropTableImportStore` — receives HTML content from `IDropTableSourceStore.LoadAsync` (already the case via the store abstraction; no direct filesystem reference expected)
- `import_metadata.SourceFilePath` — no longer meaningful once content is in DB; left null or deprecated
- `DropTableSourceDefaults` — filename constants removed or made irrelevant
- Drop Tables tab UI — new Export button; Avalonia folder picker dialog; progress/result feedback
- Existing `*.html` and `*.metadata.json` files on disk are harmless leftovers; not migrated
